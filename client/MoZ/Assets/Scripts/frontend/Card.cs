using UnityEngine;
using System;
using System.Collections;

public class Card : MonoBehaviour
{
    [SerializeField]
    public Master master;
    public bool faceup;
    public string cardName;
    public CardElements _cardElement;
    public CardClass _cardClass;
    public Rarity rarity;
    public int[] combatValues;
    public Sprite imageFront;
    public Sprite imageBack;
    public Sprite imageBackGround;
    public Sprite imageBorder;
    public SpriteRenderer[] imageValuesRenderer;
    public SpriteRenderer backGroundRenderer;
    public SpriteRenderer borderRenderer;
    public SpriteRenderer characterRenderer;
    public Transform imageValuesContainer;
    public CardBox cardBox;
    public bool bluePlayerOwned;
    public bool played;
    public bool selected;

    // Use this for initialization
    void Start()
    {
        
    }

    public void OnMouseEnter()
    {
        if (!played && faceup && bluePlayerOwned && !selected)
        {
            transform.localScale = new Vector3(1.2f, 1.2f, 1);
        }
    }
    public void OnMouseDown()
    {
        if(!played && faceup && bluePlayerOwned)
        {
            master.CardClicked(this);
        }
    }
    public void OnMouseExit()
    {
        if (!played && faceup && bluePlayerOwned && !selected)
        {
            transform.localScale = (new Vector3(1, 1, 1));
        }
    }
    //TODO els moviments de la z de select i deselect haurien de ser ABSOLUTS por siaca
    public void Select()
    {

            selected = true;
            transform.position = new Vector3(transform.position.x - 2, transform.position.y, transform.position.z - 20);
            transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            DrawBigValues();
        
    }
    public void Deselect()
    {
        if (bluePlayerOwned)
        {
            selected = false;
            transform.position = new Vector3(transform.position.x + 2, transform.position.y, transform.position.z + 20);
            transform.localScale = new Vector3(1, 1, 1);
            DrawSmallValues();
        }
    }


    public void FaceUp()
    {
        faceup = true;
        foreach (SpriteRenderer sr in imageValuesRenderer)
        {
            sr.GetComponent<Renderer>().enabled = true;
        }
        backGroundRenderer.sprite = imageBackGround;
        backGroundRenderer.enabled = true;
        borderRenderer.enabled = true;
    }
    public void FaceDown()
    {
        faceup = false;
        foreach (SpriteRenderer sr in imageValuesRenderer)
        {
            sr.GetComponent<Renderer>().enabled = false;
        }
        backGroundRenderer.sprite = imageBack;
        backGroundRenderer.enabled = false;
        borderRenderer.enabled = false;
    }
    public void Deserialize(string strCard,bool menuCard)
    {
        string[] info = strCard.Split(new string[] {":c:"},StringSplitOptions.None);
        cardName = info[0];
        _cardElement = (CardElements)Convert.ToInt32(info[1]);
        _cardClass = (CardClass)Convert.ToInt32(info[2]);
        combatValues = new int[4] { Convert.ToInt32(info[3]), Convert.ToInt32(info[4]), Convert.ToInt32(info[5]), Convert.ToInt32(info[6]) };
        if (!menuCard)
            bluePlayerOwned = info[7] == master._player.playerName;
        else bluePlayerOwned = true;
        rarity = RarityMethods.GetRarity(combatValues[0] + combatValues[1] + combatValues[2] + combatValues[3]);
        
        SetImageValues();
        SetImageFront();
        if (bluePlayerOwned)
            SetImageBackGround(CardBackgrounds.Blue);
        else SetImageBackGround(CardBackgrounds.Red);
        SetImageBorder();
    }
   public static string GetNameFromSerialized(string strCard)
    {
        string[] info = strCard.Split(new string[] { ":c:" }, StringSplitOptions.None);
        return info[0];
    }
    public void Init()
    {
        master = GameObject.Find("Master").GetComponent<Master>();
        selected = false;
        played = false;
        imageBack = SpriteManager.GetSprite("cardBack");
        imageBackGround = SpriteManager.GetSprite("cardFrontBg");
        imageValuesRenderer = new SpriteRenderer[4];
        backGroundRenderer = transform.Find("backGround").GetComponent<SpriteRenderer>();
        backGroundRenderer.sortingLayerName = "ForegroundLayer";
        borderRenderer = transform.Find("border").GetComponent<SpriteRenderer>();
        borderRenderer.sortingLayerName = "ForegroundLayer";
        characterRenderer = transform.Find("Character").GetComponent<SpriteRenderer>();
        characterRenderer.sortingLayerName = "ForegroundLayer";
        imageValuesContainer = transform.Find("combatValues");
        for (int i = 0; i < imageValuesRenderer.Length; i++)
        {
            imageValuesRenderer[i] = imageValuesContainer.GetChild(i).gameObject.GetComponent<SpriteRenderer>();
            imageValuesRenderer[i].sortingLayerName = "ForegroundLayer";
        }
        FaceUp();
        
    }

