using System;
using System.Collections.Generic;
using UnityEngine;

public class Deck
{
    public const int DECK_SIZE = 9;
    public string name;
    public Card[] cards = new Card[DECK_SIZE];
    public MenuCardBox[,] deckBoxes;
    public MenuDecks col;
    public GameObject deckGameObject;
    public bool hasChanged = false;
    int id;
    public void BoxClicked(MenuCardBox box)
    {
        Card selected = col.col.GetSelectedCard();
        if (IsValidEdit(box.GetCard(), selected))
        {
            col.col.DeselectCard();
            if (box.GetCard() != null)
                MonoBehaviour.Destroy(box.GetCard().gameObject);
            Card c = MonoBehaviour.Instantiate(selected) as Card;
            c.transform.SetParent(box.transform);
            box.SetCard(c);
            hasChanged = true;
            col.deckButtons[4].interactable = true;
        }
    }

    public Deck(MenuDecks col, float margin, int id)
    {
        this.id = id;
        deckGameObject = new GameObject();
        deckGameObject.transform.SetParent(col.DeckCardsPnl);
        deckGameObject.transform.localPosition = new Vector2(0, 0);
        this.col = col;
        deckBoxes = new MenuCardBox[3, 3];
        int index = 0;
        for (int i = 0; i < deckBoxes.GetLength(0); i++)
            for (int j = 0; j < deckBoxes.GetLength(1); j++)
            {
                deckBoxes[i, j] = MonoBehaviour.Instantiate(col.cardBoxprefab) as MenuCardBox;
                deckBoxes[i, j].Init(col, this.id, index);
                deckBoxes[i, j].transform.SetParent(deckGameObject.transform);
                deckBoxes[i, j].transform.localPosition = new Vector3(1, 1, -1);
                deckBoxes[i, j].boxPosJ = j;
                deckBoxes[i, j].boxPosI = i;
                index++;
                deckBoxes[i, j].transform.Translate(new Vector2(
                    j * (deckBoxes[i, j].GetComponent<BoxCollider2D>().size.x + margin),
                    -i * (deckBoxes[i, j].GetComponent<BoxCollider2D>().size.y + margin)));
            }
        deckGameObject.name = name + " container";
        deckGameObject.SetActive(false);
    }

    public void Deserialize(string deckStr)
    {
        string[] deckInfo = deckStr.Split(new string[] { ":d:" }, StringSplitOptions.None);
        name = deckInfo[0];
        int i = 0;
        foreach (MenuCardBox db in deckBoxes)
        {
            if (db.GetCard() != null)
                MonoBehaviour.Destroy(db.GetCard().gameObject);
            if (deckInfo[i + 1] != "")
            {
                Card c = MonoBehaviour.Instantiate(MenuMaster.cardPrefab);
                c.Init();
                c.Deserialize(deckInfo[i + 1], true);
                c.DrawBigValues();
                c.transform.SetParent(db.transform);
                db.SetCard(c);
                if(i<2)
                c.SetImageBackGround(Card.CardBackgrounds.Orange);
            }
            i++;
        }

    }
    public string Serialize()
    {
        string result = id + ":d:";
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] != null)
                result += (col.col.GetIndex(cards[i]));
            if (i < cards.Length - 1) result += ":d:";
        }
        return result;
    }

    private bool IsValidEdit(Card current, Card selected)
    {
        if (selected != null)
        {
            if (selected.rarity != Rarity.Basic)
            {
                if (current.rarity != selected.rarity)
                    foreach (MenuCardBox cb in deckBoxes)
                        if (cb.GetCard().rarity == selected.rarity) return false;
            }
        }
        else if (current != null) return false;
        return true;
    }
}