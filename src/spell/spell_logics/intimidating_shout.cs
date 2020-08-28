using System;
using System.Collections.Generic;
using Game.Actor;
using Godot;
namespace Game.Ability
{
	public class intimidating_shout : Spell
	{
		private Dictionary<Character, Tuple<int, int>> targetList = new Dictionary<Character, Tuple<int, int>>();
		public override float Cast()
		{
			if (loaded)
			{
				foreach (Character character in targetList.Keys)
				{
					// character.minDamage -= targetList[character].Item1;TODO
					// character.maxDamage -= targetList[character].Item2;TODO
				}
			}
			else
			{
				foreach (Area2D characterArea2D in GetNode<Area2D>("sight").GetOverlappingAreas())
				{
					Character character = characterArea2D.Owner as Character;
					if (character != null && character != caster)
					{
						int amount1 = (int)Math.Round(character.stats.minDamage.valueI * 0.20f);
						int amount2 = (int)Math.Round(character.stats.maxDamage.valueI * 0.20f);
						targetList.Add(character, new Tuple<int, int>(amount1, amount2));
					}
				}
			}
			SetTime(3.0f);
			return base.Cast();
		}
		public override void _OnTimerTimeout()
		{
			foreach (Character character in targetList.Keys)
			{
				// character.minDamage += targetList[character].Item1;TODO
				// character.maxDamage += targetList[character].Item2;TODO
			}
			UnMake();
		}
	}
}