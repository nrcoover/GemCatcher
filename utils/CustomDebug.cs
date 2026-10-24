namespace GemCatcher.utils
{
  public static class CustomDebug
  {
    public static readonly bool IsDebugging = true;

    public static string GetGameStateString()
    {
      return GameManager.Instance.CurrentGameState switch
      {
        (int)GameManager.GameState.PLAYING => Constants.GameState.Playing,
        (int)GameManager.GameState.DEAD => Constants.GameState.Dead,
        (int)GameManager.GameState.VICTORY => Constants.GameState.Victory,
        _ => "NO GAME STATE FOUND!",
      };
    }

    public static string GetGameModeString()
    {
      return GameManager.Instance.CurrentGameMode switch
      {
        (int)GameManager.GameMode.NORMAL => Constants.GameMode.Normal,
        (int)GameManager.GameMode.INFINITE => Constants.GameMode.Infinite,
        _ => "NO GAME MODE FOUND!",
      };
    }
  }

}