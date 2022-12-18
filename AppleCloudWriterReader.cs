using System;
using UnityEngine;

namespace DataManagement
{
    public class AppleCloudWriterReader : CloudWriterReader
    {
        //bool isSave;
        
        public AppleCloudWriterReader(string fileName) : base(fileName) { }

        public override GameData Load()
        {
            //isSave = false;
            try
            {
                // apple cloud load
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
                //isSave = true;
                toSave = GetByteDataForSave(data);
                if(toSave == null)
                    throw new Exception();
                // apple cloud save
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
    }
}
