using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// LevelManager class sets and controls the game level
/// </summary>
public class LevelManager : MonoBehaviour
{
    #region Singleton

    private static LevelManager _instance;

    public static LevelManager instance
    {
        get => _instance;
    }

    #endregion

    #region Fields

    #endregion;


    #region MonoBehaviour Methods

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(_instance);
        }

        _instance = this;


    }

    // Start is called before the first frame update
    void Start()
    {
        //Creates a GameStateChanged listener
        GameStateManager.instance.OnGameStateChanged += OnGameStateChanged;

        coins = 0;
        HUDManager.instance.pauseClick = PauseGame;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        //Removes the GameStateChanged listener
        GameStateManager.instance.OnGameStateChanged -= OnGameStateChanged;

    }
    #endregion

    #region PublicMethods

    public void PauseGame()
    {
        GameState currentGameState = GameStateManager.instance.gameState;
        GameState newGameState = (currentGameState == GameState.GAMEPLAY ? GameState.PAUSE : GameState.GAMEPLAY);
        GameStateManager.instance.SetState(newGameState);
    }

    private void OnGameStateChanged(GameState state)
    {

        switch(state){
            case GameState.GAMEPLAY:
                Time.timeScale = 1;
                break;
            case GameState.PAUSE:
                Time.timeScale = 0;
                break;
        }
    }


    public static int coins
    {
        get => PlayerStats.coins;
        set
        {
            PlayerStats.coins = value;
            HUDManager.instance.TMP_Coins.text = string.Format("x{0}", value);
        }
    }



    #endregion
}
