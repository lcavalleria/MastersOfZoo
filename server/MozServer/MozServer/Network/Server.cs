using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using MozServer.Network;
using MozServer.MoZ;
namespace MozServer
{

    class Server
    {
        //private const string ip = "127.0.0.1"; // koding.io server: 10.0.7.242 - server de veritat: 127.0.0.2
        private const char DELIMITER = '|';
        private static Dictionary<string, Player> connectedPlayers;
        public const double turnTimer = 15000;
        private static Queue<KeyValuePair<StateObject,string>> commandsQueue;
        static void Main(string[] args)
        {
            bool connected = false;
            Console.WriteLine("connecting to database...");
            while (!connected)
            {
                try
                {
                    DatabaseConnection.Connect();
                    connected = true;
                }
                catch (MySql.Data.MySqlClient.MySqlException e)
                {
                    switch (e.Number)
                    {
                        case 0:
                            Console.WriteLine("ERROR: cannot connect to server");
                            break;
                        case 1042:
                            Console.WriteLine("ERROR: unable to resolve DNS");
                            break;
                        default:
                            Console.WriteLine("ERROR: " + e.ToString());
                            break;
                    }
                }
            }
            Console.WriteLine("connected");
            connectedPlayers = new Dictionary<string, Player>();
            commandsQueue = new Queue<KeyValuePair<StateObject, string>>();
            Thread listener = new Thread(StartListening);
            Thread commandProcesser = new Thread(ProcessCommands);
            listener.IsBackground = true;
            listener.Start();
            commandProcesser.IsBackground = true;
            commandProcesser.Start();
            inputCommands();
        }
        private static void inputCommands()
        {
            string[] line;
            line = Console.ReadLine().Split(' ');
            string command="";
            List<string> args=new List<string>();
            for(int i=0;i<line.Length;i++)
            {
                Console.WriteLine(line[i]);
                if (i == 0) command = line[i];
                else args.Add(line[i]);
            }
            while (command != "quit")
            {
                switch (command)
                {
                    case "players":
                        Console.WriteLine("\t" + connectedPlayers.Count + " connected players.");
                        Console.WriteLine("\t-Name - Info----------------------------------------");
                        foreach (Player p in connectedPlayers.Values)
                        {
                            Console.WriteLine("\t" + p.name + " - " + p.so.workSocket.RemoteEndPoint + "-" + Encoding.UTF8.GetString(p.so.buffer));
                        }
                        Console.WriteLine("\t----------------------------------------------------");
                        break;
                    case "queue":
                        Console.WriteLine("\t" + Matchmaking.queuedPlayers.Count + " queued players.");
                        Console.WriteLine("\t----------------------------------------------------");
                        foreach (Player p in Matchmaking.queuedPlayers)
                        {
                            Console.WriteLine("\t" + p.name);
                        }
                        Console.WriteLine("\t----------------------------------------------------");
                        break;
                    case "games":
                        Console.WriteLine("\t" + Matchmaking.activeGames.Count + " active games.");
                        Console.WriteLine("\tgameID\t\tplayer1\t\tplayer2");
                        Console.WriteLine("\t----------------------------------------------------");
                        foreach (Game g in Matchmaking.activeGames.Values)
                        {
                            Console.WriteLine("\t" + g.id + "\t\t" + g.gp1.player.name + "\t\t" + g.gp2.player.name);
                        }
                        Console.WriteLine("\t----------------------------------------------------");
                        break;
                    case "disconnect":
                        if (connectedPlayers.ContainsKey(args[0])) ;
                            DisconnectPlayer(connectedPlayers[args[0]].so, "disconnected from server terminal");
                        break;
                    default:
                        Console.WriteLine("available commands: 'players' , 'queue','games','quit','disconnect'");
                        break;
                }
                command = Console.ReadLine();
            }
        }
        // Thread signal.
        //public static ManualResetEvent connectDone = new ManualResetEvent(false);
        // public static ManualResetEvent receiveDone = new ManualResetEvent(false);
        static Socket listener;
        public static void StartListening()
        {
            byte[] bytes = new Byte[1024];

            //IPHostEntry ipHostInfo = Dns.Resolve(ip);
            IPAddress ipAddress = IPAddress.Any;//ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

               /* while (true)
                {
                    connectDone.Reset();*/
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                   /* connectDone.WaitOne();
                }*/

            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.ToString());
            }
        }

        public static void AcceptCallback(IAsyncResult ar)
        {

            Socket s = (Socket)ar.AsyncState;
                       StateObject state = new StateObject();
            state.workSocket = s.EndAccept(ar);
            /*state.workSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            connectDone.Set();
            while (true)
            {
                string playerName = "unknown";
                    foreach (Player p in connectedPlayers.Values)
                        if (p.so.workSocket == state.workSocket)
                            playerName = p.name;
                Console.WriteLine("waiting for permission from "+playerName +" to wait for more data");
                receiveDone.Reset();*/
                try
                {
                    //Console.WriteLine("waiting for " + playerName + " to send data...");
                    state.workSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                //Console.WriteLine("beginReceive called.");
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

            }
            catch (Exception ex)
                {
                    if (ex is NullReferenceException)
                    {
                        Console.WriteLine("ERROR: " + ex.ToString());
                    }
                    else if (ex is ObjectDisposedException)
                    {
                        Console.WriteLine("ERROR: trying to receive from disconnected socket ");
                    }
                    else if (ex is SocketException)
                    {
                        Console.WriteLine("ERROR: " + ex.ToString());
                    }
                    else Console.WriteLine("ERROR: " + ex.ToString());
                //break;            
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
            }

            //receiveDone.WaitOne();
            //}
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            Console.WriteLine("now in void ReadCallBack");//asdfsdfasdfasdfasdfasdfasdfasdf
            String content = String.Empty;
            StateObject state = (StateObject)ar.AsyncState;
            int bytesRead;
            try { bytesRead = state.workSocket.EndReceive(ar); }
            catch (Exception ex)
            {
                bytesRead = 0;
                if (ex is ObjectDisposedException)
                {
                    Console.WriteLine("ERROR: tried to read from a disconnected socket.");
                }
                else
                    Console.WriteLine("ERROR: " + ex.ToString());
            }
            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));
                Console.WriteLine(state.sb.ToString());//asdfasdfasdfasdfasdf
                content = state.sb.ToString();
                if (content.IndexOf(DELIMITER) > -1)
                {
                    Console.WriteLine("Read {0} bytes from {1}.", content.Length, state.player == null ? "socket" : state.player.name);
                    if (content[0] == '/')
                    {
                        commandsQueue.Enqueue(new KeyValuePair<StateObject, string>(state, content));
                    }
                   /* else
                    {
                        state.workSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    }*/
                }
                state.sb.Clear();
                state.workSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
            //receiveDone.Set();

            /* else
             {
                 if (state.player == null)
                     DisconnectSocket(state.workSocket, "no data");
                 else DisconnectPlayer(state, "no data");
             }*/
        }
        private static void ProcessCommands()
        {
            KeyValuePair<StateObject, string> current;
            while (true)
            {
                if (commandsQueue.Count > 0)
                {
                    Console.WriteLine("found commands waiting to be processed in queue, processing");
                    current = commandsQueue.Dequeue();
                    StateObject state = current.Key;
                    string content = current.Value;
                    string command = content.Substring(1, content.IndexOf(' ') - 1);
                    string message = content.Substring(command.Length + 1, content.IndexOf(DELIMITER) - command.Length - 1).TrimStart(' ');
                    Console.WriteLine("command: " + command + ", message: " + message);
                    switch (command)
                    {
                        case "beep":
                            Send(state.player, "boop", "");
                            break;
                        case "login":
                            {
                                string[] info = message.Split(',');
                                LoginResults login = CheckLogin(info[0], info[1], state);
                                switch (login)
                                {
                                    case LoginResults.Success:
                                        bool alreadyConnected = false;
                                        if (connectedPlayers.ContainsKey(info[0])) alreadyConnected = true;
                                        if (alreadyConnected)
                                        {
                                            Console.WriteLine("player " + info[0] + " was already connected!");
                                            DisconnectPlayer(connectedPlayers[info[0]].so, "Already connected");
                                            Console.WriteLine("disconnected player " + info[0]);
                                        }
                                        ConnectPlayer(ref state, info[0]);
                                        Send(state.player, "login", "success");
                                        break;
                                    case LoginResults.NameIncorrect:
                                        Send(state.workSocket, "login", "name");
                                        DisconnectSocket(state.workSocket, "login failed.");
                                        break;
                                    case LoginResults.PasswordIncorrect:
                                        Send(state.workSocket, "login", "pass");
                                        DisconnectSocket(state.workSocket, "login failed.");
                                        break;
                                }
                                break;
                            }
                        case "create":
                            {
                                string[] info = message.Split(',');
                                CreateAccResults create = CheckCreateAcc(info[0], info[1], info[2], state);
                                switch (create)
                                {
                                    case CreateAccResults.Success:
                                        if (DatabaseConnection.InsertPlayer(info[0], info[1], info[2]) > 0)
                                            Send(state.workSocket, "create", "success");
                                        else goto default;
                                        break;
                                    case CreateAccResults.NameIncorrect:
                                        Send(state.workSocket, "create", "name");
                                        break;
                                    case CreateAccResults.NameExists:
                                        Send(state.workSocket, "create", "exists");
                                        break;
                                    case CreateAccResults.PasswordIncorrect:
                                        Send(state.workSocket, "create", "pass");
                                        break;
                                    case CreateAccResults.MailIncorrect:
                                        Send(state.workSocket, "create", "mail");
                                        break;
                                    default:
                                        Send(state.workSocket, "create", "fail");
                                        break;
                                }
                                DisconnectSocket(state.workSocket, "operation complete.");
                                break;
                            }
                        case "menu":
                            state.player = DatabaseConnection.GetPlayer(state.player.name, state);
                            Send(state.player, "playerInfo", state.player.Serialize());
                            break;
                        case "dc":
                            DisconnectPlayer(state, "Application closed");
                            break;
                        case "queue":
                            Matchmaking.EnqueuePlayer(state.player);
                            break;
                        case "deque":
                            Matchmaking.DequeuePlayer(state.player);
                            break;
                        case "decks":
                            Send(state.player, "decks", new DeckList(state.player).Serialize() + ":de:"+new CardCollection(state.player).Serialize());
                            break;
                        case "svDeck":
                            DatabaseConnection.SaveDeck(state.player,message);
                            break;
                        case "movement":
                            state.player.PlayMovement(message);
                            break;
                        default:
                            Send(state.player, "denied", content);
                            break;
                    }
                }
            }
        }
        /*   private static void SendCallback(IAsyncResult ar)
           {
               try
               {
                   Socket handler = (Socket)ar.AsyncState;
                   int bytesSent = handler.EndSend(ar);
               }
               catch (Exception ex)
               {
                   if (ex is ObjectDisposedException)
                       Console.WriteLine("ERROR: trying to send to disposed socket. skipping...");
                   else
                   Console.WriteLine(ex.ToString());
               }
           }*/

        enum LoginResults
        {
            Success,
            NameIncorrect,
            PasswordIncorrect
        }
        private static LoginResults CheckLogin(string name, string password, StateObject state)
        {
            if (DatabaseConnection.GetPlayer(name, password, state) != null)
            {
                return LoginResults.Success;
            }
            else if (DatabaseConnection.GetPlayer(name, state) != null)
            {
                return LoginResults.PasswordIncorrect;
            }
            else
            {
                return LoginResults.NameIncorrect;
            }
        }
        enum CreateAccResults
        {
            Success,
            NameIncorrect,
            NameExists,
            PasswordIncorrect,
            MailIncorrect
        }
        private static CreateAccResults CheckCreateAcc(string name, string password, string mail, StateObject state)
        {
            if (name.Contains(",") || name.Contains("-") || name.Contains(";") || name.Contains(".") || name.Contains(":") || name.Contains("'") || name.Contains("|"))
                return CreateAccResults.NameIncorrect;
            else if (DatabaseConnection.GetPlayer(name, state) != null)
                return CreateAccResults.NameExists;
            else if (password.Length < 4 || password.Contains("|") || password.Contains(",") || password.Contains("-") || password.Contains(";") || password.Contains(".") || password.Contains(":") || password.Contains("'"))//fix? pls
                return CreateAccResults.PasswordIncorrect;
            else if (!mail.Contains("@") || !mail.Contains("."))//fix pls
                return CreateAccResults.MailIncorrect;
            else return CreateAccResults.Success;
        }
        private static void ConnectPlayer(ref StateObject state, string name)
        {
            Player selected = DatabaseConnection.GetPlayer(name, state);
            state.player = selected;
            Console.WriteLine("got player '" + name + "' from database");
            connectedPlayers.Add(state.player.name, state.player);
            Console.WriteLine("connected player " + name);
        }
        private static void DisconnectPlayer(StateObject state, string reason)
        {
            Console.WriteLine("disconnecting player " + state.player.name + "...");
            Matchmaking.DisconnectPlayer(state.player);
            DisconnectSocket(state.workSocket, reason);
            connectedPlayers.Remove(state.player.name);
            string playerName = state.player.name;
            state = null;
            Console.WriteLine("player " + playerName + " disconnected");
        }
        private static void DisconnectSocket(Socket socket, string reason)
        {
            Send(socket, "dc", reason);
            if (socket.Connected)
                socket.Shutdown(SocketShutdown.Send);
            Console.WriteLine("closing socket...");
            lock (connectedPlayers)
            {
                socket.Close();
            }
            Console.WriteLine("socket closed.");
        }

        public static bool Send(Socket workSocket, string command, string message)
        {
            return Send(workSocket, "/" + command + " " + message + DELIMITER);
        }
        public static bool Send(Player player, string command, string message)
        {
            return Send(player.so.workSocket, "/" + command + " " + message + DELIMITER);
        }

        private static bool Send(Socket handler, String data)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            Console.WriteLine("sending \"" + data + "\" to  socket...");
            try {
                lock (handler)
                {
                    handler.Send(byteData, byteData.Length, SocketFlags.None);
                }
                Console.WriteLine("done");
                return true;
            }
            catch(Exception ex)
            {
                if (ex is ObjectDisposedException)
                    Console.WriteLine("ERROR: trying to send to disposed socket.");
                else
                Console.WriteLine("ERROR: " + ex);
                return false;
            }
        }
    }
}
