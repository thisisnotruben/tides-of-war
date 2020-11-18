using Godot;
using System.Collections.Generic;
namespace Game.Sound
{
	public class SoundPlayer : Godot.Object
	{
		public static readonly SoundPlayer INSTANCE = new SoundPlayer();
		private static readonly Dictionary<string, AudioStream> library = new Dictionary<string, AudioStream>();
		private const byte INIT_PLAYERS = 10;

		private List<AudioStreamPlayer> players = new List<AudioStreamPlayer>();


		static SoundPlayer() { LoadSoundLibrary(); }
		private SoundPlayer()
		{
			for (int i = 0; i < INIT_PLAYERS; i++)
			{
				players.Add(new AudioStreamPlayer());
			}
		}
		private static void LoadSoundLibrary(string path = "res://asset/snd")
		{
			library.Clear();

			string importExt = ".import";

			Directory directory = new Directory();
			directory.Open(path);
			directory.ListDirBegin(true, true);

			string resourceName = directory.GetNext();
			string resourcePath;

			// recursively loop through directory for sound files
			while (!resourceName.Empty())
			{
				resourcePath = path.PlusFile(resourceName);
				if (directory.CurrentIsDir())
				{
					LoadSoundLibrary(resourcePath);
				}
				else if (!resourcePath.Contains(importExt))
				{
					library.Add(resourceName.BaseName(), (AudioStream)GD.Load(resourcePath));
				}
				resourceName = directory.GetNext();
			}
			directory.ListDirEnd();
		}
		public void PlaySound(string soundName)
		{
			if (!library.ContainsKey(soundName))
			{
				return;
			}

			players.ForEach(p =>
			{
				if (!p.Playing)
				{
					p.Stream = library[soundName];
					p.Play();
					return;
				}
			});

			AudioStreamPlayer player = new AudioStreamPlayer();
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