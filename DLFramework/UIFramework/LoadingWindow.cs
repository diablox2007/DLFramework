using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace com.dl.framework
{
	public class LoadingWindow : UIBase
	{
		[SerializeField] private Slider progressBar;
		[SerializeField] private TextMeshProUGUI tipText;
		[SerializeField] private TextMeshProUGUI progressText;

		protected override void OnInit()
		{
			base.OnInit();
			Layer = UILayer.Loading;
		}

		public void SetTip(string tip)
		{
			if (tipText != null)
			{
				tipText.text = tip;
			}
		}

		public void UpdateProgress(float progress)
		{
			if (progressBar != null)
			{
				progressBar.value = progress;
			}

			if (progressText != null)
			{
				progressText.text = $"{(progress * 100):F0}%";
			}
		}

		protected override void OnShow()
		{
			base.OnShow();
			UpdateProgress(0);
		}
	}
}
