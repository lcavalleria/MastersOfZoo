using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using MozServer.Network;
using MozServer.MoZ;

namespace MozServer
{          
    static class DatabaseConnection
    {
        static string connectionString = "Server=xxx.xx.xxx.xxx;Port=3306;Database=moz;User ID=server;Password=xxxxxxxxxxxx;Pooling=false";//hid for github
        static IDbConnection dbcon;


        public static void Connect()
        {
            rand = new Random();//TEMPORAL
            dbcon = new MySqlConnection(connectionString);
            dbcon.Open();
        }

        public static int InsertPlayer(string name)
        {
            string sql = "INSERT INTO players (name) values ('" + name + "');";
            return ExecuteSQL(sql);
        }
        public static int InsertPlayer(string name,string password)
        {
            string sql = "INSERT INTO players (name,password) values ('" + name + "','" + password + "');";
            return ExecuteSQL(sql);
        }
        public static int InsertPlayer(string name,string password,string mail)
        {
            string sql = "INSERT INTO players (name,password,mail) values ('" + name + "','" + password + "','" + mail + "');";
            return ExecuteSQL(sql);
        }
        private static int ExecuteSQL(string sql)
        {
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = sql;
            return dbcmd.ExecuteNonQuery();
        }

        public static int DeletePlayer(int id)
        {
            IDbCommand dbcmd = dbcon.CreateCommand();
            string sql = "DELETE FROM players WHERE ID=" + id + ";";
            dbcmd.CommandText = sql;
            return dbcmd.ExecuteNonQuery();
        }
        public static List<Card> GetCollection(Player owner)
        {
            IDbCommand dbcmd = dbcon.CreateCommand();
            string sql = "select c.* from cards c join players_cards pc on c.ID = pc.card_ID join players p on p.ID = pc.player_ID where p.name = '" + owner.name + "' order by rarity asc;";
            dbcmd.CommandText = sql;
            IDataReader reader = dbcmd.ExecuteReader();
            List<Card> result = new List<Card>();
            while (reader.Read())
            {
                result.Add(new Card(Convert.ToInt32(reader["ID"]),
                      (string)reader["name"],
                      (CardElements)Convert.ToInt32(reader["element"]),
                      (CardClasses)Convert.ToInt32(reader["class"]),
                      Convert.ToInt32(reader["valueUp"]),
                      Convert.ToInt32(reader["valueRight"]),
                   Convert.ToInt32(reader["valueDown"]),
                  Convert.ToInt32(reader["valueLeft"]),
                  (float)Convert.ToDouble(reader["rarity"]), owner));
            }
            reader.Close();
            return result;
        }
        public static List<Deck> getDecks(Player owner)
        {
            IDbCommand dbcmd = dbcon.CreateCommand();
            string sql = "select d.* from decks d join players p on d.player_ID=p.ID where p.name='"+owner.name+"';";
            dbcmd.CommandText = sql;
            IDataReader reader = dbcmd.ExecuteReader();
            List<Deck> result = new List<Deck>();
            List<KeyValuePair<int, string>> decksInfo = new List<KeyValuePair<int, string>>();
            while (reader.Read())
            {
                decksInfo.Add(new KeyValuePair<int, string>(Convert.ToInt32(reader["ID"]), (string)reader["name"]));
            }
            reader.Close();
            foreach(KeyValuePair<int,string> kvp in decksInfo)
            {
                result.Add(new Deck(kvp.Value, getDeckCards(owner, kvp.Key)));
            }
            return result;
        }
        private static Card[] getDeckCards(Player owner ,int deckID)
        {
            IDbCommand dbcmd = dbcon.CreateCommand();
            string sql = "select c.* from cards c join decks_cards dc on c.ID = dc.card_ID join decks d on d.ID=dc.deck_ID join players p on p.ID=d.player_ID "+
                "where p.name = '" + owner.name + "' and d.ID = " + deckID + "; ";
            dbcmd.CommandText = sql;
            IDataReader reader = dbcmd.ExecuteReader();
            Card[] deckCards = new Card[Deck.DECK_SIZE];
            int i = 0;
            
            while (reader.Read())
            {
                deckCards[i] = (new Card(Convert.ToInt32(reader["ID"]),
                    (string)reader["name"],
                      (CardElements)Convert.ToInt32(reader["element"]),
                      (CardClasses)Convert.ToInt32(reader["class"]),
                      Convert.ToInt32(reader["valueUp"]),
                      Convert.ToInt32(reader["valueRight"]),
                      Convert.ToInt32(reader["valueDown"]),
                     Convert.ToInt32(reader["valueLeft"]),
                      (float)Convert.ToDouble(reader["rarity"]), 
                      owner));
                i++;
            }
            reader.Close();
            return deckCards;
        }
        public static int SaveDeck(Player owner,string deckStr)
        {
            string[] info=deckStr.Split(new string[] { ":d:" }, StringSplitOptions.None);
            List<Card> colCards =GetCollection(owner);
            List<Card> colUniques = new List<Card>();
            string currentName = "";
            for(int i=colCards.Count-1;i>=0;i--)
            {
                if (currentName != colCards[i].cardName)
                {
                    colUniques.Add(colCards[i]);
                    currentName = colCards[i].cardName;
                }
            }
            int[] deckIDs = new int[4];
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = "select ID from decks where player_ID =" + owner.id + ";";
            IDataReader reader = dbcmd.ExecuteReader();
            int j = 0;
            while (reader.Read())
            {
                deckIDs[j] = Convert.ToInt32(reader["ID"]);
                j++;
            }
            reader.Close();
            int deckIDIndex = Convert.ToInt32(info[0]);
            DeleteCardsFromDeck(deckIDs[deckIDIndex]);
            string sql = "INSERT INTO decks_cards VALUES ";
            for(int i = 1; i < info.Length; i++)
            {
                sql += "("+ deckIDs[deckIDIndex] + ","+colUniques[Convert.ToInt32(info[i])].id+"),";
            }
            sql =sql.Remove(sql.Length - 1) + ";";
            return ExecuteSQL(sql);
        }
        private static int DeleteCardsFromDeck(int deckID)
        {
            string sql = "DELETE FROM decks_cards WHERE deck_ID = " + deckID + ";";
            return ExecuteSQL(sql);
        }
        public static Player GetPlayer(string name, Network.StateObject so)
        {
            string sql = "SELECT * FROM players WHERE name='" + name + "';";
            return GetPlayerSQL(sql,so);
        }
        public static Player GetPlayer(string name, string password, Network.StateObject so)
        {
            string sql = "SELECT * FROM players WHERE name='" + name + "' AND password ='"+password+"';";
            return GetPlayerSQL(sql, so);
        }
        private static Player GetPlayerSQL(string sql, Network.StateObject so    )
        {
            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = sql;
            IDataReader reader = dbcmd.ExecuteReader();
            Player player;
            if (reader.Read())
            {
                player = new Player(Convert.ToInt32(reader["ID"]),
                    (string)reader["name"],
                     Convert.ToInt32(reader["gold"]),
                     Convert.ToInt32(reader["exp"]),
                     Convert.ToInt32(reader["cash"]),
                    Convert.ToInt32(reader["energy"]),
                    so);
            }
            else player = null;
            reader.Close();
            return player;
        }

