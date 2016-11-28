using UnityEngine;
using System.Collections;

public class MenuMaster : MonoBehaviour {
    [SerializeField]
    private Card editorCardPrefab;
    public static Card cardPrefab;
    public MainMenuUI mainMenuUI;

    // Use this for initialization
    void Start () {
        cardPrefab = editorCardPrefab;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
