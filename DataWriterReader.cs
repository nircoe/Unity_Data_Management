using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace DataManagement
{
    public abstract class DataWriterReader
    {
        protected readonly string fullPath;

        protected const string AesKey = "Aes_key";

        protected DataWriterReader(string fileName)
        {
            fullPath = Social.localUser.authenticated 
                ? fileName
                : Path.Combine(Application.persistentDataPath, fileName);
        }

        public abstract GameData Load();

        public abstract bool Save(GameData data);

        protected static GameData GetGameDataForLoad(byte[] byteData)
        {
            if(!PlayerPrefs.HasKey(AesKey)) // succeed to read so AesKey needs to be stored in PlayerPrefs
                return null;
            GameData data;
            try
            { 
                byte[] savedKey = Convert.FromBase64String(PlayerPrefs.GetString(AesKey));
                
                using MemoryStream memoryStream = new MemoryStream(byteData);
                
                Aes outAes = Aes.Create();
                byte[] savedIv = new byte[outAes.IV.Length];
                if(memoryStream.Read(savedIv, 0, savedIv.Length) != savedIv.Length)
                    throw new Exception();
                
                using CryptoStream cryptoStream = new CryptoStream(
                    memoryStream,
                    outAes.CreateDecryptor(savedKey, savedIv),
                    CryptoStreamMode.Read);
                
                using BinaryReader binaryReader = new BinaryReader(cryptoStream, Encoding.Unicode);
                
                byte[] decryptedBytes = new byte[byteData.Length - savedIv.Length];
                if(binaryReader.Read(decryptedBytes, savedIv.Length, decryptedBytes.Length)
                   != decryptedBytes.Length)
                    throw new Exception();
                string text = Encoding.Unicode.GetString(decryptedBytes);
                data = JsonUtility.FromJson<GameData>(text);
            }
            catch (Exception)
            {
                Debug.LogError("failed to read");
                throw;
            }
            
            return data;
        }
        
        protected static byte[] GetByteDataForSave(GameData data)
        {
            using MemoryStream memoryAesStream = new MemoryStream();
            
            try
            {
                Aes inAes = Aes.Create();
                byte[] bytesKey = inAes.Key;
                string key = Convert.ToBase64String(bytesKey);
                PlayerPrefs.SetString(AesKey, key);
                byte[] inputIv = inAes.IV;
                memoryAesStream.Write(inputIv, 0, inputIv.Length);
                
                using CryptoStream cryptoStream = new CryptoStream(
                    memoryAesStream,
                    inAes.CreateEncryptor(inAes.Key, inAes.IV),
                    CryptoStreamMode.Write);
                
                using BinaryWriter binaryWriter = new BinaryWriter(cryptoStream, Encoding.Unicode);
                
                string jsonString = JsonUtility.ToJson(data);
                binaryWriter.Write(jsonString);
            }
            catch (Exception)
            {
                Debug.LogError("failed to write");
                throw;
            }

            return memoryAesStream.ToArray();
        }
    }
}

