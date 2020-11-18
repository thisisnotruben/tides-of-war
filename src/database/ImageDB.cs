using System.Collections.Generic;
using System;
using Godot;
namespace Game.Database
{
	public static class ImageDB
	{
		public class ImageData
		{
			public readonly int total, moving, dying, attacking;
			public readonly string weapon, swing, body, weaponMaterial;
			public readonly bool melee;

			public ImageData(int total, int moving, int dying, int attacking,
			string weapon, string swing, string body, string weaponMaterial, bool melee)
			{
				this.total = total;
				this.moving = moving;
				this.dying = dying;
				this.attacking = attacking;
				this.weapon = weapon;
				this.swing = swing;
				this.body = body;
				this.weaponMaterial = weaponMaterial;
				this.melee = melee;
			}
		}
		private static Dictionary<string, ImageData> imageData = new Dictionary<string, ImageData>();

		public static void Init() { LoadImageData(PathManager.image); }
		private static Dictionary<string, ImageData> LoadImageData(string path)
		{
			File file = new File();
			file.Open(path, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Dictionary<string, ImageData> imageData = new Dictionary<string, ImageData>();

			Godot.Collections.Dictionary dict, rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			int moving, dying, attacking;
			foreach (string imgName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[imgName];

				moving = (int)(Single)dict[nameof(ImageData.moving)];
				dying = (int)(Single)dict[nameof(ImageData.dying)];
				attacking = (int)(Single)dict[nameof(ImageData.attacking)];

				imageData.Add(imgName, new ImageData(
					total: moving + dying + attacking,
					moving: moving,
					dying: dying,
					attacking: attacking,
					weapon: (string)dict[nameof(ImageData.weapon)],
					weaponMaterial: (string)dict[nameof(ImageData.weaponMaterial)],
					swing: (string)dict[nameof(ImageData.swing)],
					body: (string)dict[nameof(ImageData.body)],
					melee: (bool)dict[nameof(ImageData.melee)]
				));
			}
			return imageData;
		}
		public static ImageData GetImageData(string imageName) { return imageData[imageName]; }
	}
}