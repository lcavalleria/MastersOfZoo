using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MozServer.Network;

namespace MozServer
{
    public class Player
    {
        public int id { get; private set; }
        public string name { get; private set; } = "";
        public int gold { get; private set; }
        public int exp { get; private set; }
        public int cash { get; private set; }
        public int energy { get; private set; }
        public StateObject so { get; private set; }
        public Game game=null;

        public Player(Network.StateObject so)
        {
            this.so = so;
        }

        public Player(int id,string name,int gold,int exp,int cash,int energy, Network.StateObject so)
        {
            this.id = id;
            this.name = name;
            this.gold = gold;
            this.exp = exp;
            this.cash = cash;
            this.energy = energy;
            this.so = so;
        }
        public string Serialize()
        {
            string result =name+":p:"+gold+":p:"+exp+":p:"+cash+":p:"+energy;
            return result;
        }
        public void PlayMovement(string movement)
        {
            game.PlayMovement(name,movement);
        }
    }
}
