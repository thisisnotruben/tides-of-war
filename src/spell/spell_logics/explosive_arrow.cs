using Game.Actor;
using Game.Actor.Doodads;
using Game.Actor.State;
using Godot;
namespace Game.Ability
{
	public class explosive_arrow : Spell
	{
		private int damage = 10;

		public override float Cast()
		{
			foreach (Area2D characterArea2D in GetNode<Area2D>("sight").GetOverlappingAreas())
			{
				Character character = characterArea2D.Owner as Character;
				if (character != null && character != caster)
				{
					character.Harm(damage);
					character.SpawnCombatText(damage.ToString(), CombatText.TextType.HIT);
				}
			}
			return base.Cast();
		}
		public override void ConfigureSpell()
		{
			caster.SetCurrentSpell(this);
			caster.state = FSM.State.IDLE;
			PrepSight();
		}
	}
}