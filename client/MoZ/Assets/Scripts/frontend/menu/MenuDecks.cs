using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class MenuDecks : MonoBehaviour
{
    public Deck[] decks = new Deck[4];
    public CardCollection col;
    public Button[] deckButtons;
    public Deck selectedDeck;

    [SerializeField]
    public MenuCardBox cardBoxprefab;
    [SerializeField]
    private float margin;

    public Transform DeckCardsPnl;
    public Transform ColSv;

    // Use this for initialization
    void Start()
    {
        transform.localScale = new Vector3(1, 1, 1);
        transform.localPosition = new Vector3(0, 0, 0);
        DeckCardsPnl = transform.GetChild(0).FindChild("DeckCardsPnl");
        ColSv = transform.GetChild(1).FindChild("ColSv");
    }

    public void Deserialize(string decksStr)
    {
        string[] deckscol = decksStr.Split(new string[] { ":de:" }, StringSplitOptions.None);
        string[] decksInfo = deckscol[0].Split(new string[] { ":dl:" }, StringSplitOptions.None);
        col = new CardCollection(deckscol[1], this, margin);
        for (int i = 0; i < 4; i++)
        {
            decks[i] = new Deck(this, margin,i);
            decks[i].Deserialize(decksInfo[i]);
        }
        selectedDeck = decks[0];
        selectedDeck.deckGameObject.SetActive(true);
        deckButtons[0].interactable = false;
        deckButtons[4].interactable = false;
    }

    public void OnButtonDeck(int deckNum)
    {
        selectedDeck.deckGameObject.SetActive(false);
        selectedDeck = decks[deckNum - 1];
        for (int i = 0; i < 4; i++)
        {
            if (i == deckNum - 1)
                deckButtons[i].interactable = false;
            else deckButtons[i].interactable = true;
        }
        if (selectedDeck.hasChanged)
            deckButtons[4].interactable = true;
        selectedDeck.deckGameObject.SetActive(true);
    }
    public void OnButtonSaveDeck()
    {
        NetworkManager.Send("svDeck", selectedDeck.Serialize());
        //TODO:messagebox: "saving..."
        selectedDeck.hasChanged = false;
        deckButtons[4].interactable = false;
    }

    public void BoxClicked(MenuCardBox box)
    {
        if (box.deckIndex>=0)
            selectedDeck.BoxClicked(box);
        else col.BoxClicked(box);
    }
}