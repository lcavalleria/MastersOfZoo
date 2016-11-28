using UnityEngine;


public abstract class CardBox : MonoBehaviour
{
    protected Card _card;
    public int boxPosI;
    public int boxPosJ;
    public Card GetCard() { return _card; }

    public virtual void SetCard(Card card)
    {
        _card = card;
        _card.cardBox = this;
    }
}
