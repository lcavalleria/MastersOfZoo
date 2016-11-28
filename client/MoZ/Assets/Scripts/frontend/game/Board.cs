using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour
{

    public GameCardBox[,] boardBoxes;
    private Player blue;
    private Player red;

    [SerializeField]
    private GameCardBox cardBoxprefab;
    [SerializeField]
    private float margin;

    // Use this for initialization

    void Start()
    {

        boardBoxes = new GameCardBox[3, 3];

        for (int i = 0; i < boardBoxes.GetLength(0); i++)
        {
            for (int j = 0; j < boardBoxes.GetLength(1); j++)
            {
                boardBoxes[i, j] = Instantiate(cardBoxprefab) as GameCardBox;
                boardBoxes[i, j].Init();
                boardBoxes[i, j].boxPosJ = j;
                boardBoxes[i, j].boxPosI = i;
                float marginX;
                float marginY;
                if (i == 0) marginY = 0;
                else marginY = margin;
                if (j == 0) marginX = 0;
                else marginX = margin;
                boardBoxes[i, j].transform.position = new Vector3(
                    boardBoxes[i, j].transform.position.x + j * (boardBoxes[i, j].GetComponent<BoxCollider2D>().size.x + marginX),
                    boardBoxes[i, j].transform.position.y - i * (boardBoxes[i, j].GetComponent<BoxCollider2D>().size.y + marginY),
                    boardBoxes[i, j].transform.position.z
                    );
            }
        }
    }


    public void UpdateBoard(string[] info)
    {
        foreach (GameCardBox cb in boardBoxes)
            if (cb.GetCard() != null)
                Destroy(cb.GetCard().gameObject);
        for (int j = 0; j < boardBoxes.GetLength(0); j++)
            for (int i = 0; i < boardBoxes.GetLength(1); i++)
            {
                if (info[i * 3 + j] != "null")
                {
                    Card c = Instantiate(MenuMaster.cardPrefab);
                    c.Init();
                    c.Deserialize(info[i * boardBoxes.GetLength(0) + j],false);
                    c.DrawBigValues();
                    boardBoxes[i, j].SetCard(c);
                }
            }
    }
}