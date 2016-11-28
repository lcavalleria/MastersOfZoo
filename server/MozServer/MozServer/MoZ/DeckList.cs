using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MozServer.MoZ
{
    public class DeckList
    {
        public List<Deck> decks = new List<Deck>();
        public string Serialize()
        {
            string result = "";
            foreach (Deck d in decks)
            {
                result += d.Serialize();
                result += ":dl:";
            }
            result = result.Substring(0, result.Length - 4);
            return result;
        }
        public DeckList(Player player)
        {
            decks = DatabaseConnection.getDecks(player);
        }
    }
    public class Deck
    {
        public const int DECK_SIZE = 9;
        public string name;
        public Card[] cards;

        public string Serialize()
        {
            string result = name + ":d:";
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] != null)
                    result += cards[i].Serialize();
                if (i < cards.Length - 1) result += ":d:";
            }
            return result;
        }
        public Deck(/*int id,*/string name, Card[] cards)
        {
            /*this.id = id;*/
            this.name = name;
            this.cards = cards;
        }
    }
    class CardCollection
    {
        public List<Card> cards;
        public CardCollection(Player player)
        {
            cards = DatabaseConnection.GetCollection(player);
        }
        public string Serialize()
        {
            string result = "";
            foreach (Card c in cards)
            {
                result += c.Serialize();
                result += ":co:";
            }
            if(result.Length>4)
            result = result.Substring(0, result.Length - 4);
            return result;
        }
    }
}
