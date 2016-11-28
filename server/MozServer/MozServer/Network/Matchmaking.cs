using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
namespace MozServer
{
    static class Matchmaking
    {
        public static LinkedList<Player> queuedPlayers= new LinkedList<Player>();
        public static Dictionary<int, Game> activeGames= new Dictionary<int, Game>();

        public static void EnqueuePlayer(Player p)
        {
            queuedPlayers.AddFirst(p);
            Console.WriteLine("queued player " + p.name);
            if (queuedPlayers.Count >= 2)
            {
                Player p1 = queuedPlayers.Last();
                queuedPlayers.RemoveLast();
                Player p2 = queuedPlayers.Last();
                queuedPlayers.RemoveLast();
                StartGame(p1, p2);
            }
        }
        public static void DequeuePlayer(Player p)
        {
            if (queuedPlayers.Remove(p))
                Console.WriteLine("removed player " + p.name + " from queue");
            else Console.WriteLine("cannot remove " + p.name + "from queue");
        }
        private static void StartGame(Player p1, Player p2)
        {
            int newId = 0;
            while (activeGames.ContainsKey(newId))
            {
                newId++;
            }
            Game game = new Game(newId, p1, p2);
            activeGames.Add(game.id, game);
            Server.Send(p1, "gameFound", "");
            Server.Send(p2, "gameFound", "");
            game.StartGame();
        }
        public static void EndGame(Game game)
        {
            activeGames.Remove(game.id);
        }
        public static void DisconnectPlayer(Player player)
        {
            DequeuePlayer(player);
            if (player.game != null)
            {
                player.game.ForceEndGame(player);
            }
        }
    }
}
