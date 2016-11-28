using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum GameResults
{
    Win,
    Lose,
    Draw
}
public class GameUI : MonoBehaviour {

    Button btnContinue;
    Image imgEndGame;
    [SerializeField]
    Sprite winSprite, loseSprite, drawSprite;

	// Use this for initialization
	void Start () {
        btnContinue = transform.Find("BtnContinue").GetComponent<Button>();
        imgEndGame = transform.Find("ImgEndGame").GetComponent<Image>();
        Master.canvas = gameObject;
        gameObject.SetActive(false);
	}

    public void OnContinueButton()
    {
        Application.LoadLevel("MainMenu");
    }

    /// <summary>
    /// result: 0=lose,1=draw,2=win
    /// </summary>
    /// <param name="result">0=lose,1=draw,2=win</param>
	public void SetImage(GameResults result)
    {
        switch (result)
        {
            case GameResults.Win:
                imgEndGame.sprite = winSprite;
                break;
            case GameResults.Lose:
                imgEndGame.sprite = loseSprite;
                break;
            case GameResults.Draw:
                imgEndGame.sprite = drawSprite;
                break;
        }
    }
	// Update is called once per frame
	void Update () {
	
	}
}
