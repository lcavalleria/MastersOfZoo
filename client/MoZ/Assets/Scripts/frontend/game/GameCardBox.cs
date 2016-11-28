using UnityEngine;
using System.Collections;

public class GameCardBox : CardBox
{
    private Master master;

    public bool empty { get; private set; }

    public void Init()
    {
        empty = true;
        master = GameObject.Find("Master").GetComponent<Master>();
    }

    public override void SetCard(Card card)
    {
        empty = false;
        base.SetCard(card);
        _card.transform.position = new Vector3(transform.position.x, transform.position.y, -20);
        _card.Play();
    }

    public void OnMouseDown()
    {
        master.BoxClicked(this);
    }
}
