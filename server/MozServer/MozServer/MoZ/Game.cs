using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MozServer
{

    public class Game
    {
        public const double turnTime = 15000;
        public int id { get; private set; }
        public GamePlayer gp1 { get; private set; }
        public GamePlayer gp2 { get; private set; }
        private Board board;
        private bool player1Turn;
        Timer turnTimer;
        public Game(int id, Player player1, Player player2)
        {
            turnTimer = new Timer(turnTime);
            turnTimer.Elapsed += OnTimeEnded;
            turnTimer.AutoReset = true;
            this.id = id;
            player1.game = this;
            player2.game = this;
            gp1 = new GamePlayer(player1);
            gp2 = new GamePlayer(player2);
            board = new Board();
        }

        public void StartGame()
        {
            if (new Random().NextDouble() >= 0.5) player1Turn = true;
            else player1Turn = false;
            turnTimer.Start();
            UpdateClients();
        }
        private void EndGame()
        {
            if (gp1.score > gp2.score)
            {
                SaveGameResult(gp1.player, gp2.player,false);
            }
            else if (gp2.score > gp1.score)
            { 
                SaveGameResult(gp2.player, gp1.player,false);
            }
            else
            {
                SaveGameResult(gp1.player, gp2.player, true);
             }
            turnTimer.Elapsed -= OnTimeEnded;
            gp1.player.game = null;
            gp2.player.game = null;
            Matchmaking.EndGame(this);
        }
        private void SaveGameResult(Player winner,Player loser,bool draw)
        {
            DatabaseConnection.SaveGameResult(winner, loser,draw);
            if (draw)
            {
                Server.Send(winner, "gameEnded", "draw");
                Server.Send(loser, "gameEnded", "draw");
            }
            else
            {
                Server.Send(winner, "gameEnded", "win");
                Server.Send(loser, "gameEnded", "lose");
            }
        }
        public void ForceEndGame(Player disconnected)
        {
            if (gp1.player == disconnected)
            {
                DatabaseConnection.SaveGameResult(gp2.player, gp1.player, false);
                Server.Send(gp2.player, "gameEnded", "win");
                gp2.player.game = null;
            }
            else
            {
                DatabaseConnection.SaveGameResult(gp1.player, gp2.player, false);
                Server.Send(gp1.player, "gameEnded", "win");
                gp1.player.game = null;
            }
            turnTimer.Elapsed -= OnTimeEnded;
        }

        private void UpdateClients()
        {
            Server.Send(gp1.player, "gameInfo", Serialize());
            Server.Send(gp2.player, "gameInfo", Serialize());
        }
        private string Serialize()
        {
            int turn;
            if (player1Turn) turn = 1;
            else turn = 0;
            return gp1.Serialize() + ":g:" + gp2.Serialize() + ":g:" + board.Serialize() + ":g:" + turn;
        }
        private void OnTimeEnded(Object source, ElapsedEventArgs e)
        {
            GamePlayer player;
            if (player1Turn) player = gp1;
            else player = gp2;
            int i = 0, j = 0;
            while (board.boardCards[i, j] != null)
            {
                if (i + 1 < 3) i++;
                else
                {
                    i = 0;
                    if (j + 1 < 3) j++;
                }
            }
            int cIndex = 0;
            while (player.hand[cIndex] == null)
            {
                cIndex++;
            }
            string movement = i + "," + j + "," + cIndex;
            Console.WriteLine("time out for " + player.player.name + ", playing "+cIndex+" to "+i+","+j);
            PlayMovement(player.player.name, movement);
        }
        public void PlayMovement(string playerName, string movement)
        {
            GamePlayer gp;
            if (gp1.player.name == playerName) gp = gp1;
            else gp = gp2;
            if (player1Turn && gp == gp1 || !player1Turn && gp == gp2)
            {
                string[] info = movement.Split(',');
                int posI = Convert.ToInt32(info[0]), posJ = Convert.ToInt32(info[1]), cardIndex = Convert.ToInt32(info[2]);
                Card card = gp.hand[cardIndex];
                if (board.boardCards[posI, posJ] == null)
                {
                    board.boardCards[posI, posJ] = card;
                    gp.hand[cardIndex] = null;
                    Card[] adjacents = new Card[4];

                    if (posI >= 1)
                        adjacents[0] = board.boardCards[posI - 1, posJ];
                    if (posJ < 2)
                        adjacents[1] = board.boardCards[posI, posJ + 1];
                    if (posI < 2)
                        adjacents[2] = board.boardCards[posI + 1, posJ];
                    if (posJ >= 1)
                        adjacents[3] = board.boardCards[posI, posJ - 1];

                    if (adjacents[0] != null && adjacents[0].owner != card.owner &&
                        adjacents[0].values[2] < card.values[0])
                    {
                        CaptureCard(adjacents[0], gp);
                    }
                    if (adjacents[1] != null && adjacents[1].owner != card.owner &&
                        adjacents[1].values[3] < card.values[1])
                    {
                        CaptureCard(adjacents[1], gp);
                    }
                    if (adjacents[2] != null && adjacents[2].owner != card.owner &&
                       adjacents[2].values[0] < card.values[2])
                    {
                        CaptureCard(adjacents[2], gp);
                    }
                    if (adjacents[3] != null && adjacents[3].owner != card.owner &&
                       adjacents[3].values[1] < card.values[3])
                    {
                        CaptureCard(adjacents[3], gp);
                    }

                    player1Turn = !player1Turn;
                    turnTimer.Stop();
                    turnTimer.Start();
                    UpdateClients();
                    if (board.GetEmptyBoxesCount() == 0)
                    {
                        EndGame();
                    }
                }
            }
        }

        private void CaptureCard(Card c, GamePlayer gp)
        {
            c.Capture(gp.player);
            if (c.owner == gp1.player)
            {
                gp1.ScorePoint();
                gp2.LosePoint();
            }
            else
            {
                gp2.ScorePoint();
                gp1.LosePoint();
            }
        }
    }
}
