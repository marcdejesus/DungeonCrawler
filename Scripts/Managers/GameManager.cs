/// <summary>
/// Set the game state
/// </summary>
/// <param name="newState">New game state</param>
public void SetGameState(GameState newState)
{
    currentState = newState;
    
    // Notify listeners
    OnGameStateChanged?.Invoke();
} 