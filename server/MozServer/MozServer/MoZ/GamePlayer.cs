using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MozServer
{
    public class GamePlayer
    {
        public Card[] hand { get; private set; }
        public int score { get; private set; }
        public Player player;
        public GamePlayer(Player player)
        {
            this.player = player;
            hand= DatabaseConnection.GetHand(player);
            score = 5;
        }
        public string Serialize()
        {
            string result = player.name + ":gp:";
            for (int i=0;i<hand.Length;i++)
            {
                if (hand[i] != null)
                    result += hand[i].Serialize();
                else result += "null";
                if(i<4) result += ":h:";
            }
            result += ":gp:"+score;
            return result;
        }
        public void ScorePoint()
        {
            score++;
        }
        public void LosePoint()
        {
            score--;
        }
    }
}
