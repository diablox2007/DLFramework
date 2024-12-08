using System;
using System.IO;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;

namespace com.dl.framework
{
    public static class SaveSystem
    {
        private static readonly string saveFolder = "SaveData";
        private static readonly string encryptionKey = "DLFramework2024"; // 加密密钥

        public static void SaveData<T>(string fileName, T data) where T : class
        {
            try
            {
                string folderPath = Path.Combine(Application.persistentDataPath, saveFolder);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string filePath = Path.Combine(folderPath, fileName + ".json");
                string jsonData = JsonUtility.ToJson(data);
                string encryptedData = EncryptString(jsonData);

                File.WriteAllText(filePath, encryptedData);
                DLLogger.Log($"Data saved successfully: {fileName}");
            }
            catch (Exception e)
            {
                DLLogger.LogError($"Failed to save data {fileName}: {e.Message}");
            }
        }

        public static T LoadData<T>(string fileName) where T : class, new()
        {
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, saveFolder, fileName + ".json");
                if (!File.Exists(filePath))
                {
                    DLLogger.LogWarning($"Save file not found: {fileName}");
                    return new T();
                }

                string encryptedData = File.ReadAllText(filePath);
                string jsonData = DecryptString(encryptedData);
                return JsonUtility.FromJson<T>(jsonData);
            }
            catch (Exception e)
            {
                DLLogger.LogError($"Failed to load data {fileName}: {e.Message}");
                return new T();
            }
        }

        public static void DeleteData(string fileName)
        {
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, saveFolder, fileName + ".json");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    DLLogger.Log($"Data deleted successfully: {fileName}");
                }
            }
            catch (Exception e)
            {
                DLLogger.LogError($"Failed to delete data {fileName}: {e.Message}");
            }
        }

        public static bool HasData(string fileName)
        {
            string filePath = Path.Combine(Application.persistentDataPath, saveFolder, fileName + ".json");
            return File.Exists(filePath);
        }

        private static string EncryptString(string text)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32, '*').Substring(0, 32));
                aes.IV = new byte[16];

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(text);
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public static string DecryptString(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32, '*').Substring(0, 32));
                aes.IV = new byte[16];

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
    }
}
