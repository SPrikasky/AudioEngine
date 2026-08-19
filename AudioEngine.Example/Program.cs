using AudioEngine.Core;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AudioEngine.Example
{
    /// <summary>
    /// Example demonstrating the AudioEngine with all 12 sound slots.
    /// Shows how to initialize, load, and play sounds with minimal latency.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║       AudioEngine - Game Sound Manager v1.0        ║");
            Console.WriteLine("║        High-Performance, Low-Latency Audio         ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝\n");

            try
            {
                // Create and initialize the audio manager
                var audioManager = AudioManager.Instance;
                string soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds");

                Console.WriteLine("Initializing AudioEngine...");
                
                // Create sounds directory if it doesn't exist
                if (!Directory.Exists(soundPath))
                {
                    Directory.CreateDirectory(soundPath);
                    Console.WriteLine($"✓ Created sounds directory: {soundPath}");
                    Console.WriteLine("  Please add your WAV files here.\n");
                }

                audioManager.Initialize(soundPath);
                Console.WriteLine("✓ AudioEngine initialized successfully.\n");

                // Run interactive menu
                RunInteractiveMode(audioManager);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static void RunInteractiveMode(AudioManager audioManager)
        {
            var config = new AudioConfiguration();
            bool running = true;

            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("Available Commands:");
            Console.WriteLine("  load <slot> <filename> - Load a sound file");
            Console.WriteLine("  play <slot>            - Play a sound");
            Console.WriteLine("  stop <slot>            - Stop a sound");
            Console.WriteLine("  stopall                - Stop all sounds");
            Console.WriteLine("  volume <slot> <0-100>  - Set volume");
            Console.WriteLine("  status <slot>          - Show sound status");
            Console.WriteLine("  list                   - List all slots");
            Console.WriteLine("  demo                   - Run demo sequence");
            Console.WriteLine("  exit                   - Exit program");
            Console.WriteLine("═══════════════════════════════════════════════════\n");

            while (running)
            {
                Console.Write("AudioEngine> ");
                string input = Console.ReadLine() ?? "";
                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0)
                    continue;

                try
                {
                    switch (parts[0].ToLower())
                    {
                        case "load":
                            HandleLoad(audioManager, parts);
                            break;

                        case "play":
                            HandlePlay(audioManager, parts);
                            break;

                        case "stop":
                            HandleStop(audioManager, parts);
                            break;

                        case "stopall":
                            audioManager.StopAllSounds();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("✓ All sounds stopped.");
                            Console.ResetColor();
                            break;

                        case "volume":
                            HandleVolume(audioManager, parts);
                            break;

                        case "status":
                            HandleStatus(audioManager, parts);
                            break;

                        case "list":
                            DisplaySlotList(config);
                            break;

                        case "demo":
                            RunDemo(audioManager);
                            break;

                        case "exit":
                            running = false;
                            break;

                        default:
                            Console.WriteLine("Unknown command. Type 'help' for available commands.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"✗ Error: {ex.Message}");
                    Console.ResetColor();
                }

                Console.WriteLine();
            }

            audioManager.Dispose();
        }

        static void HandleLoad(AudioManager audioManager, string[] parts)
        {
            if (parts.Length < 3)
            {
                Console.WriteLine("Usage: load <slot> <filename>");
                return;
            }

            if (!int.TryParse(parts[1], out int slotId))
            {
                Console.WriteLine("Invalid slot ID. Must be 0-11.");
                return;
            }

            string fileName = parts[2];
            audioManager.LoadSound(slotId, fileName);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Loaded '{fileName}' into slot {slotId}");
            Console.ResetColor();
        }

        static void HandlePlay(AudioManager audioManager, string[] parts)
        {
            if (parts.Length < 2)
            {
                Console.WriteLine("Usage: play <slot>");
                return;
            }

            if (!int.TryParse(parts[1], out int slotId))
            {
                Console.WriteLine("Invalid slot ID. Must be 0-11.");
                return;
            }

            audioManager.PlaySound(slotId);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"▶ Playing slot {slotId}");
            Console.ResetColor();
        }

        static void HandleStop(AudioManager audioManager, string[] parts)
        {
            if (parts.Length < 2)
            {
                Console.WriteLine("Usage: stop <slot>");
                return;
            }

            if (!int.TryParse(parts[1], out int slotId))
            {
                Console.WriteLine("Invalid slot ID. Must be 0-11.");
                return;
            }

            audioManager.StopSound(slotId);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⏹ Stopped slot {slotId}");
            Console.ResetColor();
        }

        static void HandleVolume(AudioManager audioManager, string[] parts)
        {
            if (parts.Length < 3)
            {
                Console.WriteLine("Usage: volume <slot> <0-100>");
                return;
            }

            if (!int.TryParse(parts[1], out int slotId))
            {
                Console.WriteLine("Invalid slot ID. Must be 0-11.");
                return;
            }

            if (!int.TryParse(parts[2], out int volumePercent))
            {
                Console.WriteLine("Invalid volume. Must be 0-100.");
                return;
            }

            float volume = volumePercent / 100f;
            audioManager.SetVolume(slotId, volume);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"🔊 Slot {slotId} volume set to {volumePercent}%");
            Console.ResetColor();
        }

        static void HandleStatus(AudioManager audioManager, string[] parts)
        {
            if (parts.Length < 2)
            {
                Console.WriteLine("Usage: status <slot>");
                return;
            }

            if (!int.TryParse(parts[1], out int slotId))
            {
                Console.WriteLine("Invalid slot ID. Must be 0-11.");
                return;
            }

            bool isPlaying = audioManager.IsPlaying(slotId);
            float volume = audioManager.GetVolume(slotId);
            int duration = audioManager.GetDuration(slotId);

            Console.WriteLine($"\nSlot {slotId} Status:");
            Console.WriteLine($"  Playing: {(isPlaying ? "Yes" : "No")}");
            Console.WriteLine($"  Volume: {(int)(volume * 100)}%");
            Console.WriteLine($"  Duration: {duration}ms");
        }

        static void DisplaySlotList(AudioConfiguration config)
        {
            Console.WriteLine("\n╔════════════════════════════════════════╗");
            Console.WriteLine("║      Audio Engine Sound Slots          ║");
            Console.WriteLine("╠════════════════════════════════════════╣");

            for (int i = 0; i < 12; i++)
            {
                string slotName = config.GetSlotName(i);
                Console.WriteLine($"║ Slot {i:D2}: {slotName,-30} ║");
            }

            Console.WriteLine("╚════════════════════════════════════════╝\n");
        }

        static void RunDemo(AudioManager audioManager)
        {
            Console.WriteLine("\n╔════════════════════════════════════════╗");
            Console.WriteLine("║      AudioEngine Demo Sequence         ║");
            Console.WriteLine("╚════════════════════════════════════════╝\n");

            Console.WriteLine("This demo shows the capabilities of the AudioEngine:");
            Console.WriteLine("• Low-latency playback");
            Console.WriteLine("• Concurrent sound playing");
            Console.WriteLine("• Volume control");
            Console.WriteLine("• Sound slot management\n");

            Console.WriteLine("Demo Instructions:");
            Console.WriteLine("1. First, load some WAV files into slots using: load <slot> <filename>");
            Console.WriteLine("2. Then run 'demo' again to hear the sequence\n");

            Console.WriteLine("To create test sounds, you can use free tools like:");
            Console.WriteLine("• Audacity (https://www.audacityteam.org/)");
            Console.WriteLine("• Generate tone sounds and export as WAV format\n");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Demo requires pre-loaded sounds. Please load sounds first.");
            Console.ResetColor();
        }
    }
}
