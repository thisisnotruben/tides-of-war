using System.Collections.Generic;
using Game.Actor.Doodads;
using Game.Actor;
using Godot;
namespace Game.Ability
{
	public class stomp : Spell
	{
		private List<Character> targetList = new List<Character>();
		private int damage = 10;

		public override void _Ready()
		{
			base._Ready();
			GD.Randomize();
		}
		public override float Cast()
		{
			if (loaded)
			{
				foreach (Character character in targetList)
				{
					StunUnit(character, true);
				}
			}
			else
			{
				foreach (Area2D characterArea2D in GetNode<Area2D>("sight").GetOverlappingAreas())
				{
					Character character = characterArea2D.Owner as Character;
					if (character != null && character != caster)
					{
						character.Harm(damage);
						character.SpawnCombatText(damage.ToString(), CombatText.TextType.HIT);

						GD.Randomize();
						if (GD.Randi() % 100 + 1 > 20)
						{
							StunUnit(character, true);
							targetList.Add(character);
						}
					}
				}
			}
			SetTime(3.0f);
			return base.Cast();
		}
		public override void _OnTimerTimeout()
		{
			foreach (Character character in targetList)
			{
				StunUnit(character, false);
			}
			UnMake();
		}
	}
}