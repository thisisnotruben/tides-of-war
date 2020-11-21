using Godot;
using System;
namespace Game.Database
{
	public class MissileSpellDB : AbstractDB<MissileSpellDB.SpellMissileData>
	{
		public class SpellMissileData
		{
			public readonly Shape2D hitBox;
			public readonly Texture img;
			public readonly Boolean rotate;
			public readonly string sound;

			public SpellMissileData(Shape2D hitBox, Texture img, bool rotate, string sound)
			{
				this.hitBox = hitBox;
				this.img = img;
				this.rotate = rotate;
				this.sound = sound;
			}
		}

		public static readonly MissileSpellDB Instance = new MissileSpellDB();

		public MissileSpellDB() : base(PathManager.missileSpell) { }
		public override void LoadData(string path)
		{
			Godot.Collections.Dictionary dict, rawDict = LoadJson(path);
			string filePath = "res://asset/img/missile-spell/{0}.tres";
			foreach (string spellName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[spellName];

				data.Add(spellName, new SpellMissileData(
					hitBox: (Shape2D)GD.Load(string.Format(filePath, (string)dict[nameof(SpellMissileData.hitBox)])),
					img: dict[nameof(SpellMissileData.img)] == null ? null
						: GD.Load<Texture>(string.Format(filePath, (string)dict[nameof(SpellMissileData.img)])),
					rotate: (bool)dict[nameof(SpellMissileData.rotate)],
					sound: (string)dict[nameof(SpellMissileData.sound)]
				));
			}
		}
	}
}