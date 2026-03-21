public class GameplaySettings
{
    public static GameplaySettings Instance { get; private set; }
    public float ReferenceSpeed { get; private set; }
    public float ZLimit { get; private set; }

    public GameplaySettings(GameplaySettingsSO settingsSO)
    {
        Instance = this;
        ReferenceSpeed = settingsSO.ReferenceSpeed;
        ZLimit = settingsSO.ZLimit;
    }
}