        static Random rand;//TEMPORAL
        public static Card[] GetHand(Player owner)
        {
            //TODO: TEMPORAL!!!!
            Card[] hand = new Card[5];
            for(int i = 0; i < 5; i++)
            {
                hand[i]=new Card(1,"MA07",  CardElements.Fire, CardClasses.Felines, rand.Next(1, 12), rand.Next(1, 12), rand.Next(1, 12), rand.Next(1, 12),1f,owner);
            }
            return hand;
        }
        private static int UpdatePlayer(Player player, int goldAdd, int expAdd, int cashAdd, int energyAdd)
        {
            IDbCommand dbcmd = dbcon.CreateCommand();
            string sql = "UPDATE players SET exp=" + (player.exp + expAdd) + ",gold=" + (player.gold + goldAdd) + " WHERE name='" + player.name + "';";
            dbcmd.CommandText = sql;
            return dbcmd.ExecuteNonQuery();
        }

        public static void SaveGameResult(Player winner, Player loser,bool draw)
        {
            if (draw)
            {
                UpdatePlayer(winner, 25, 65, 0, 0);
                UpdatePlayer(loser, 25, 65, 0, 0);
            }
            else
            {
                UpdatePlayer(winner, 30, 100, 0, 0);
                UpdatePlayer(loser, 20, 40, 0, 0);
            }

        }
    }
}
