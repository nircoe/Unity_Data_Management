# Unity_Data_Management
Data Management for Unity, Encrypted (AES), Cloud + File management

Download the files or clone the Repo into your Game Project

To use it in your own Game Project, add this to every header of script that use the data management : 

         using DataManagement;

Include : 
          
          * class GameData - needs to modify to match your own game data
          
          * class DataManager - no need to change
          
          * class DataWriterReader - no need to change
          
          * interface IDataManagement - no need to change, 
                  every script in your game that will need to write/read game data needs to implement this inteface
