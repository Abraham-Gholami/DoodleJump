using System;

namespace UGS
{
    [Serializable]
    public class Player
    {
        public string Username { get; set; }
        public DateTime LastUsernameChangeTime { get; set; }

        public Player(string newUsername)
        {
            Username = newUsername;
        }
    }
}