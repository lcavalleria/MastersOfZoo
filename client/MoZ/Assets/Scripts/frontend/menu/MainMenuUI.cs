using UnityEngine;
using UnityEngine.UI;
using System;

public enum MenuWindows
{
    Main,
    Decks,
    Shop,
    Profile,
    Battle
}

public class MainMenuUI : MonoBehaviour
{
    [SerializeField]
    Image xpBar;
    [SerializeField]
    Text LblGold;
    [SerializeField]
    Text LblCash;
    [SerializeField]
    Text LblPlayerName;
    [SerializeField]
    Text LblLvl;
    Text LblBtnQueue;
    private bool inQueue;
    float queueTimer;
    private static double beepTimeMax = 60;
    private static double beepTime = 0;
    [SerializeField]
    private MenuDecks deckColPrefab;
    private GameObject currentWindow;
    public MenuWindows currentMenuWindow;

    // Use this for initialization
    void Start()
    {
        queueTimer = 0f;
        inQueue = false;
       
        NetworkManager.Send("menu", "");
    }
    public void DeserializePlayer(string playerStr)
    {
        string[] playerInfo = playerStr.Split(new string[] { ":p:" }, System.StringSplitOptions.None);
        NetworkManager.playerName = LblPlayerName.text = playerInfo[0];
        LblGold.text = playerInfo[1];
        LblCash.text = playerInfo[3];
        int totalExp = Convert.ToInt32(playerInfo[2]);
        int lvl = totalExp/1000+1;
        int exp = totalExp - (lvl-1)*1000;
        LblLvl.text = ""+lvl;
        xpBar.transform.localScale = new Vector3((float)exp  /1000f, 1, 1);
    }

    private void EnterQueue()
    {
        if (!inQueue)
        {
            NetworkManager.Send("queue", "");
            inQueue = true;
        }
        else
        {
            NetworkManager.Send("deque", "");
            inQueue = false;
            queueTimer = 0f;
            LblBtnQueue.text = "Play";
        }
    }

    private void OpenDecksPanel()
    {
        currentMenuWindow = MenuWindows.Decks;
        Destroy(currentWindow);
        currentWindow = Instantiate(deckColPrefab).gameObject;
        currentWindow.transform.SetParent(transform);
        currentWindow.name = currentWindow.name.Replace("(Clone)", "");
        NetworkManager.Send("decks", "");
    }
    public void MenuButton(Button btn)
    {
        if (btn.name == "BtnCards")
        {
            if (currentMenuWindow != MenuWindows.Decks)
                OpenDecksPanel();
        }
        else if (btn.name == "BtnBattle")
        {

        }
    }
    public void Quit()
    {
        GameObject.Find("Master").GetComponent<NetworkManager>().Quit();
    }
    // Update is called once per frame
    void Update()
    {
        float deltaTime = Time.deltaTime;
        if (inQueue)
        {
            queueTimer += deltaTime;
            LblBtnQueue.text = "Searching... (" + (int)queueTimer + ")";
        }

        if (beepTime < beepTimeMax)
            beepTime += deltaTime;
        else
        {
            beepTime = 0;
            NetworkManager.Send("beep", "");
        }
    }
}
