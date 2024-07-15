using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// This script is a Singleton only to reference the H.U.D Elements and Actions. 
/// </summary>
public class HUDManager : MonoBehaviour
{
    #region Singleton

    private static HUDManager _instance;

    public static HUDManager instance {
        get => _instance;
    }

    #endregion

    #region Fields
    [Header("Menu")]
    public Button pauseButton;
    public GameObject pauseCaption;
    public Button menuButton;

    [Header("MenuPanel")]
    public GameObject menuPanel;
    public Button prevSetButton;
    public Button nextSetButton;
    public Button closeMenuButton;
    public Button okMenuButton;
    public TextMeshProUGUI TMP_AcceptCaptionButton;
    public TextMeshProUGUI TMP_MenuMessage;

    [Header("Controllers")]
    public Joystick joystick;
    public Button actionButton;
    public Button jumpButton;
    public Button carryButton;
    public Button dropButton;
    public Button interactButton;

    [Header("FeedBack Elements")]
    public TextMeshProUGUI TMP_Coins;

    #endregion

    #region Delegates
    public delegate void MenuClick();
    public MenuClick menuClick;

    public delegate void PauseClick();
    public PauseClick pauseClick;

    public delegate void ActionClick();
    public ActionClick actionClick;

    public delegate void JumpClick(bool down);
    public JumpClick jumpClick;

    public delegate void CarryClick();
    public CarryClick carryClick;

    public delegate void DropClick();
    public DropClick dropClick;
    
    public delegate void InteractClick();
    public InteractClick interactClick;

    public delegate void SetChangeClick(int indicator);
    public SetChangeClick setChangeClick;

    public delegate void CloseMenuClick(bool status);
    public CloseMenuClick closeMenuClick;


    #endregion

    #region MonoBehaviour Methods

    void Awake (){
        if (_instance != null){
            Destroy(_instance);
        }

        _instance = this;
    }


    #endregion

    #region Public Methods

    /// <summary>
    /// Sets Action button active
    /// </summary>
    /// <param name="status"></param>
    public void SetActionButton(bool status)
    {
        actionButton.gameObject.SetActive(status);
    }

    // HUD Buttons Delegates
    public void OnMenuClick() { menuPanel.SetActive(true); }
    public void OnPauseClick() { pauseClick(); pauseCaption.SetActive(GameStateManager.instance.gameState == GameState.PAUSE); }
    public void OnActionClick() { actionClick(); }

    public void OnJumpClick(bool down) { jumpClick(down); }

    public void OnCarryClick() { carryClick(); }

    public void OnDropClick() { dropClick(); }
    public void OnInteractClick() { interactClick(); }

    public void OnChangeSetClick(int indicator) { setChangeClick(indicator); }

    public void OnCloseMenuClick(bool status) { closeMenuClick(status); }
    // end of delegates


    public void ShowMessage(TextMeshProUGUI caption, string message, float time)
    {
        StartCoroutine(ShowMessageCor(caption, message, time)); 
    }

    #endregion

    #region Coroutines
    private IEnumerator ShowMessageCor(TextMeshProUGUI caption, string message, float time) {

        caption.text = message;
        caption.enabled = true;
        yield return new WaitForSeconds(time);
        caption.enabled = false;
        caption.text = string.Empty;

    }
    #endregion
}
