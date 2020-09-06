using Godot;
namespace Game.Actor.State
{
	public abstract class StateBehavior : Node
	{
		private protected FSM fsm;
		private protected Character character;

		public void Init(FSM FSM, Character character)
		{
			this.fsm = FSM;
			this.character = character;
		}
		public abstract void Start();
		public abstract void Exit();
		public virtual void UnhandledInput(InputEvent @event)
		{
			if (!(@event is InputEventScreenTouch))
			{
				return;
			}
			else if (!@event.IsPressed() || @event.IsEcho())
			{
				return;
			}

			if (Map.Map.map.IsValidMove(character.GetGlobalMousePosition()))
			{
				// any valid move overrides any state from player
				fsm.ChangeState((fsm.IsDead())
					? FSM.State.PLAYER_MOVE_DEAD
					: FSM.State.PLAYER_MOVE);
			}
		}
	}
}