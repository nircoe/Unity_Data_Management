using System;
using Services;
using UnityEngine;
using UnityEngine.SceneManagement;

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

            NewWriterReader();
        }

        void Start()
        {
            LoadGame();
        }

        void NewGame()
        {
            gameData = new GameData();
        }
        
        void LoadGame()
        {
            gameData = dataWriterReader.Load();
            
            if(gameData == null)
                NewGame();

            OnLoad?.Invoke(gameData);
        }
        
        void SaveGame()
        {
            OnSave?.Invoke(ref gameData);

            bool success = dataWriterReader.Save(gameData);
            
            if(!success) 
                Debug.LogError("Save Data Failed");
        }

        #region Save Game Events
        
        void OnEnable()
        {
            SceneManager.activeSceneChanged += SaveGameBetweenScenes;
        }

        void OnDisable()
        {
            SceneManager.activeSceneChanged -= SaveGameBetweenScenes;
        }

        void SaveGameBetweenScenes(Scene arg0, Scene arg1)
        {
            if(arg0.buildIndex != 0) // more scenes if they are don't change any game data
                SaveGame();
        }
        
        void OnApplicationFocus(bool hasFocus)
        {
            if(!hasFocus)
                SaveGame();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if(pauseStatus)
                SaveGame();
        }

        void OnApplicationQuit()
        {
            SaveGame();
        }
        
        #endregion

        void NewWriterReader()
        {
            if(Social.localUser.authenticated)
            {
                if(Authentication.Instance.Platform == Platform.Google)
                    dataWriterReader = new GoogleCloudWriterReader(fileName);
                else
                    dataWriterReader = new AppleCloudWriterReader(fileName);
            }
            else
                dataWriterReader = new FileWriterReader(fileName);
        }
    }
}

