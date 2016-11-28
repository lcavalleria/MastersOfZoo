using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MozServer
{
    class Board
    {
        public Card[,] boardCards { get; private set; }

        public Board()
        {
            boardCards = new Card[3, 3];
        }
        public string Serialize()
        {
            string result = "";
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    if (boardCards[i, j] != null)
                        result += boardCards[i, j].Serialize();
                    else result += "null";
                    if(j*i<4)result += ":b:";
                }
            result.Remove(result.Length - 3);
            return result;
        }
        public int GetEmptyBoxesCount()
        {
            int count = 9;
            foreach (Card c in boardCards)
                if(c!= null) count--;
            return count;
        }
    }
}
