using System.Collections.Generic;
using System;
namespace Game.Actor
{
	public class DropSystem
	{
		private Character character;
		private readonly List<string> addedDrops = new List<string>();

		public DropSystem(Character character) { this.character = character; }
		public void AddDrop(params string[] gameItems)
		{
			foreach (string gameItem in gameItems)
			{
				addedDrops.Add(gameItem);
			}
		}
		public void ClearAddedDrops() { addedDrops.Clear(); }
		public void Drop()
		{
			Random rand = new Random();
			if (Globals.unitDB.GetData(character.Name).dropRate > rand.NextDouble())
			{
				return;
			}

			List<string> dropItems = new List<string>();
			double dropChance = rand.NextDouble();

			dropItems.AddRange(addedDrops);
			addedDrops.Clear();
			foreach (string gameItem in Globals.itemDB.data.Keys)
			{
				if (Globals.itemDB.GetData(gameItem).dropRate <= dropChance
				&& !dropItems.Contains(gameItem))
				{
					dropItems.Add(gameItem);
				}
			}

			if (dropItems.Count > 0)
			{
				Map.Map.map.AddDrop(character.GlobalPosition, dropItems[rand.Next(dropItems.Count)]);
			}
		}
	}
}
