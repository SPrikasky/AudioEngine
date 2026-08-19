# AudioEngine Integration Guide

This guide shows how to integrate AudioEngine into your C# game project.

## Installation

### Step 1: Add Reference to Your Project

```xml
<ProjectReference Include="path/to/AudioEngine/AudioEngine.Core/AudioEngine.Core.csproj" />
<PackageReference Include="NAudio" Version="2.2.1" />
```

### Step 2: Basic Initialization

```csharp
using AudioEngine.Core;

public class GameManager
{
    private AudioManager _audioManager;

    public void InitializeGame()
    {
        // Get the singleton instance
        _audioManager = AudioManager.Instance;
        
        // Initialize with sound directory
        string soundPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, 
            "Assets/Sounds"
        );
        _audioManager.Initialize(soundPath);
        
        // Load your sounds
        LoadGameSounds();
    }

    private void LoadGameSounds()
    {
        // Load sounds during initialization (not during gameplay)
        _audioManager.LoadSound(0, "jump.wav");
        _audioManager.LoadSound(1, "explosion.wav");
        _audioManager.LoadSound(2, "coin.wav");
        _audioManager.LoadSound(3, "powerup.wav");
        _audioManager.LoadSound(4, "enemy.wav");
        _audioManager.LoadSound(5, "ambient.wav");
        _audioManager.LoadSound(6, "click.wav");
        _audioManager.LoadSound(7, "damage.wav");
        _audioManager.LoadSound(8, "levelup.wav");
        _audioManager.LoadSound(9, "gameover.wav");
        _audioManager.LoadSound(10, "select.wav");
        _audioManager.LoadSound(11, "alert.wav");
    }
}
```

## Common Use Cases

### Playing Sounds from Game Events

```csharp
public class Player
{
    private AudioManager _audioManager = AudioManager.Instance;

    public void Jump()
    {
        // Play jump sound (slot 0)
        _audioManager.PlaySound(0);
        // ... rest of jump logic
    }

    public void TakeDamage()
    {
        // Play damage sound (slot 7)
        _audioManager.PlaySound(7);
        // ... rest of damage logic
    }
}

public class CoinCollector
{
    private AudioManager _audioManager = AudioManager.Instance;

    public void CollectCoin()
    {
        // Play coin sound (slot 2)
        _audioManager.PlaySound(2);
        Score += 10;
    }
}

public class Explosion
{
    private AudioManager _audioManager = AudioManager.Instance;

    public void Explode()
    {
        // Play explosion sound (slot 1)
        _audioManager.PlaySound(1);
        // Destroy objects, create particles, etc.
    }
}
```

### Volume Control

```csharp
public class GameSettings
{
    private AudioManager _audioManager = AudioManager.Instance;

    public void SetMasterVolume(float volume)
    {
        // Set volume for all sound slots
        for (int i = 0; i < 12; i++)
        {
            _audioManager.SetVolume(i, volume);
        }
    }

    public void SetEffectsVolume(float volume)
    {
        // Set volume for specific slots (e.g., effects)
        for (int i = 0; i < 7; i++)
        {
            _audioManager.SetVolume(i, volume);
        }
    }

    public void SetMusicVolume(float volume)
    {
        // Set volume for music slots
        _audioManager.SetVolume(5, volume);
    }
}
```

### Checking Sound Status

```csharp
public class UIManager
{
    private AudioManager _audioManager = AudioManager.Instance;

    public void UpdateSoundIndicators()
    {
        int playingCount = _audioManager.GetPlayingCount();
        Console.WriteLine($"Playing sounds: {playingCount}/12");

        // Check specific sound
        if (_audioManager.IsPlaying(1))
        {
            Console.WriteLine("Explosion sound is playing");
        }

        // Get sound duration
        int jumpDuration = _audioManager.GetDuration(0);
        Console.WriteLine($"Jump sound duration: {jumpDuration}ms");
    }
}
```

### Stopping Sounds

```csharp
public class BossEncounter
{
    private AudioManager _audioManager = AudioManager.Instance;

    public void StartBoss()
    {
        // Stop all ambient sounds when boss starts
        _audioManager.StopAllSounds();
        
        // Play boss music or alert
        _audioManager.PlaySound(11); // Alert sound
    }

    public void OnBossDefeat()
    {
        _audioManager.StopAllSounds();
        _audioManager.PlaySound(8); // Level complete sound
    }
}
```

## Performance Best Practices

### 1. Pre-load Sounds at Startup

