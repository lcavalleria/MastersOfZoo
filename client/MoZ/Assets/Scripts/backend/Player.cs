using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public string playerName { get; private set; }

    //public Card[] deck { get; private set; }
    public Card[] hand { get; private set; }

    private Master master;
    public int score { get; private set; }
    public bool isBlue;
    private SpriteRenderer ScoreRenderer;
    private TextMesh NameMesh;
    private Sprite[] scoreValues;
    public int _selectedCardIndex = -1;


    // Use this for initialization
    void Start()
    {
        master = GameObject.Find("Master").GetComponent<Master>();
        ScoreRenderer = transform.Find("Score").GetComponent<SpriteRenderer>();
        ScoreRenderer.transform.localScale = new Vector3(3, 3, 3);
        NameMesh = transform.Find("NameLbl").GetComponent<TextMesh>();
        scoreValues = new Sprite[9];
        for (int i = 0; i < 9; i++)
        {
            scoreValues[i] = SpriteManager.GetSprite("value" + (i+1));
        }

    }
    public void UpdatePlayerState(string[] info)
    {
        playerName = info[0];
        string[] handInfo = info[1].Split(new string[] { ":h:" }, System.StringSplitOptions.None);
        score =System.Convert.ToInt32(info[2]);
        SetHand(handInfo);
        SetName();
        SetScore();
    }
    private void SetHand(string[] handInfo)
    {
        if (hand != null)
            foreach (Card c in hand)
                if(c!=null)
                Destroy(c.gameObject);
        hand = new Card[5];
        int i = 0;
        int cardIndex = 0;
        foreach (string s in handInfo)
        {
            if (s != "null")
            {
                Card c = Instantiate(MenuMaster.cardPrefab) as Card;
                c.Init();
                c.Deserialize(s,false);
                hand[i]=c;
                c.DrawSmallValues();
                if (c.bluePlayerOwned)
                    c.transform.position = new Vector3(8f, 3f - (1.7f * cardIndex), -10f - cardIndex);
                else
                    c.transform.position = new Vector3(-8f, 3f - (1.7f * cardIndex), -10f - cardIndex);
                cardIndex++;
            }
            i++;
        }
    }
    private void SetName()
    {
        NameMesh.text = playerName;
    }
    private void SetScore()
    {
        ScoreRenderer.sprite = scoreValues[score-1];
    }
    public int GetIndexOfCard(Card card)
    {
        for(int i = 0; i < 5; i++)
        {
            if (hand[i] == card) return i;
        }
        return -1;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
