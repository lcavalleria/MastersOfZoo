using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class CardCollection
{
    public Dictionary<string, Card> cards;
    private MenuDecks col;
    MenuCardBox[,] colboxes;
    GameObject container;
    private int selectedBoxI = -1, selectedBoxJ = -1;

    public void BoxClicked(MenuCardBox b)
    {
        if (b.GetCard() != null)
        {
            if (b.boxPosI == selectedBoxI && b.boxPosJ == selectedBoxJ)
            {
                DeselectCard();
            }
            else if (selectedBoxI != -1)
            {
                DeselectCard();
                SetSelectedPos(b.boxPosI, b.boxPosJ);
                b.GetCard().SetImageBackGround(Card.CardBackgrounds.Blue);
            }
            else
            {
                SetSelectedPos(b.boxPosI, b.boxPosJ);
                b.GetCard().SetImageBackGround(Card.CardBackgrounds.Blue);
            }
        }
    }
    private void SetSelectedPos(int i, int j)
    {
        selectedBoxI = i;
        selectedBoxJ = j;
    }
    public void DeselectCard()
    {
        GetSelectedCard().SetImageBackGround(Card.CardBackgrounds.Black);
        SetSelectedPos(-1, -1);
    }
    public Card GetSelectedCard()
    {
        if (selectedBoxI != -1)
        {
            return colboxes[selectedBoxI, selectedBoxJ].GetCard();
        }
        else return null;
    }
    public int GetIndex(Card c)
    {
        if (cards.ContainsKey(c.cardName))
            return ((MenuCardBox)cards[c.cardName].cardBox).index;
        else return -1;
    }

    public CardCollection(string colStr, MenuDecks col, float margin)
    {
        container = new GameObject();
        container.transform.SetParent(col.ColSv.GetChild(0).GetChild(0).transform);
        container.transform.localPosition = new Vector2(0, 0);
        this.col = col;
        colboxes = new MenuCardBox[3, 5];
        cards = new Dictionary<string, Card>();
        int index = 0;
        for (int i = 0; i < colboxes.GetLength(0); i++)
            for (int j = 0; j < colboxes.GetLength(1); j++)
            {
                colboxes[i, j] = MonoBehaviour.Instantiate(col.cardBoxprefab) as MenuCardBox;
                colboxes[i, j].Init(col, -1, index);
                colboxes[i, j].transform.SetParent(container.transform);
                colboxes[i, j].transform.localPosition = new Vector3(1, 1, -1);
                colboxes[i, j].boxPosJ =j;
                colboxes[i, j].boxPosI = i;
                index++;
                colboxes[i, j].transform.Translate(new Vector2(
                    j * (colboxes[i, j].GetComponent<BoxCollider2D>().size.x + margin),
                    -i * (colboxes[i, j].GetComponent<BoxCollider2D>().size.y + margin)));
            }
        Deserialize(colStr);
        container.name = "Collection container";
    }

    public void Deserialize(string colStr)
    {
        string[] colInfo = colStr.Split(new string[] { ":co:" }, StringSplitOptions.None);
        foreach (MenuCardBox cb in colboxes)
            if (cb.GetCard() != null)
                MonoBehaviour.Destroy(cb.GetCard().gameObject);
        int i = 0;
        foreach (MenuCardBox cb in colboxes)
        {
            bool filled = false;
            while (!filled)
            {
                if (i < colInfo.Length)
                {
                    if (cards.ContainsKey(Card.GetNameFromSerialized(colInfo[i])))
                        ((MenuCardBox)cards[Card.GetNameFromSerialized(colInfo[i])].cardBox).addDuplicate();
                    else
                    {
                        filled = true;
                        Card c = MonoBehaviour.Instantiate(MenuMaster.cardPrefab);
                        c.Init();
                        c.Deserialize(colInfo[i], true);
                        cards.Add(c.cardName, c);
                        c.DrawBigValues();
                        c.transform.SetParent(cb.transform);
                        Debug.Log(cb.name);
                        cb.SetCard(c);
                    }
                }
                else filled = true;
                i++;
            }
            while (!filled && i < colInfo.Length)
            {
                if (cards.ContainsKey(Card.GetNameFromSerialized(colInfo[i])))
                    ((MenuCardBox)cards[Card.GetNameFromSerialized(colInfo[i])].cardBox).addDuplicate();
                i++;
            }
        }
    }
}