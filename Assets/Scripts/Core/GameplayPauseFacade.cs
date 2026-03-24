using System.Collections.Generic;

public static class GameplayPauseFacade
{
    private static readonly HashSet<int> activeTokens = new HashSet<int>();
    private static int nextToken = 1;

    public static bool IsPaused => activeTokens.Count > 0;

    public static int PushPause()
    {
        int token = nextToken++;
        activeTokens.Add(token);
        return token;
    }

    public static void PopPause(int token)
    {
        if (token <= 0)
        {
            return;
        }

        activeTokens.Remove(token);
    }

    public static void ClearAll()
    {
        activeTokens.Clear();
    }
}
