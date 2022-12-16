using System;
using UnityEngine;

namespace DataManagement
{
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance { get; private set; }

        [SerializeField] string fileName = "game.data";

        public delegate void ActionRef<T>(ref T obj); 
        public event Action<GameData> OnLoad;
        public event ActionRef<GameData> OnSave;
        
        GameData gameData;
        DataWriterReader dataWriterReader;
        
        void Awake()
        {
            if(Instance == null) Instance = this;
            else Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            dataWriterReader = new DataWriterReader(fileName);
            LoadGame();
        }

        void NewGame()
        {
            gameData = new GameData();
        }
        
        void LoadGame()
        {
            gameData = Social.localUser.authenticated
                ? dataWriterReader.CloudLoad()
                : dataWriterReader.FileLoad();
            
            if(gameData == null)
                NewGame();

            OnLoad?.Invoke(gameData);
        }
        
        void SaveGame()
        {
            OnSave?.Invoke(ref gameData);

            bool success = Social.localUser.authenticated 
                ? dataWriterReader.CloudSave(gameData) 
                : dataWriterReader.FileSave(gameData);
            
            if(!success) 
                Debug.LogError("Save Data Failed");
        }
        
        void OnApplicationQuit()
        {
            SaveGame();
        }
    }
}

