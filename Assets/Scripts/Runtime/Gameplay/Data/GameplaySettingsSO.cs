using UnityEngine;
using NewFrogger.Player.Domain;
using NewFrogger.Gameplay.Domain;

namespace NewFrogger.Gameplay.Data
{
    [CreateAssetMenu(fileName = "gameplay_settings", menuName = "ScriptableObjects/Gameplay/Settings")]
    public class GameplaySettingsSO : ScriptableObject, IPlayerSettings, ITrafficLevelSettings
    {
        [Header("Traffic Physics")]
        [Tooltip("Base speed used to calculate relative vehicle velocity.")]
        [field: Min(0.1f)]
        [field: SerializeField] public float ReferenceSpeed { get; private set; } = 10f;

        [Header("Lifecycle Limits")]
        [Tooltip("The Z position where vehicles are recycled.")]
        [field: SerializeField, Min(0)] public float ZLimit { get; private set; } = 50f;

        [field: SerializeField] public float MinX { get; private set; }
        [field: SerializeField] public float MaxX { get; private set; }
        [field: SerializeField] public float MinZ { get; private set; }
        [field: SerializeField] public float MaxZ { get; private set; }

        [field: SerializeField] public float GridMovementFactor { get; private set; }
        [field: SerializeField] public float PlayerSpeedFactor { get; private set; }
    }
}