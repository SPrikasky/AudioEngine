using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace AudioEngine.Core
{
    /// <summary>
    /// Main audio engine manager providing low-latency sound playback for games.
    /// Supports 12 independent concurrent sound effects with minimal initialization overhead.
    /// Thread-safe singleton for game-wide audio management.
    /// </summary>
    public sealed class AudioManager : IDisposable
    {
        private static readonly Lazy<AudioManager> _instance = new Lazy<AudioManager>(() => new AudioManager());
        private static readonly object _lockObject = new object();

        public static AudioManager Instance => _instance.Value;

        private readonly SoundEffect[] _soundSlots = new SoundEffect[12];
        private readonly ReaderWriterLockSlim _soundLock = new ReaderWriterLockSlim();
        private string _soundPath = string.Empty;
        private bool _isInitialized = false;
        private bool _isDisposed = false;

        private AudioManager()
        {
            // Private constructor for singleton pattern
        }

        /// <summary>
        /// Initialize the audio manager with a base sound directory path.
        /// Must be called before loading or playing sounds.
        /// </summary>
        /// <param name="soundPath">Base directory path for sound files</param>
        public void Initialize(string soundPath)
        {
            lock (_lockObject)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException("AudioManager");

                if (string.IsNullOrWhiteSpace(soundPath))
                    throw new ArgumentException("Sound path cannot be null or empty.", nameof(soundPath));

                if (!Directory.Exists(soundPath))
                    throw new DirectoryNotFoundException($"Sound directory not found: {soundPath}");

                _soundPath = Path.GetFullPath(soundPath);
                _isInitialized = true;
            }
        }

        /// <summary>
        /// Load a sound file into a specific slot (0-11).
        /// Sounds are pre-loaded for minimal playback latency.
        /// </summary>
        /// <param name="slotId">Slot ID (0-11)</param>
        /// <param name="fileName">Sound file name (relative to sound path)</param>
        public void LoadSound(int slotId, string fileName)
        {
            ValidateSlotId(slotId);

            if (!_isInitialized)
                throw new InvalidOperationException("AudioManager not initialized. Call Initialize() first.");

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

            string fullPath = Path.Combine(_soundPath, fileName);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Sound file not found: {fullPath}");

            _soundLock.EnterWriteLock();
            try
            {
                // Dispose existing sound if any
                _soundSlots[slotId]?.Dispose();

                // Load new sound
                _soundSlots[slotId] = new SoundEffect(fullPath);
            }
            finally
            {
                _soundLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Play a sound from the specified slot immediately with minimal latency.
        /// </summary>
        /// <param name="slotId">Slot ID (0-11)</param>
        public void PlaySound(int slotId)
        {
            ValidateSlotId(slotId);

            _soundLock.EnterReadLock();
            try
            {
                SoundEffect sound = _soundSlots[slotId];
                if (sound != null)
                {
                    sound.Play();
                }
            }
            finally
            {
                _soundLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Stop playback of a sound in the specified slot.
        /// </summary>
        /// <param name="slotId">Slot ID (0-11)</param>
        public void StopSound(int slotId)
        {
            ValidateSlotId(slotId);

            _soundLock.EnterReadLock();
            try
            {
                SoundEffect sound = _soundSlots[slotId];
                if (sound != null)
                {
                    sound.Stop();
                }
            }
            finally
            {
                _soundLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Stop all currently playing sounds.
        /// </summary>
        public void StopAllSounds()
        {
            _soundLock.EnterReadLock();
            try
            {
                for (int i = 0; i < _soundSlots.Length; i++)
                {
                    SoundEffect sound = _soundSlots[i];
                    if (sound != null && sound.IsPlaying)
                    {
                        sound.Stop();
                    }
                }
            }
            finally
            {
                _soundLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Set the volume level for a specific sound slot.
        /// </summary>
        /// <param name="slotId">Slot ID (0-11)</param>
        /// <param name="volume">Volume level (0.0 to 1.0)</param>
        public void SetVolume(int slotId, float volume)
        {
            ValidateSlotId(slotId);

            if (volume < 0f || volume > 1f)
                throw new ArgumentException("Volume must be between 0.0 and 1.0", nameof(volume));

            _soundLock.EnterReadLock();
            try
            {
                SoundEffect sound = _soundSlots[slotId];
                if (sound != null)
                {
                    sound.Volume = volume;
                }
            }
            finally
            {
                _soundLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Get the volume level of a specific sound slot.
        /// </summary>
        /// <param name="slotId">Slot ID (0-11)</param>
        /// <returns>Volume level (0.0 to 1.0)</returns>
        public float GetVolume(int slotId)
        {
            ValidateSlotId(slotId);

            _soundLock.EnterReadLock();
            try
            {
                SoundEffect sound = _soundSlots[slotId];
                return sound?.Volume ?? 0f;
            }
            finally
            {
                _soundLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Check if a sound in the specified slot is currently playing.
        /// </summary>
        /// <param name="slotId">Slot ID (0-11)</param>
        /// <returns>True if sound is playing, false otherwise</returns>
        public bool IsPlaying(int slotId)
        {
            ValidateSlotId(slotId);

            _soundLock.EnterReadLock();
            try
            {
                SoundEffect sound = _soundSlots[slotId];
                return sound?.IsPlaying ?? false;
            }
            finally
            {
                _soundLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Get the duration of a loaded sound.
        /// </summary>
        /// <param name="slotId">Slot ID (0-11)</param>
        /// <returns>Duration in milliseconds, or 0 if no sound loaded</returns>
        public int GetDuration(int slotId)
        {
            ValidateSlotId(slotId);

            _soundLock.EnterReadLock();
            try
            {
                SoundEffect sound = _soundSlots[slotId];
                return sound?.Duration ?? 0;
            }
            finally
            {
                _soundLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Unload a sound from a specific slot and free its resources.
        /// </summary>
        /// <param name="slotId">Slot ID (0-11)</param>
        public void UnloadSound(int slotId)
        {
            ValidateSlotId(slotId);

            _soundLock.EnterWriteLock();
            try
            {
                SoundEffect sound = _soundSlots[slotId];
                if (sound != null)
                {
                    sound.Stop();
                    sound.Dispose();
                    _soundSlots[slotId] = null;
                }
            }
            finally
            {
                _soundLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Get the number of currently playing sounds.
        /// </summary>
        /// <returns>Count of sounds currently playing</returns>
        public int GetPlayingCount()
        {
            _soundLock.EnterReadLock();
            try
            {
                int count = 0;
                for (int i = 0; i < _soundSlots.Length; i++)
                {
                    SoundEffect sound = _soundSlots[i];
                    if (sound != null && sound.IsPlaying)
                        count++;
                }
                return count;
            }
            finally
            {
                _soundLock.ExitReadLock();
            }
        }

        private void ValidateSlotId(int slotId)
        {
            if (slotId < 0 || slotId >= 12)
                throw new ArgumentOutOfRangeException(nameof(slotId), "Slot ID must be between 0 and 11.");
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            lock (_lockObject)
            {
                if (_isDisposed)
                    return;

                _soundLock.EnterWriteLock();
                try
                {
                    for (int i = 0; i < _soundSlots.Length; i++)
                    {
                        SoundEffect sound = _soundSlots[i];
                        if (sound != null)
                        {
                            sound.Stop();
                            sound.Dispose();
                            _soundSlots[i] = null;
                        }
                    }

                    _isDisposed = true;
                }
                finally
                {
                    _soundLock.ExitWriteLock();
                }

                _soundLock?.Dispose();
            }
        }
    }
}
