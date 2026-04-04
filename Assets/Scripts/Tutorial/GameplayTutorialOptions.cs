public static class GameplayTutorialOptions
{
    private static bool _shouldRunTutorial = true;

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
}
