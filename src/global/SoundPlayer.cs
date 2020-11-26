using Godot;
using Game.Database;
using System.Collections.Generic;
using System;
using System.Linq;
namespace Game.Sound
{
	public class SoundPlayer : Node
	{
		private static readonly Dictionary<string, AudioStream> library = new Dictionary<string, AudioStream>();
		private const byte INIT_PLAYERS = 10;

		private List<AudioStreamPlayer> players = new List<AudioStreamPlayer>();
		private readonly Random rand = new Random();

		static SoundPlayer() { LoadSoundLibrary(PathManager.sndDir); }
		public override void _Ready()
		{
			Name = nameof(SoundPlayer);
			PauseMode = PauseModeEnum.Process;
			for (int i = 0; i < INIT_PLAYERS; i++)
			{
				players.Add(new AudioStreamPlayer());
				players[i].Name = $"player{i}";
				AddChild(players[i]);
			}
		}
		private static void LoadSoundLibrary(string path)
		{
			Directory directory = new Directory();
			directory.Open(path);
			directory.ListDirBegin(true, true);

			string resourceName = directory.GetNext(),
				resourcePath;

			AudioStream audioStream;
			AudioStreamRandomPitch audioStreamRandomPitch;

			while (!resourceName.Empty())
			{
				// Android workaround for actually loading the sounds
				resourceName = resourceName.Replace("." + PathManager.importExt, string.Empty);

				resourcePath = path.PlusFile(resourceName);
				if (directory.CurrentIsDir())
				{
					LoadSoundLibrary(resourcePath);
				}
				else if (!resourceName.Extension().Equals(PathManager.importExt)
				&& !library.ContainsKey(resourceName.BaseName()))
				{
					audioStream = GD.Load<AudioStream>(resourcePath);
					if (PathManager.randdomSndDirs.Contains(path.GetFile()))
					{
						audioStreamRandomPitch = new AudioStreamRandomPitch();
						audioStreamRandomPitch.AudioStream = audioStream;
						audioStreamRandomPitch.RandomPitch = 1.15f;
						audioStream = audioStreamRandomPitch;
					}

					library.Add(resourceName.BaseName(), audioStream);
				}
				resourceName = directory.GetNext();
			}
			directory.ListDirEnd();
		}
		public void PlaySoundRandomized(string soundName, AudioStreamPlayer2D player2D)
		{
			IEnumerable<string> soundNames =
				from sound in library.Keys
				where sound.Contains(soundName)
				select sound;

			if (soundNames.Any())
			{
				PlaySound(soundNames.ElementAt(rand.Next(soundNames.Count())), player2D);
			}
		}
		public void PlaySound(string soundName, bool on) { PlaySound(soundName + (on ? "_on" : "_off")); }
		public void PlaySound(string soundName)
		{
			if (!library.ContainsKey(soundName))
			{
				return;
			}

			foreach (AudioStreamPlayer p in players)
			{
				if (!p.Playing)
				{
					p.Stream = library[soundName];
					p.Play();
					return;
				}
			}

			AudioStreamPlayer player = new AudioStreamPlayer();

			AddChild(player);
			player.Connect("finished", this, nameof(Delete),
				new Godot.Collections.Array() { player });

			player.Stream = library[soundName];
			player.Play();
		}
		public void PlaySound(string soundName, AudioStreamPlayer2D player2D)
		{
			if (!library.ContainsKey(soundName))
			{
				return;
			}

			if (player2D.Playing)
			{
				AudioStreamPlayer2D player = new AudioStreamPlayer2D();

				player2D.AddChild(player);
				player.Connect("finished", this, nameof(Delete),
					new Godot.Collections.Array() { player });

				player.Stream = library[soundName];
				player.Play();
			}
			else
			{
				player2D.Stream = library[soundName];
				player2D.Play();
			}
		}
		private void Delete(Node player) { player.QueueFree(); }
	}
}