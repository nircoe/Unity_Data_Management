namespace DataManagement
{
    public interface IDataManagement
    {
        void LoadData(GameData data);
        
        void SaveData(ref GameData data);
    }
}
