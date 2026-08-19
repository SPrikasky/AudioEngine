using NAudio.Wave;
using System;

namespace AudioEngine.Core
{
    /// <summary>
    /// Represents a single sound effect that can be played, stopped, and volume-controlled.
    /// Wraps NAudio's IWavePlayer for low-latency playback optimized for games.
    /// </summary>
    public sealed class SoundEffect : IDisposable
    {
        private IWavePlayer _wavePlayer;
        private AudioFileReader _audioFileReader;
        private readonly string _filePath;
        private float _volume = 1f;
        private bool _isDisposed = false;

        /// <summary>
        /// Gets whether the sound is currently playing.
        /// </summary>
        public bool IsPlaying => _wavePlayer?.PlaybackState == PlaybackState.Playing;

        /// <summary>
        /// Gets or sets the volume level (0.0 to 1.0).
        /// </summary>
        public float Volume
        {
            get => _volume;
            set
            {
                if (value < 0f || value > 1f)
                    throw new ArgumentException("Volume must be between 0.0 and 1.0");

                _volume = value;
                if (_audioFileReader != null)
                {
                    _audioFileReader.Volume = value;
                }
            }
        }

        /// <summary>
        /// Gets the duration of the sound in milliseconds.
        /// </summary>
        public int Duration => _audioFileReader != null ? (int)_audioFileReader.TotalTime.TotalMilliseconds : 0;

        /// <summary>
        /// Initialize a new SoundEffect from a WAV file.
        /// </summary>
        /// <param name="filePath">Full path to the WAV file</param>
        public SoundEffect(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            _filePath = filePath;
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                // Use DirectSoundOut for lowest latency on Windows
                _wavePlayer = new DirectSoundOut();
                _audioFileReader = new AudioFileReader(_filePath);
                _audioFileReader.Volume = _volume;
                _wavePlayer.Init(_audioFileReader);
            }
            catch
            {
                // Fallback to WaveOutEvent if DirectSoundOut unavailable
                try
                {
                    _wavePlayer = new WaveOutEvent();
                    _audioFileReader = new AudioFileReader(_filePath);
                    _audioFileReader.Volume = _volume;
                    _wavePlayer.Init(_audioFileReader);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to initialize sound from file: {_filePath}", ex);
                }
            }
        }

        /// <summary>
        /// Play the sound immediately.
        /// </summary>
        public void Play()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("SoundEffect");

            if (_wavePlayer != null)
            {
                // Reset to beginning if already played
                if (_audioFileReader.Position > 0)
                {
                    _audioFileReader.CurrentTime = TimeSpan.Zero;
                }

                _wavePlayer.Play();
            }
        }

        /// <summary>
        /// Stop the sound playback.
        /// </summary>
        public void Stop()
        {
            if (_isDisposed)
                return;

            if (_wavePlayer != null)
            {
                _wavePlayer.Stop();
                // Reset position for next play
                if (_audioFileReader != null)
                {
                    _audioFileReader.CurrentTime = TimeSpan.Zero;
                }
            }
        }

        /// <summary>
        /// Pause the sound playback (preserves position).
        /// </summary>
        public void Pause()
        {
            if (_isDisposed)
                return;

            if (_wavePlayer != null)
            {
                _wavePlayer.Pause();
            }
        }

        /// <summary>
        /// Reset the sound to the beginning without stopping playback.
        /// </summary>
        public void Reset()
        {
            if (_isDisposed)
                return;

            if (_audioFileReader != null)
            {
                _audioFileReader.CurrentTime = TimeSpan.Zero;
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            try
            {
                if (_wavePlayer != null)
                {
                    _wavePlayer.Stop();
                    _wavePlayer.Dispose();
                    _wavePlayer = null;
                }

                if (_audioFileReader != null)
                {
                    _audioFileReader.Dispose();
                    _audioFileReader = null;
                }

                _isDisposed = true;
            }
            catch
            {
                // Suppress exceptions during disposal
            }
        }
    }
}