✅ **Good:**
```csharp
// During game initialization
public void Initialize()
{
    _audioManager.Initialize(soundPath);
    _audioManager.LoadSound(0, "jump.wav");
    _audioManager.LoadSound(1, "explosion.wav");
    // Load all sounds upfront
}
```

❌ **Bad:**
```csharp
// During gameplay - causes latency
public void OnPlayerAction()
{
    _audioManager.LoadSound(0, "jump.wav");  // Slow!
    _audioManager.PlaySound(0);
}
```

### 2. Thread-Safe Operations

AudioManager is thread-safe and can be called from any thread:

```csharp
// From game thread
_audioManager.PlaySound(0);

// From audio thread
_audioManager.StopSound(1);

// From UI thread
_audioManager.SetVolume(2, 0.5f);

// All safe - no race conditions
```

### 3. Resource Cleanup

```csharp
public class GameManager : IDisposable
{
    private AudioManager _audioManager = AudioManager.Instance;

    public void OnGameEnd()
    {
        _audioManager.StopAllSounds();
        _audioManager.Dispose();
    }

    public void Dispose()
    {
        _audioManager?.Dispose();
    }
}
```

### 4. Sound File Optimization

- **Sample Rate**: 44.1 kHz (standard for games)
- **Bit Depth**: 16-bit (sufficient quality, smaller file size)
- **File Size**: Keep individual files < 1 MB
- **Duration**: Keep sound clips 0.5-3 seconds for effects

## Troubleshooting

### No Sound Output

```csharp
// 1. Verify sounds are loaded
bool isLoaded = audioManager.GetDuration(0) > 0;

// 2. Check if sound is playing
bool isPlaying = audioManager.IsPlaying(0);

// 3. Verify volume
float volume = audioManager.GetVolume(0);
```

### Audio Latency

```csharp
// Ensure sounds are pre-loaded
// Pre-load during initialization, not gameplay

// Check concurrent playback
int activeCount = audioManager.GetPlayingCount();
if (activeCount > 12)
{
    // Too many sounds playing!
}
```

### File Not Found Error

```csharp
try
{
    _audioManager.LoadSound(0, "jump.wav");
}
catch (FileNotFoundException ex)
{
    // Check file exists and path is correct
    string expectedPath = Path.Combine(soundPath, "jump.wav");
    Console.WriteLine($"Expected: {expectedPath}");
}
```

## API Reference

| Method | Purpose | Thread-Safe |
|--------|---------|-------------|
| `Initialize(path)` | Initialize audio engine | No |
| `LoadSound(slot, file)` | Load a sound file | Yes |
| `PlaySound(slot)` | Play sound immediately | Yes |
| `StopSound(slot)` | Stop sound playback | Yes |
| `StopAllSounds()` | Stop all sounds | Yes |
| `SetVolume(slot, volume)` | Set sound volume | Yes |
| `GetVolume(slot)` | Get current volume | Yes |
| `IsPlaying(slot)` | Check if playing | Yes |
| `GetDuration(slot)` | Get sound duration (ms) | Yes |
| `UnloadSound(slot)` | Unload a sound | Yes |
| `GetPlayingCount()` | Count active sounds | Yes |
| `Dispose()` | Cleanup resources | No |

## Complete Example - Game Class

```csharp
using AudioEngine.Core;
using System;
using System.IO;

public class SimpleGame : IDisposable
{
    private AudioManager _audioManager;

    public void Initialize()
    {
        Console.WriteLine("Initializing game...");
        
        _audioManager = AudioManager.Instance;
        string soundPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Assets/Sounds"
        );
        
        _audioManager.Initialize(soundPath);
        
        // Load all game sounds
        _audioManager.LoadSound(0, "jump.wav");
        _audioManager.LoadSound(1, "explosion.wav");
        _audioManager.LoadSound(2, "coin.wav");
        
        Console.WriteLine("Game initialized!");
    }

    public void GameLoop()
    {
        while (true)
        {
            // Update game state
            Update();
            Render();
        }
    }

    private void Update()
    {
        // Handle player input and game logic
        if (PlayerJumps())
        {
            _audioManager.PlaySound(0);
        }
    }

    private void Render()
    {
        // Render game
    }

    private bool PlayerJumps()
    {
        // Input detection logic
        return false;
    }

    public void Dispose()
    {
        _audioManager?.StopAllSounds();
        _audioManager?.Dispose();
    }
}
```

---

For more examples, see the `AudioEngine.Example` project in the repository.
