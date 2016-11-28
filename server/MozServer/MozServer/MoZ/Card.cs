using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MozServer
{
    public class Card
    {
        public int id { get; private set; }
        public string cardName;
        CardElements cardElement;
        CardClasses cardClass;
        public int[] values { get; private set; }
        float rarity;
        public Player owner { get; private set; }

        public Card()
        {
            cardName="FE01";
            cardElement = CardElements.Fire;
            cardClass = CardClasses.Felines;
            values = new int[4] { 1, 1, 1, 1 };
            rarity = 1f;
        }

        public Card(int id,string name, CardElements element, CardClasses race, int valueUp, int valueRight, int valueDown, int valueLeft, float rarity,Player owner)
        {
            this.id = id;
            cardName = name;
            cardElement = element;
            cardClass = race;
            values = new int[4] { valueUp, valueRight, valueDown, valueLeft };
            this.rarity = rarity;
            this.owner = owner;
        }
        public string Serialize()
        {
           string result = cardName + ":c:" + (int)cardElement + ":c:" + (int)cardClass + ":c:";
            foreach (int i in values)
                result += i + ":c:";
            result += owner.name;
            return result;
        }
        public void Capture(Player owner)
        {
            this.owner = owner;
        }
    }
}
