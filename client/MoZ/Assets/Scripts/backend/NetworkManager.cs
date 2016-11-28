using System;
using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections;

public class StateObject {
    public Socket workSocket = null;
    public const int BufferSize = 256;
    public byte[] buffer = new byte[BufferSize];
    public StringBuilder sb = new StringBuilder();
}

public class NetworkManager : MonoBehaviour
{
    Master master;
    public static string host;
    private const int port = 11000;
    private static Thread t;
    private static Queue<string> responses = new Queue<string>();
    private static Socket client;
    private const char DELIMITER = '|';
    public static string playerName;

    #region Responses reactions
    public static LoginResults loginResult = LoginResults.Unassigned;
    private static bool disconnecOk = false;
    private static bool connectionError = false;
    public static CreateAccResults createResult = CreateAccResults.Unassigned;
    #endregion

    private static void Connect()
    {

        try
        {
            IPHostEntry ipHostInfo = Dns.Resolve(host);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(remoteEP);
            Debug.Log("Socket connected to " + client.RemoteEndPoint.ToString());
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            connectionError = true;
        }
    }

    private static void Receive(Socket client)
    {
        try
        {
            while (true)
            {
                StateObject state = new StateObject();
                state.workSocket = client;
                int bytesRead = 0;
                try
                {
                    bytesRead = client.Receive(state.buffer);
                }
                catch (SocketException) { break; }
                bool foundEOF = false;
                while (bytesRead > 0 && !foundEOF)
                {
                    state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));
                    if (state.sb.ToString().IndexOf(DELIMITER) > -1)
                    {
                        foundEOF = true;
                        string[] msgs = state.sb.ToString().Split(DELIMITER);
                        foreach (string msg in msgs)
                        {
                            if (msg.Length > 1)
                            {
                                responses.Enqueue(msg);
                                Debug.Log("Enqueued message " + msg);
                            }
                        }
                    }
                    else
                    {
                        bytesRead = client.Receive(state.buffer);
                    }
                }
                foundEOF = false;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    public static void Send(string command, string message)
    {
        String data = "/" + command + " " + message + DELIMITER;
        byte[] byteData = Encoding.UTF8.GetBytes(data);
        try
        {
            Debug.Log("sending \"" + data + "\" to server...(" + System.DateTime.Now.TimeOfDay + ")");
            int bytesSent = client.Send(byteData);
            Debug.Log(bytesSent + " bytes sent");
        }
        catch (SocketException e)
        {
            if (e.ErrorCode == 10054)
            {
                Debug.LogError("RIP server. reinicia tot.");
            }
        }
    }

    public static void StartClient(string name, string password)
    {
        playerName = name;
        t = new Thread(() =>
        {
            Connect();
            Send("login", playerName + "," + password);
            Receive(client);//infinite loop
        });
        t.Start();
    }
    public static void CreateAccount(string name, string password, string mail)
    {
        playerName = name;
        t = new Thread(() =>
          {
              Connect();
              Send("create", playerName + "," + password + "," + mail);
              Receive(client);//infinite loop
          });
        t.Start();
    }

    void Update()
    {
        if (connectionError)
        {
            t.Abort();
            connectionError = false;
            LoginUI.ConnectionFailed();
            Debug.Log("Error connecting. engega el fucking server.");
        }
        if (responses != null && responses.Count > 0)
        {
            string response = responses.Dequeue();
            string command = response.Substring(1, response.IndexOf(' ') - 1);
            string message = response.Substring(command.Length + 1).TrimStart(' ');
            if (response[0] == '/')
            {
                switch (command)
                {
                    case "boop":
                        Debug.Log("boop received");
                        break;
                    case "login":
                        switch (message)
                        {
                            case "success":
                                loginResult = LoginResults.Success;
                                break;
                            case "name":
                                loginResult = LoginResults.NameIncorrect;
                                break;
                            case "pass":
                                loginResult = LoginResults.PasswordIncorrect;
                                break;
                        }
                        break;
                    case "create":
                        switch (message)
                        {
                            case "success":
                                createResult = CreateAccResults.Success;
                                client.Shutdown(SocketShutdown.Both);
                                client.Close();
                                break;
                            case "name":
                                createResult = CreateAccResults.NameIncorrect;
                                break;
                            case "exists":
                                createResult = CreateAccResults.NameExists;
                                break;
                            case "pass":
                                createResult = CreateAccResults.PasswordIncorrect;
                                break;
                            case "mail":
                                createResult = CreateAccResults.MailIncorrect;
                                break;
                            case "fail":
                                createResult = CreateAccResults.Fail;
                                break;
                        }
                        break;
                    case "playerInfo":
                        if (Application.loadedLevelName == "MainMenu")
                        {
                            GameObject mmCanvas = GameObject.Find("MainMenuCanvas");
                            if (mmCanvas != null) mmCanvas.GetComponent<MainMenuUI>().DeserializePlayer(message);
                            else Debug.Log("wtf algo raro");
                        }
                        else responses.Enqueue(response);
                        break;
                    case "gameFound":
                        Application.LoadLevel("Game");
                        break;
                    case "gameInfo":
                        if (Application.loadedLevelName == "Game")
                        {
                            if (master == null)
                                master = GameObject.Find("Master").GetComponent<Master>();
                            master.UpdateGameState(message);
                        }
                        else responses.Enqueue(response);
                        break;
                    case "decks":
                        if (Application.loadedLevelName == "MainMenu")
                        {
                            GameObject decksPnl = GameObject.Find("DecksPnl");
                            if (decksPnl != null) decksPnl.GetComponent<MenuDecks>().Deserialize(message);
                            else Debug.Log("wtf algo raro");
                        }
                        else responses.Enqueue(response);
                        break;
                    case "dc":
                        if (message == "Application closed")
                            Debug.Log("Disconnecting. Reason: " + message);
                        else if (message == "Already connected")
                            Debug.LogError("Disconnected. someone else connected using your credentials ");
                        disconnecOk = true;
                        break;
                    case "gameEnded":
                        switch (message)
                        {
                            case "win":
                                master.EndGame(GameResults.Win);
                                break;
                            case "lose":
                                master.EndGame(GameResults.Lose);
                                break;
                            case "draw":
                                master.EndGame(GameResults.Draw);
                                break;
                        }
                        break;
                }
            }
            else
            {
                Debug.Log("Process response " + response);
            }
        }
    }

    void OnApplicationQuit()
    {
        if (!Application.isEditor)
        {
            Application.CancelQuit();
            Quit();
        }
    }

    public void Quit()
    {
        StartCoroutine(WaitForDcResponse());
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }
    IEnumerator WaitForDcResponse()
    {
        Send("dc", playerName);
        client.Shutdown(SocketShutdown.Send);
        while (!disconnecOk) { yield return null; }
        Debug.Log("TANCANT SOCKET");
        client.Shutdown(SocketShutdown.Receive);
        client.Close();
        Debug.Log("SOCKET TANCAT");
        t.Abort();
    }
}
