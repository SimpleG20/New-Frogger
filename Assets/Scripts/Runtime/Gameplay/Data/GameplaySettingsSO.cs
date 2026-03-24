using UnityEngine;
using NewFrogger.Player.Domain;
using NewFrogger.Gameplay.Domain;

namespace NewFrogger.Gameplay.Data
{
    [CreateAssetMenu(fileName = "gameplay_settings", menuName = "ScriptableObjects/Gameplay/Settings")]
    public class GameplaySettingsSO : ScriptableObject, IPlayerSettings, ITrafficLevelSettings
    {
        [Header("Traffic")]
        [Tooltip("Base speed used to calculate relative vehicle velocity.")]
        [field: Min(0.1f)]
        [field: SerializeField] public float ReferenceSpeed { get; private set; } = 10f;
        [field: SerializeField] public int MaxLevels { get; private set; } = 3;

        [Header("Lifecycle Limits")]
        [Tooltip("The Z position where vehicles are recycled.")]
        [field: SerializeField, Min(0)] public float zVehicleLimit { get; private set; } = 50f;
        [Tooltip("The X position where the player when get it will win the level")]
        [field: SerializeField] public float xVictory { get; private set; }

        [Header("Player Movement Limits")]
        [field: SerializeField] public float MinX { get; private set; }
        [field: SerializeField] public float MaxX { get; private set; }
        [field: SerializeField] public float MinZ { get; private set; }
        [field: SerializeField] public float MaxZ { get; private set; }

        [Header("Player Movement Factors")]
        [Tooltip("Amount of squares the player will jump")]
        [field: SerializeField] public float GridMovementFactor { get; private set; }
        [field: SerializeField] public float PlayerDelayFactor { get; private set; }
    }
}