using UnityEngine;
using System.Timers;
using System;

public class Master : MonoBehaviour
{
    [SerializeField]
    public Player _player;
    [SerializeField]
    public Player _enemy;
    [SerializeField]
    private Board _board;

    public static bool playerTurn;
    private Sprite turnPlayerArrow;
    private Sprite turnEnemyArrow;
    [SerializeField]
    private SpriteRenderer turnArrow;
    public static GameObject canvas;
    [SerializeField]
    private double turnTimeSeconds;
    private static double turnTimeCurrent;
    [SerializeField]
    private TextMesh timerLbl;
    private bool gameEnded = false;
    // Use this for initialization
    void Start()
    {
        turnPlayerArrow = SpriteManager.GetSprite("UIBlueTurn");
        turnEnemyArrow = SpriteManager.GetSprite("UIRedTurn");
        turnTimeCurrent = turnTimeSeconds;
    }
    // Update is called once per frame
    void Update()
    {
        if (!gameEnded)
        {
            turnTimeCurrent -= Time.deltaTime;
            timerLbl.text = "" + (int)turnTimeCurrent;
        }
    }
    public void UpdateGameState(string gameState)
    {
        string[] strGameState = gameState.Split(new string[] { ":g:" }, System.StringSplitOptions.None);
        string[] info = strGameState[0].Split(new string[] { ":gp:" }, System.StringSplitOptions.None);
        bool player1turn = System.Convert.ToInt32(strGameState[3]) == 1;
        if (info[0] == NetworkManager.playerName)
        {
            _player.UpdatePlayerState(info);
            info = strGameState[1].Split(new string[] { ":gp:" }, System.StringSplitOptions.None);
            _enemy.UpdatePlayerState(info);
            playerTurn = player1turn;
        }
        else
        {
            _enemy.UpdatePlayerState(info);
            info = strGameState[1].Split(new string[] { ":gp:" }, System.StringSplitOptions.None);
            _player.UpdatePlayerState(info);
            playerTurn = !player1turn;
        }
        info = strGameState[2].Split(new string[] { ":b:" }, System.StringSplitOptions.None);
        _board.UpdateBoard(info);
        SetTurnArrow();
    }

    public void CardClicked(Card selectedCard)
    {
        if (selectedCard.bluePlayerOwned)
        {
            if (selectedCard.selected)
            {
                DeselectCard();
            }
            else if (_player._selectedCardIndex != -1)
            {
                DeselectCard();
                _player._selectedCardIndex = _player.GetIndexOfCard(selectedCard);
                _player.hand[_player._selectedCardIndex].Select();
            }
            else
            {
                _player._selectedCardIndex = _player.GetIndexOfCard(selectedCard);
                _player.hand[_player._selectedCardIndex].Select();
            }
        }
    }
    private void DeselectCard()
    {
        _player.hand[_player._selectedCardIndex].Deselect();
        _player._selectedCardIndex = -1;
    }
    public void BoxClicked(GameCardBox box)
    {
        if (playerTurn && _player._selectedCardIndex != -1 && box.empty)
        {
            int index = _player._selectedCardIndex;
            DeselectCard();
            PlayCard(box, index);
        }
    }

    private void PlayCard(GameCardBox box, int cardIndex)
    {
        NetworkManager.Send("movement", box.boxPosI + "," + box.boxPosJ + "," + cardIndex);
    }
    public void SetTurnArrow()
    {
        if (playerTurn)
            turnArrow.sprite = turnPlayerArrow;
        else turnArrow.sprite = turnEnemyArrow;
        turnTimeCurrent = turnTimeSeconds;

    }

    public void EndGame(GameResults result)
    {
        canvas.SetActive(true);
        canvas.GetComponent<GameUI>().SetImage(result);
        gameEnded = true;
    }
}