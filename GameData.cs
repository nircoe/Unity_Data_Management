using UnityEngine.Serialization;

namespace DataManagement
{
    [System.Serializable]
    public class GameData
    {
        public int coins;
        public int record;
        
        public int power;
        public int speed;

        public int currentSpaceship;
        public int[] mySpaceships;

        public int currentBackground;
        public int[] myBackgrounds;

        public GameData()
        {
            this.coins = 0;
            this.record = 0;
            
            this.power = 0;
            this.speed = 1;
            
            this.currentSpaceship = 0;
            this.mySpaceships = new int[1];
            this.mySpaceships[0] = 0;
            
            this.currentBackground = 0;
            this.myBackgrounds = new int[1];
            this.myBackgrounds[0] = 0;
        }
    }
}


