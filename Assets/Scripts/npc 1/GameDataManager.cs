// GameDataManager.cs
public static class GameDataManager
{
    // ✅ 기본 흐름
    public static int nextStoryIdToLoad = 0;

    // ✅ 문제해결게임 분기 관련
    public static int puzzleSuccessStoryId = 0;
    public static int puzzleFailStoryId = 0;
    public static bool isPuzzleSuccess = false;
    public static bool puzzleWasPlayed = false;
}