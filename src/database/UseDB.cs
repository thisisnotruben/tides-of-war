using System;
namespace Game.Database
{
	public class UseDB : AbstractDB<UseDB.Use>
	{
		public class Use : Godot.Object
		{
			public readonly int totalSec, repeatSec;
			public readonly ModDB.ModifierNode hp, mana, damage;

			public Use(int totalSec, int repeatSec, ModDB.ModifierNode hp, ModDB.ModifierNode mana, ModDB.ModifierNode damage)
			{
				this.totalSec = totalSec;
				this.repeatSec = repeatSec;
				this.hp = hp;
				this.mana = mana;
				this.damage = damage;
			}
		}

		public static readonly UseDB Instance = new UseDB();

		public UseDB() : base(PathManager.use) { }
		public override void LoadData(string path)
		{
			Godot.Collections.Dictionary dict, rawDict = LoadJson(path);
			foreach (string worldName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[worldName];

				data.Add(worldName, new Use(
					totalSec: (int)(Single)dict[nameof(Use.totalSec)],
					repeatSec: (int)(Single)dict[nameof(Use.repeatSec)],
					hp: ModDB.GetModifier(dict, nameof(Use.hp)),
					mana: ModDB.GetModifier(dict, nameof(Use.mana)),
					damage: ModDB.GetModifier(dict, nameof(Use.damage))
				));
			}
		}
	}
}