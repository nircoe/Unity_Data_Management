using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace DataManagement
{
    public class FileWriterReader : DataWriterReader
    {
        public FileWriterReader(string fileName) : base(fileName) { }

        public override GameData Load()
        {
            if(!File.Exists(fullPath) || !PlayerPrefs.HasKey(AesKey))
                return null;
            GameData data;
            try
            {
                byte[] savedKey = Convert.FromBase64String(PlayerPrefs.GetString(AesKey));
                using FileStream dataStream = new FileStream(fullPath, FileMode.Open);
                Aes outAes = Aes.Create();
                byte[] savedIv = new byte[outAes.IV.Length];
                if(dataStream.Read(savedIv, 0, savedIv.Length) != savedIv.Length)
                    throw new Exception();
                using CryptoStream cryptoStream = new CryptoStream(
                    dataStream,
                    outAes.CreateDecryptor(savedKey, savedIv),
                    CryptoStreamMode.Read);
                using StreamReader streamReader = new StreamReader(cryptoStream);
                string text = streamReader.ReadToEnd();
                data = JsonUtility.FromJson<GameData>(text);
            }
            catch (Exception)
            {
                Debug.LogError("failed to read");
                return null;
            }
            
            return data;
        }

        public override bool Save(GameData data)
        {
            try
            {
                using FileStream dataAesStream = new FileStream(fullPath, FileMode.Create);
                Aes inAes = Aes.Create();
                byte[] bytesKey = inAes.Key;
                string key = Convert.ToBase64String(bytesKey);
                PlayerPrefs.SetString(AesKey, key);
                byte[] inputIv = inAes.IV;
                dataAesStream.Write(inputIv, 0, inputIv.Length);
                using CryptoStream cryptoStream = new CryptoStream(
                    dataAesStream,
                    inAes.CreateEncryptor(inAes.Key, inAes.IV),
                    CryptoStreamMode.Write);
                using StreamWriter streamWriter = new StreamWriter(cryptoStream);
                string jsonString = JsonUtility.ToJson(data);
                streamWriter.Write(jsonString);
            }
            catch (Exception)
            {
                return false;
            }
            
            return true;
        }
    }
}
