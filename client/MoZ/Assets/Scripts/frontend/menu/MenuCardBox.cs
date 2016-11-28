using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class MenuCardBox : CardBox
{
    private MenuMaster master;
    private int amount;
    SpriteRenderer amountBgd;
    SpriteRenderer amountSR;
    public int deckIndex;
    MenuDecks deckCol;
    public int index;
    public void Init(MenuDecks col,int deckIndex,int index)
    {
        this.index = index;
        deckCol = col;
        master = GameObject.Find("Master").GetComponent<MenuMaster>();
        amountBgd = transform.GetChild(0).GetComponent<SpriteRenderer>();
        amountSR = amountBgd.transform.GetChild(0).GetComponent<SpriteRenderer>();
        this.deckIndex = deckIndex;
        amountBgd.enabled = false;
        amountSR.enabled = false;
        amount = 1;
    }
    public void OnMouseDown()
    {
        deckCol.BoxClicked(this);
    }
    public void addDuplicate()
    {
        amount++;
        amountBgd.enabled = true;
        amountSR.enabled = true;
        amountSR.sprite = SpriteManager.GetSprite("value" + amount);
    }

    public override void SetCard(Card card)
    {
        base.SetCard(card);
        card.Play();
        card.transform.localPosition = new Vector3(0, 0, 1);
        card.transform.localScale = new Vector3(GetComponent<BoxCollider2D>().size.x / card.GetComponent<BoxCollider2D>().size.x, 
            GetComponent<BoxCollider2D>().size.y / card.GetComponent<BoxCollider2D>().size.y, 1);
        if  (deckIndex>=0) {
            deckCol.decks[deckIndex].cards[index] = card;
            card.SetImageBackGround(Card.CardBackgrounds.Grey);
        }
        else card.SetImageBackGround(Card.CardBackgrounds.Black);
    }
    void Update()
    {

    }
}

