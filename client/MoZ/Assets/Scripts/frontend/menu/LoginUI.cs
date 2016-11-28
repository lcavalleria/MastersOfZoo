using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public enum LoginResults
{
    Unassigned,
    Success,
    NameIncorrect,
    PasswordIncorrect
}
public enum CreateAccResults
{
    Unassigned,
    Success,
    NameIncorrect,
    NameExists,
    PasswordIncorrect,
    MailIncorrect,
    Fail
}
public class LoginUI : MonoBehaviour {
    [SerializeField]
    private InputField createUsername;
    [SerializeField]
    private InputField createPassword;
    [SerializeField]
    private InputField createMail;
    [SerializeField]
    private InputField loginUsername;
    [SerializeField]
    private InputField loginPassword;
    [SerializeField]
    bool localhost;
    [SerializeField]
    private MessageBox messageBoxPrefab;
    private MessageBox messageBox;
    [SerializeField]
    private Text connectBtnTxt;

    EventSystem system;

    #region bools
    private bool creating = false;
    private bool connecting = false;
    private static bool connectionError=false;
    #endregion
    void Start()
    {
        if (localhost)
            NetworkManager.host = "localhost";
        else
            NetworkManager.host = "185.81.166.126";
        system = EventSystem.current;
        system.SetSelectedGameObject(loginUsername.gameObject, new BaseEventData(system));
    }
    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.createResult != CreateAccResults.Unassigned)
        {
            switch (NetworkManager.createResult)
            {
                case CreateAccResults.Success:
                    ShowMessageBox("User created!", true);
                    break;
                case CreateAccResults.NameIncorrect:
                    ShowMessageBox("Incorrect name. Name can not contain special characters!", true);
                    break;
                case CreateAccResults.NameExists:
                    ShowMessageBox("Name already taken! Try another name.", true);
                    break;
                case CreateAccResults.PasswordIncorrect:
                    ShowMessageBox("Password must be at least 4 characters long and can not contain special characters.", true);
                    break;
                case CreateAccResults.MailIncorrect:
                    ShowMessageBox("Mail incorrect", true);
                    break;
            }
            NetworkManager.createResult = CreateAccResults.Unassigned;

        }
        if (connectionError)
        {
            connectionError = false;
            ShowMessageBox("Cannot connect to server.", true);
        }
        if (NetworkManager.loginResult != LoginResults.Unassigned)
        {
            switch (NetworkManager.loginResult)
            {
                case LoginResults.Success:
                    ConnectionAccepted();
                    break;
                case LoginResults.NameIncorrect:
                    ShowMessageBox("Invalid username", true);
                    break;
                case LoginResults.PasswordIncorrect:
                    ShowMessageBox("Invalid password", true);
                    break;
            }
            NetworkManager.loginResult = LoginResults.Unassigned;
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next;
            if (system.currentSelectedGameObject == null)
            {
                next = loginUsername.GetComponent<Selectable>();
            }
            else
            {
                next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
            }
            if (next != null)
            {
                InputField inputfield = next.GetComponent<InputField>();
                if (inputfield != null)
                    inputfield.OnPointerClick(new PointerEventData(system));
                system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
            }
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (messageBox != null && messageBox.button.interactable)
                OnMessageBoxButton();
            else if (system.currentSelectedGameObject.transform.parent.name == "CreateAccBgd")
                OnCreateButton();
            else if (system.currentSelectedGameObject.transform.parent.name == "LoginBgd")
                OnConnectButton();
            else Debug.Log("weird thing");
        }
    }
    public static void ConnectionFailed()
    {
        connectionError = true;
    }
    public void OnConnectButton()
    {
        NetworkManager.StartClient(loginUsername.text,loginPassword.text);
        connecting = true;
        ShowMessageBox("connecting...", false);
    }
    public void OnCreateButton()
    {
        NetworkManager.CreateAccount(createUsername.text,createPassword.text,createMail.text);
        creating = true;
        ShowMessageBox("Creating user",false);
    }
    public void OnMessageBoxButton()
    {
        messageBox.button.onClick.RemoveListener(OnMessageBoxButton);
        Destroy(messageBox.gameObject);
        messageBox = null;
    }
    public void ConnectionAccepted()
    {
        Application.LoadLevel("MainMenu");
    }

    private void ShowMessageBox(string message,bool canDismiss)
    {
        if(messageBox!=null)
        Destroy(messageBox.gameObject);
        messageBox = Instantiate(messageBoxPrefab);
        messageBox.transform.SetParent(transform);
        messageBox.button.onClick.AddListener(OnMessageBoxButton);
        messageBox.message.text = message;
        if (!canDismiss)
            messageBox.button.interactable = false;
    }
}