    public void Play()
    {
        played = true;
    }
    public void DrawSmallValues()
    {
        if (bluePlayerOwned)
            imageValuesContainer.localPosition = new Vector3(0, 0, 0);
        else
            imageValuesContainer.localPosition = new Vector3(-1, 0, 0);
        foreach (SpriteRenderer sr in imageValuesRenderer)
            sr.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        imageValuesRenderer[0].transform.localPosition = new Vector3(0.45f, 0.8f, -0.2f);
        imageValuesRenderer[1].transform.localPosition = new Vector3(0.8f, 0.5f, -0.2f);
        imageValuesRenderer[2].transform.localPosition = new Vector3(0.45f, 0.2f, -0.2f);
        imageValuesRenderer[3].transform.localPosition = new Vector3(0.1f, 0.45f, -0.2f);
    }
    public void DrawBigValues()
    {
        imageValuesContainer.localPosition = new Vector3(0, 0, 0);
        foreach (SpriteRenderer sr in imageValuesRenderer)
            sr.transform.localScale = new Vector3(1, 1, 1);
        imageValuesRenderer[0].transform.localPosition = new Vector3(0, 0.9f,-0.2f);
        imageValuesRenderer[1].transform.localPosition = new Vector3(0.9f,0, -0.2f);
        imageValuesRenderer[2].transform.localPosition = new Vector3(0, -0.9f, -0.2f);
        imageValuesRenderer[3].transform.localPosition = new Vector3(-0.9f, 0, -0.2f);
    }

    private void SetImageValues()
    {
        for (int i = 0; i < combatValues.Length; i++)
        {
            imageValuesRenderer[i].sprite = SpriteManager.GetSprite("value" + combatValues[i].ToString());
        }
    }
    public enum CardBackgrounds
    {
        Blue,
        Red,
        Black,
        Grey,
        Orange
    }
    public void SetImageBackGround(CardBackgrounds bg)
    {
        switch (bg)
        {
            case CardBackgrounds.Black:
                backGroundRenderer.color = new Color32(51, 51, 51,255);
                break;
            case CardBackgrounds.Grey:
                backGroundRenderer.color = new Color32(203, 213, 225, 255);
                break;
            case CardBackgrounds.Blue:
                backGroundRenderer.color = new Color32(75, 145, 234, 255);
                break;
            case CardBackgrounds.Red:
                backGroundRenderer.color = new Color32(168, 32, 61, 255);
                break;
            case CardBackgrounds.Orange:
                backGroundRenderer.color = new Color32(224,174,124,255);
                break;
        }
    }
    private void SetImageFront()
    {
        imageFront = SpriteManager.GetSprite(cardName);
        characterRenderer.sprite = imageFront;
    }

    private void SetImageBorder()
    {
        switch (rarity)
        {
            case Rarity.Basic:
                imageBorder = SpriteManager.GetSprite("borderBasic");
                break;
            case Rarity.Common:
                imageBorder = SpriteManager.GetSprite("borderCommon");
                break;
            case Rarity.Uncommon:
                imageBorder = SpriteManager.GetSprite("borderUncommon");
                break;
            case Rarity.Rare:
                imageBorder = SpriteManager.GetSprite("borderRare");
                break;
            case Rarity.Epic:
                imageBorder = SpriteManager.GetSprite("borderEpic");
                break;
            case Rarity.Heroic:
                imageBorder = SpriteManager.GetSprite("borderHeroic");
                break;
            case Rarity.Legendary:
                imageBorder = SpriteManager.GetSprite("borderLegendary");
                break;
        }
        borderRenderer.sprite = imageBorder;
    }
}
