using Godot;
namespace Game.Database
{
	public class ImageDB : AbstractDB<ImageDB.ImageData>
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

		public ImageDB(string path) : base(path) { }
		public override void LoadData(string path)
		{
			Godot.Collections.Dictionary dict, rawDict = LoadJson(path);
			int moving, dying, attacking;
			foreach (string imgName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[imgName];

				moving = dict[nameof(ImageData.moving)].ToString().ToInt();
				dying = dict[nameof(ImageData.dying)].ToString().ToInt();
				attacking = dict[nameof(ImageData.attacking)].ToString().ToInt();

				data.Add(imgName, new ImageData(
					total: moving + dying + attacking,
					moving: moving,
					dying: dying,
					attacking: attacking,
					weapon: dict[nameof(ImageData.weapon)].ToString(),
					weaponMaterial: dict[nameof(ImageData.weaponMaterial)].ToString(),
					swing: dict[nameof(ImageData.swing)].ToString(),
					body: dict[nameof(ImageData.body)].ToString(),
					melee: (bool)dict[nameof(ImageData.melee)]
				));
			}
		}
	}
}