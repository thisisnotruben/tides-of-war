using Game.Ability;
namespace Game.Actor.State
{
	public class Cast : TakeDamage
	{
		private Spell spell;

		public override void Start()
		{
			if (spell == null)
			{
				fsm.RevertState();
				return;
			}

			Map.Map.map.OccupyCell(character.GlobalPosition, true);
			Globals.TryLinkSignal(character.anim, "animation_finished", this, nameof(OnCastFinished), true);
		}
		public override void Exit()
		{
			Globals.TryLinkSignal(character.anim, "animation_finished", this, nameof(OnCastFinished), false);
			Map.Map.map.OccupyCell(character.GlobalPosition, false);
		}
		public void OnSetCastSpell(Spell spell) { this.spell = spell; }

		public void OnCastFinished(string animName)
		{
			// TODO: need to find a better way than the animation 
			// finishing as it can be in a loop or part of the spell
			fsm.RevertState();
		}
	}
}