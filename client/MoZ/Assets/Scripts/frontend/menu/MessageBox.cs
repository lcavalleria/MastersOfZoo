using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MessageBox : MonoBehaviour {
    [SerializeField]
    public Text message;
    [SerializeField]
    public Button button;
	// Use this for initialization
	void Start () {
        transform.localScale = new Vector3(1, 1, 1);
        transform.localPosition = new Vector3(0, 0, 0);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
