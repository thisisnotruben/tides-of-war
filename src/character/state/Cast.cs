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
			character.anim.Connect("animation_finished", this, nameof(OnCastFinished));
		}
		public override void Exit()
		{
			if (character.anim.IsConnected("animation_finished", this, nameof(OnCastFinished)))
			{
				character.anim.Disconnect("animation_finished", this, nameof(OnCastFinished));
				Map.Map.map.OccupyCell(character.GlobalPosition, false);
			}
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