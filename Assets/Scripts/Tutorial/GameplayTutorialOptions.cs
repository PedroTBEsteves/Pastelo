public static class GameplayTutorialOptions
{
    private static bool _shouldRunTutorial = false;

    public static void SetShouldRunTutorial(bool shouldRunTutorial)
    {
        _shouldRunTutorial = shouldRunTutorial;
    }

    public static bool ConsumeShouldRunTutorial()
    {
        var shouldRunTutorial = _shouldRunTutorial;
        _shouldRunTutorial = false;
        return shouldRunTutorial;
    }

    public static bool PeekShouldRunTutorial()
    {
        return _shouldRunTutorial;
    }
}
