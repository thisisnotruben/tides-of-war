using Game.Actor;
using Game.Actor.State;
namespace Game.Ability
{
	public class Volley : SpellProto
	{
		private int hit = 3;

		public Volley(Character character, string worldName) : base(character, worldName) { }
		public override void _Ready()
		{
			base._Ready();
			character.fsm.Connect(nameof(FSM.StateChanged), this, nameof(CheckState));
			character.anim.Connect("animation_finished", this, nameof(OnCharacterAnimFinished));
		}
		public void CheckState(FSM.State state)
		{
			if (!state.Equals(FSM.State.ATTACK))
			{
				Exit();
			}
		}
		public void OnCharacterAnimFinished(string animName)
		{
			if (!animName.Equals("attacking") || --hit == 0)
			{
				Exit();
			}
		}
	}
}