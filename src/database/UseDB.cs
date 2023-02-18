using Godot;
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

		public UseDB(string path) : base(path) { }
		public override void LoadData(string path)
		{
			Godot.Collections.Dictionary dict, rawDict = LoadJson(path);
			foreach (string worldName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[worldName];

				data.Add(worldName, new Use(
					totalSec: dict[nameof(Use.totalSec)].ToString().ToInt(),
					repeatSec: dict[nameof(Use.repeatSec)].ToString().ToInt(),
					hp: ModDB.GetModifier(dict, nameof(Use.hp)),
					mana: ModDB.GetModifier(dict, nameof(Use.mana)),
					damage: ModDB.GetModifier(dict, nameof(Use.damage))
				));
			}
		}
	}
}