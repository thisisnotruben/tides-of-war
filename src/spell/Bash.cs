using Game.Actor.State;
using Game.Actor;
namespace Game.Ability
{
	public class Bash : SpellProto
	{
		public Bash(Character character, string worldName) : base(character, worldName) { }
		public override void Start()
		{
			base.Start();
			character.state = FSM.State.STUN;
			character.fsm.lockState = true;
		}
		public override void Exit()
		{
			base.Exit();
			character.fsm.lockState = false;
			character.fsm.RevertState();
		}
	}

}