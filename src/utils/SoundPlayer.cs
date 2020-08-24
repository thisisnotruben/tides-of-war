using System.Collections.Generic;
using Godot;
namespace Game.Utils
{
	public abstract class SoundPlayer : Node
	{
		public enum SoundType { SPECIFIC, RANDOM }
		public static Dictionary<string, AudioStream> sndCache = new Dictionary<string, AudioStream>();

		static SoundPlayer()
		{
			LoadSoundLibrary();
		}
		private static void LoadSoundLibrary(string path = "res://asset/snd")
		{
			sndCache.Clear();

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
					sndCache.Add(resourceName.BaseName(), (AudioStream)GD.Load(resourcePath));
				}
				resourceName = directory.GetNext();
			}
			directory.ListDirEnd();
		}
		public static void PlaySound(string sndName, SoundType soundType = SoundType.SPECIFIC)
		{
			if (!sndCache.ContainsKey(sndName))
			{
				return;
			}
		}
		public static void PlaySound(string sndName, Node originator)
		{
			if (!sndCache.ContainsKey(sndName))
			{
				return;
			}
		}
		public static void PlayWeaponSound(string sndName)
		{

		}
		public static void PlayWeaponMiss(string sndName)
		{

		}
	}
}