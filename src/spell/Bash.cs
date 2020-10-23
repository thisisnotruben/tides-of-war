using Game.Actor.State;
using Game.Actor;
namespace Game.Ability
{
	public class Bash : SpellProto
	{
		private FSM.State previousState;

		public Bash(Character character, string worldName) : base(character, worldName) { }
		public override void Start()
		{
			base.Start();
			previousState = character.state;
			character.state = FSM.State.STUN;
		}
		public override void Exit()
		{
			base.Exit();
			character.state = previousState;
		}
	}
}