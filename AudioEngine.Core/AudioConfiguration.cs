using System;

namespace AudioEngine.Core
{
    /// <summary>
    /// Configuration settings for the AudioEngine.
    /// Provides centralized management of audio engine parameters.
    /// </summary>
    public sealed class AudioConfiguration
    {
        /// <summary>
        /// Maximum number of concurrent sounds (12 slots).
        /// </summary>
        public const int MaxSoundSlots = 12;

        /// <summary>
        /// Default master volume level.
        /// </summary>
        public float MasterVolume { get; set; } = 1.0f;

        /// <summary>
        /// Base directory path for sound files.
        /// </summary>
        public string SoundPath { get; set; } = string.Empty;

        /// <summary>
        /// Enable or disable sound engine.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Sound slot configuration names (for reference).
        /// </summary>
        public string[] SoundSlotNames { get; } = new string[MaxSoundSlots]
        {
            "Jump/Movement",
            "Explosion/Impact",
            "Coin/Pickup",
            "Power-up",
            "Enemy Sound",
            "Background Effect",
            "UI Interaction",
            "Damage/Hit",
            "Level Complete",
            "Game Over",
            "Menu Select",
            "Alert/Warning"
        };

        /// <summary>
        /// Validate the configuration.
        /// </summary>
        /// <returns>True if configuration is valid</returns>
        public bool Validate()
        {
            if (MasterVolume < 0f || MasterVolume > 1f)
                return false;

            if (string.IsNullOrWhiteSpace(SoundPath))
                return false;

            return true;
        }

        /// <summary>
        /// Get the recommended sound slot name.
        /// </summary>
        /// <param name="slotId">Slot ID (0-11)</param>
        /// <returns>Recommended name for the slot</returns>
        public string GetSlotName(int slotId)
        {
            if (slotId < 0 || slotId >= MaxSoundSlots)
                throw new ArgumentOutOfRangeException(nameof(slotId));

            return SoundSlotNames[slotId];
        }
    }
}
