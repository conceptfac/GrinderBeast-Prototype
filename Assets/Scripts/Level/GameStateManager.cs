using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    GAMEPLAY,
    PAUSE
}

public class GameStateManager : MonoBehaviour
{

    #region Singleton

    private static GameStateManager _instance;

    public static GameStateManager instance
    {
        get => _instance;
    }

    #endregion

    #region Fields
    [SerializeField] private GameState _gameState;
    public GameState gameState {  get=>_gameState; private set { _gameState = value; } }
    #endregion

    #region Delegates
    public delegate void GameStateChange(GameState state);
        public GameStateChange OnGameStateChanged;
    #endregion

    #region MonoBehaviour Methods

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(_instance);
        }

        _instance = this;
    }


    #endregion

    #region Public Methods
    public void SetState(GameState state)
    {
        if (gameState == state) return;
        gameState = state;
        OnGameStateChanged(state);
    }
    #endregion
}
