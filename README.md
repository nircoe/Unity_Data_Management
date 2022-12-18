# Unity_Data_Management
Data Management for Unity, Encrypted (AES), Cloud + File management

Download the files or clone the Repo into your Game Project

To use it in your own Game Project, add this to every header of script that use the data management : 

         using DataManagement;

Include : 
          
          * class GameData - needs to modify to match your own game data
          
          * class DataManager - no need to change
          
          * class DataWriterReader - no need to change, abstract base class for specific Data Writer-Readers
          
          * class FileWriterReader - no need to change, inheret from DataWriterReader
          
          * class CloudWriterReader - no need to change, 
                  abstarct base class for specific Cloud Data Writer-Readers, inheret from DataWriterReader
          
          * class GoogleCloudWriterReader - no need to change, inheret from CloudWriterReader
          
          * class AppleCloudWriterReader - no need to change, inheret from CloudWriterReader
          
          * interface IDataManagement - no need to change, 
                  every script in your game that will need to write/read game data needs to implement this inteface
