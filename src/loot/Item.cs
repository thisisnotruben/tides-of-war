using System;
using System.Collections.Generic;
using Game.Actor;
using Game.Actor.State;
using Game.Database;
using Game.Ui;
using Godot;
namespace Game.Loot
{
	public class Item : Pickable
	{
		public const float MAX_DURABILITY = 1.0f;
		public float durability { get; private set; }
		private int minValue;
		private int maxValue;
		public int value { get; private set; }

		public override void Init(string worldName)
		{
			durability = MAX_DURABILITY;
			ItemDB.ItemNode itemData = ItemDB.GetItemData(worldName);
			this.worldName = worldName;
			Name = worldName;
			// TODO
			// worldType = (WorldTypes)Enum.Parse(typeof(WorldTypes), itemData.type.ToUpper());
			if (worldType == WorldTypes.WEAPON || worldType == WorldTypes.POTION)
			{
				subType = (WorldTypes)Enum.Parse(typeof(WorldTypes), itemData.subType.ToUpper());
			}
			Tuple<int, int> itemStats = Stats.GetItemStats(itemData.level, worldType, subType);
			string durabilitytext = $"\n-Durability: {(durability * 100.0f).ToString("00")}%";
			switch (worldType)
			{
				case WorldTypes.WEAPON:
					minValue = itemStats.Item1;
					maxValue = itemStats.Item2;
					// TODO: taking this
					// menuDescription = $"-Damage: {minValue} - {maxValue}" + durabilitytext;
					break;
				case WorldTypes.ARMOR:
					value = itemStats.Item1;
					// menuDescription = $"-Armor: {value}" + durabilitytext;
					break;
				case WorldTypes.FOOD:
					minValue = itemStats.Item1;
					maxValue = itemStats.Item2;
					// menuDescription = $"-Restores {minValue} - {maxValue} health";
					break;
				case WorldTypes.POTION:
					duration = 120.0f;
					switch (subType)
					{
						case WorldTypes.HEALING:
							// menuDescription = $"-Restores {minValue} - {maxValue} health";
							minValue = itemStats.Item1;
							maxValue = itemStats.Item2;
							break;
						case WorldTypes.MANA:
							// menuDescription = $"-Restores {minValue} - {maxValue} mana";
							minValue = itemStats.Item1;
							maxValue = itemStats.Item2;
							break;
						default:
							value = itemStats.Item1;
							break;
					}
					// menuDescription = $"-Grants {value} {subType.ToString().ToLower()}\nfor {duration.ToString("0.00")} seconds.";
					break;
			}
			// menuDescription += $"\n-Gold: {goldWorth}";
		}
		public override void _OnTimerTimeout()
		{
			if (Owner is Character)
			{
				ConfigureBuff((Character)Owner, true);
			}
		}
		public Tuple<int, int> GetValues()
		{
			return new Tuple<int, int>(minValue, maxValue);
		}
		public void Consume(Character character, float seek)
		{
			GD.Randomize();
			int amount = (int)Math.Round(GD.RandRange((double)minValue, (double)maxValue));
			switch (subType)
			{
				case WorldTypes.HEALING:
					character.hp = amount;
					break;
				case WorldTypes.MANA:
					character.mana = amount;
					break;
			}
			if (worldType == WorldTypes.POTION)
			{
				ConfigureBuff(character, false);
				if (GetTree().Paused)
				{
					// character.buffs["pending"].Add(this);
				}
				else
				{
					// character.SetBuff(new List<Item> { this }, seek);
				}
			}
			else
			{
				UnMake();
			}
		}
		public void ConfigureBuff(Character character, bool expire = false)
		{
			if (character is Player)
			{
				// Player player = (Player)character;
				// if (IsConnected(nameof(SetInMenu), player.GetMenu(), nameof(InGameMenu._OnSetPickableInMenu)))
				// {
				// 	Disconnect(nameof(SetInMenu), player.GetMenu(), nameof(InGameMenu._OnSetPickableInMenu));
				// }
			}
			if (expire)
			{
				value *= -1;
			}
			double percent;
			switch (subType)
			{
				case WorldTypes.STAMINA:
					percent = (double)character.hp / (double)character.stats.hpMax.value;
					// character.hpMax += value;TODO
					character.hp = (int)Math.Round(percent * (double)character.stats.hpMax.value);
					break;
				case WorldTypes.INTELLECT:
					percent = (double)character.mana / (double)character.stats.manaMax.value;
					// character.manaMax += value;
					character.mana = (int)Math.Round(percent * (double)character.stats.manaMax.value);
					break;
				case WorldTypes.AGILITY:
					int regenAmount = Stats.GetModifiedRegen(character.level,
						Stats.GetMultiplier(character is Npc, character.img.Texture.ResourcePath));
					// character.regenTime = regenAmount;
					if (character.state != FSM.State.ATTACK)
					{
						// TODO: deprecated way to do this
						// character.SetTime(character.regenTime, false);
					}
					break;
				case WorldTypes.STRENGTH:
					// character.minDamage += value;TODO
					// character.maxDamage += value;TODO
					break;
				case WorldTypes.DEFENSE:
					// character.armor += value;TODO
					break;
			}
			if (expire)
			{
				// character.buffs["active"].Remove(this);
				UnMake();
			}
		}
		public void RepairItem(float amount)
		{
			durability += Math.Abs(amount);
			if (durability > 1.0f)
			{
				durability = 1.0f;
			}
			GD.Print("TODO: Not Implemented");
		}
		public void TakeDamage(bool byPass = false, float damageAmount = 0.1f)
		{
			GD.Randomize();
			if (byPass || GD.Randi() % 100 + 1 <= 10)
			{
				float oldDurability = durability;
				durability -= Math.Abs(damageAmount);
				if (durability > MAX_DURABILITY)
				{
					durability = MAX_DURABILITY;
				}
				else if (durability < 0.0f)
				{
					durability = 0.0f;
				}
				// use regex to find old durability string and replace with new durability string
				// if anything, this whole function needs rewriting
				if (durability >= 0.5f)
				{
					// goldWorth = Stats.GetItemGoldWorth(ItemDB.GetItemData(worldName).level, worldType, durability);
					switch (worldType)
					{
						case WorldTypes.WEAPON:
							break;
						case WorldTypes.ARMOR:
							break;
					}
				}
			}
			GD.Print("TODO: Not Implemented");
		}
	}
}