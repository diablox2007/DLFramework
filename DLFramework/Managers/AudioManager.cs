using UnityEngine;
using System.Collections.Generic;
using System;

namespace com.dl.framework
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        [Serializable]
        private class AudioPool
        {
            public int initSize = 5;
            public int maxSize = 10;
            private Queue<AudioSource> pool;
            private Transform parent;

            public void Initialize(Transform parent)
            {
                this.parent = parent;
                pool = new Queue<AudioSource>();

                // 预创建对象池
                for (int i = 0; i < initSize; i++)
                {
                    CreateNewAudioSource();
                }
            }

            private AudioSource CreateNewAudioSource()
            {
                GameObject go = new GameObject("AudioSource");
                go.transform.SetParent(parent);
                AudioSource source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                pool.Enqueue(source);
                return source;
            }

            public AudioSource Get()
            {
                if (pool.Count == 0 && pool.Count < maxSize)
                {
                    CreateNewAudioSource();
                }
                return pool.Count > 0 ? pool.Dequeue() : null;
            }

            public void Return(AudioSource source)
            {
                source.Stop();
                source.clip = null;
                source.transform.SetParent(parent);
                pool.Enqueue(source);
            }
        }

        [SerializeField] private AudioSource bgmSource; // BGM专用音源
        [SerializeField] private AudioPool sfxPool = new AudioPool(); // 音效对象池

        private float bgmVolume = 1f;
        private float sfxVolume = 1f;
        private Dictionary<AudioSource, float> activeEffects = new Dictionary<AudioSource, float>();
        private AudioClip currentBGM;

        protected override void OnInit()
        {
            // 创建BGM音源
            if (bgmSource == null)
            {
                GameObject bgmGo = new GameObject("BGM_Source");
                bgmGo.transform.SetParent(transform);
                bgmSource = bgmGo.AddComponent<AudioSource>();
                bgmSource.playOnAwake = false;
                bgmSource.loop = true;
            }

            // 初始化音效池
            sfxPool.Initialize(transform);

            // 从PlayerPrefs加载音量设置
            bgmVolume = PlayerPrefs.GetFloat("BGM_Volume", 1f);
            sfxVolume = PlayerPrefs.GetFloat("SFX_Volume", 1f);

            // 应用音量设置
            UpdateBGMVolume();

            base.OnInit();
        }

        private void Update()
        {
            // 检查并回收已播放完成的音效
            var finishedSources = new List<AudioSource>();
            foreach (var kvp in activeEffects)
            {
                if (!kvp.Key.isPlaying)
                {
                    finishedSources.Add(kvp.Key);
                }
            }

            foreach (var source in finishedSources)
            {
                activeEffects.Remove(source);
                sfxPool.Return(source);
            }
        }

        #region BGM控制
        public void PlayBGM(AudioClip clip, float fadeTime = 0.5f)
        {
            if (clip == currentBGM && bgmSource.isPlaying)
                return;

            currentBGM = clip;

            if (fadeTime > 0)
            {
                StartCoroutine(FadeBGM(clip, fadeTime));
            }
            else
            {
                bgmSource.clip = clip;
                bgmSource.volume = bgmVolume;
                bgmSource.Play();
            }
        }

        private System.Collections.IEnumerator FadeBGM(AudioClip newClip, float fadeTime)
        {
            // 淡出当前BGM
            if (bgmSource.isPlaying)
            {
                float startVolume = bgmSource.volume;
                float timer = fadeTime;
                while (timer > 0)
                {
                    timer -= Time.deltaTime;
                    bgmSource.volume = startVolume * (timer / fadeTime);
                    yield return null;
                }
                bgmSource.Stop();
            }

            // 设置并播放新BGM
            bgmSource.clip = newClip;
            bgmSource.volume = 0;
            bgmSource.Play();

            // 淡入新BGM
            float currentTime = 0;
            while (currentTime < fadeTime)
            {
                currentTime += Time.deltaTime;
                bgmSource.volume = bgmVolume * (currentTime / fadeTime);
                yield return null;
            }

            bgmSource.volume = bgmVolume;
        }

        public void StopBGM(float fadeTime = 0.5f)
        {
            if (fadeTime > 0)
            {
                StartCoroutine(FadeOutBGM(fadeTime));
            }
            else
            {
                bgmSource.Stop();
                currentBGM = null;
            }
        }

        private System.Collections.IEnumerator FadeOutBGM(float fadeTime)
        {
            float startVolume = bgmSource.volume;
            float timer = fadeTime;

            while (timer > 0)
            {
                timer -= Time.deltaTime;
                bgmSource.volume = startVolume * (timer / fadeTime);
                yield return null;
            }

            bgmSource.Stop();
            bgmSource.volume = bgmVolume;
            currentBGM = null;
        }
        #endregion

        #region SFX控制
        public AudioSource PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return null;

            AudioSource source = sfxPool.Get();
            if (source == null) return null;

            source.clip = clip;
            source.volume = sfxVolume * volumeScale;
            source.loop = false;
            source.Play();

            activeEffects[source] = volumeScale;
            return source;
        }

        public AudioSource PlaySFXLoop(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return null;

            AudioSource source = sfxPool.Get();
            if (source == null) return null;

            source.clip = clip;
            source.volume = sfxVolume * volumeScale;
            source.loop = true;
            source.Play();

            activeEffects[source] = volumeScale;
            return source;
        }

        public void StopSFX(AudioSource source)
        {
            if (source != null && activeEffects.ContainsKey(source))
            {
                source.Stop();
                activeEffects.Remove(source);
                sfxPool.Return(source);
            }
        }
        #endregion

        #region 音量控制
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("BGM_Volume", bgmVolume);
            UpdateBGMVolume();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("SFX_Volume", sfxVolume);
            UpdateSFXVolume();
        }

        private void UpdateBGMVolume()
        {
            if (bgmSource != null)
            {
                bgmSource.volume = bgmVolume;
            }
        }

        private void UpdateSFXVolume()
        {
            foreach (var kvp in activeEffects)
            {
                kvp.Key.volume = sfxVolume * kvp.Value;
            }
        }

        public float GetBGMVolume() => bgmVolume;
        public float GetSFXVolume() => sfxVolume;
        #endregion

        protected override void OnDestroy()
        {
            StopAllCoroutines();
            base.OnDestroy();
        }
    }
}
