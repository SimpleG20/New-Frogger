using UnityEngine;

namespace NewFrogger.Gameplay.Data
{
    [CreateAssetMenu(fileName = "gameplay_settings", menuName = "ScriptableObjects/Gameplay/Settings")]
    public class GameplaySettingsSO : ScriptableObject
    {
        [Header("Traffic Physics")]
        [Tooltip("Base speed used to calculate relative vehicle velocity.")]
        [field: Min(0.1f)]
        [field: SerializeField] public float ReferenceSpeed { get; private set; } = 10f;

        [Header("Lifecycle Limits")]
        [Tooltip("The Z position where vehicles are recycled.")]
        [field: Min(0)]
        [field: SerializeField] public float ZLimit { get; private set; } = 50f;
    }
}