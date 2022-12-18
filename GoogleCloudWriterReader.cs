using System;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using UnityEngine;

namespace DataManagement
{
    public class GoogleCloudWriterReader : CloudWriterReader
    {
        bool isSave;
        
        public GoogleCloudWriterReader(string fileName) : base(fileName) { }

        public override GameData Load()
        {
            isSave = false;
            try
            {
                PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
                    fullPath,
                    DataSource.ReadCacheOrNetwork,
                    ConflictResolutionStrategy.UseLastKnownGood,
                    GoogleCloud);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                Debug.Log("Load from google cloud succeed!");
            }

            return toLoad;
        }

        public override bool Save(GameData data)
        {
            try
            {
                isSave = true;
                toSave = GetByteDataForSave(data);
                if(toSave == null)
                    throw new Exception();
                PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
                    fullPath,
                    DataSource.ReadCacheOrNetwork,
                    ConflictResolutionStrategy.UseLastKnownGood,
                    GoogleCloud);
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
        
        void GoogleCloud(SavedGameRequestStatus status, ISavedGameMetadata metadata)
        {
            if(status == SavedGameRequestStatus.Success)
            {
                if(isSave)
                {
                    SavedGameMetadataUpdate metadataUpdate = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedDescription("Update data file at " + DateTime.Now).Build();
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

        static void GoogleCloudSave(SavedGameRequestStatus status, ISavedGameMetadata metadata)
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
    }
}
