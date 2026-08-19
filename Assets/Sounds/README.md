# AudioEngine Sounds Directory

This directory should contain your WAV audio files for the game.

## How to Add Sounds

1. Place your WAV files in this directory
2. Load them using AudioManager in your code:

```csharp
var audioManager = AudioManager.Instance;
audioManager.Initialize(@"Assets/Sounds/");

// Load sounds into slots
audioManager.LoadSound(0, "jump.wav");
audioManager.LoadSound(1, "explosion.wav");
audioManager.LoadSound(2, "coin.wav");
// ... etc
```

## Sound File Requirements

- **Format**: WAV (Waveform Audio File Format)
- **Sample Rate**: 44.1 kHz or 48 kHz recommended
- **Bit Depth**: 16-bit or 24-bit
- **Channels**: Mono or Stereo
- **File Size**: Keep individual files under 1 MB for best performance

## Recommended Sound Slots

| Slot | Recommended Use | Example |
|------|-----------------|---------|
| 0 | Jump/Movement | jump.wav |
| 1 | Explosion/Impact | explosion.wav |
| 2 | Coin/Pickup | coin.wav |
| 3 | Power-up | powerup.wav |
| 4 | Enemy Sound | enemy.wav |
| 5 | Background Effect | ambient.wav |
| 6 | UI Interaction | click.wav |
| 7 | Damage/Hit | damage.wav |
| 8 | Level Complete | levelup.wav |
| 9 | Game Over | gameover.wav |
| 10 | Menu Select | select.wav |
| 11 | Alert/Warning | alert.wav |

## Tools to Create Test Sounds

- **Audacity** (Free) - https://www.audacityteam.org/
  - Generate tones
  - Record audio
  - Export as WAV
  
- **Bfxr** (Free) - https://www.bfxr.net/
  - Generate retro 8-bit sound effects
  - Perfect for game sounds
  
- **Sfxia** (Free) - https://rxi.itch.io/sfxia
  - Lightweight sound effect generator

## Performance Tips

1. Pre-load all sounds during initialization
2. Use compressed WAV files if file size is a concern
3. Test concurrent playback of all 12 sounds
4. Monitor memory usage on target platform
