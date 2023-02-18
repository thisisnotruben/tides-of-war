using Game.Actor.State;
namespace Game.Ability
{
	public class Bash : Spell
	{
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