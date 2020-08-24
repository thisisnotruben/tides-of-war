using Godot;
namespace Game.Actor.State
{
	public abstract class StateBehavior : Node
	{
		private protected FSM fsm;
		private protected Character character;
		private protected AnimationPlayer animationPlayer;
		private protected Sprite sprite;

		public void Init(FSM FSM, Character character)
		{
			this.fsm = FSM;
			this.character = character;
			animationPlayer = character.GetNode<AnimationPlayer>("anim");
			sprite = character.GetNode<Sprite>("img");
		}
		public abstract void Start();
		public abstract void Exit();
		public virtual void Process(float delta) { }
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
				FSM.State state = fsm.GetState();
				fsm.ChangeState((state == FSM.State.DEAD
					|| state == FSM.State.PLAYER_DEAD_IDLE
					|| state == FSM.State.PLAYER_DEAD_MOVE)
					? FSM.State.PLAYER_DEAD_MOVE
					: FSM.State.MOVE);
			}
		}
	}
}