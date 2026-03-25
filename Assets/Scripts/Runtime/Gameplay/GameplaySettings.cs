using NewFrogger.Gameplay.Data;

namespace NewFrogger.Gameplay.Domain
{
    public class GameplaySettings
    {
        public float ReferenceSpeed { get; private set; }
        public float ZLimit { get; private set; }

        public GameplaySettings(GameplaySettingsSO settingsSO)
        {
            ReferenceSpeed = settingsSO.ReferenceSpeed;
            ZLimit = settingsSO.ZLimit;
        }
    }
}