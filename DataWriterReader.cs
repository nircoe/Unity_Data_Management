using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using UnityEngine;

namespace DataManagement
{
    public class DataWriterReader
    {
        readonly string fullPath;

        const string AesKey = "Aes_key";
        
        public DataWriterReader(string fileName)
        {
            fullPath = Social.localUser.authenticated 
                ? fileName
                : Path.Combine(Application.persistentDataPath, fileName);
        }
        
        #region File
        
        public GameData FileLoad()
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

        public bool FileSave(GameData data)
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
        
        #endregion
        
        #region Cloud

        bool isSave;
        byte[] toSave;
        GameData toLoad;
        
        public GameData CloudLoad()
        {
            isSave = false;
            try
            {
#if UNITY_ANDROID
                PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
                    fullPath,
                    DataSource.ReadCacheOrNetwork,
                    ConflictResolutionStrategy.UseLastKnownGood,
                    GoogleCloud);
#elif UNITY_IOS

#endif
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                Debug.Log("Load from the cloud succeed!");
            }

            return toLoad;
        }

        public bool CloudSave(GameData data)
        {
            try
            {
                isSave = true;
                toSave = GetByteDataForSave(data);
                if(toSave == null)
                    throw new Exception();
#if UNITY_ANDROID
                PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
                    fullPath,
                    DataSource.ReadCacheOrNetwork,
                    ConflictResolutionStrategy.UseLastKnownGood,
                    GoogleCloud);
#elif UNITY_IOS

#endif
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                Debug.Log("Save to the cloud succeed!");
            }
            
            return true;
        }

        #region Methods

        GameData GetGameDataForLoad(byte[] byteData)
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
        
        byte[] GetByteDataForSave(GameData data)
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

        #endregion
        
        #region Google
        
        void GoogleCloud(SavedGameRequestStatus status, ISavedGameMetadata metadata)
        {
            if(status == SavedGameRequestStatus.Success)
            {
                if(isSave)
                {
                    SavedGameMetadataUpdate metadataUpdate = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedDescription("update data file at " + DateTime.Now).Build();
                    PlayGamesPlatform.Instance.SavedGame.CommitUpdate(
                        metadata, metadataUpdate, toSave, GoogleCloudSave);
                }
                else
                {
                    PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(metadata, GoogleCloudLoad);
                }
            }
            else
            {
                Debug.LogError("Failed open google cloud");
                throw new Exception();
            }
        }

        void GoogleCloudSave(SavedGameRequestStatus status, ISavedGameMetadata metadata)
        {
            if(status == SavedGameRequestStatus.Success)
                Debug.Log("Succeed Save data on google cloud!");
            else
            {
                Debug.LogError("Failed to Commit update with google cloud");
                throw new Exception();
            }
        }
        
        void GoogleCloudLoad(SavedGameRequestStatus status, byte[] byteData)
        {
            if(status == SavedGameRequestStatus.Success)
            {
                toLoad = GetGameDataForLoad(byteData);
            }
            else
            {
                
                Debug.LogError("Failed load data from google cloud");
                throw new Exception();
            }
        }

        #endregion
        
        #endregion
    }
}

