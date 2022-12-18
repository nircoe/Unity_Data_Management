namespace DataManagement
{
    public abstract class CloudWriterReader : DataWriterReader
    {
        protected byte[] toSave;
        protected GameData toLoad;
        
        protected CloudWriterReader(string fileName) : base(fileName) { }

    }
}
