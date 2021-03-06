using Godot;
namespace Game.Actor.State
{
	public abstract class StateBehavior : Node
	{
		protected FSM fsm;
		protected Character character;

		[Signal] public delegate void PlayerWantsToMove(bool wantsToMove);

		public void Init(FSM FSM, Character character)
		{
			this.fsm = FSM;
			this.character = character;
		}
		public abstract void Start();
		public abstract void Exit();
		public virtual void UnhandledInput(InputEvent @event)
		{
			if (@event is InputEventScreenTouch && @event.IsPressed()
			&& Map.Map.map.IsValidMove(character.GlobalPosition, character.GetGlobalMousePosition()))
			{
				EmitSignal(nameof(PlayerWantsToMove), true);
				// any valid move overrides any state from player
				fsm.ChangeState(fsm.IsDead()
							? FSM.State.PLAYER_MOVE_DEAD
							: FSM.State.PLAYER_MOVE);
			}
		}
		public virtual void OnAttacked(Character whosAttacking)
		{
			if (whosAttacking == null)
			{
				return;
			}

			character.regenTimer.Stop();
			character.pursuantUnitIds.Add(whosAttacking.GetInstanceId());

			if (character is Npc
			&& character.target == null
			&& !MoveNpcAttack.OutOfPursuitRange(character, whosAttacking))
			{
				character.target = whosAttacking;
				fsm.ChangeState(
					character.pos.DistanceTo(character.target.pos) > character.stats.weaponRange.value
					? FSM.State.NPC_MOVE_ATTACK
					: FSM.State.ATTACK);
			}
			else if (character is Player
			&& character.target == whosAttacking
			&& character.pos.DistanceTo(whosAttacking.pos) <= character.stats.weaponRange.value)
			{
				character.target = whosAttacking;
				((Player)character).menu.SetTargetDisplay(whosAttacking as Npc);
				fsm.ChangeState(FSM.State.ATTACK);
			}
			else
			{
				ClearOnAttackedSignals(whosAttacking);
			}
		}
		protected void ClearOnAttackedSignals(Character whosAttacking)
		{
			character.pursuantUnitIds.Add(whosAttacking.GetInstanceId());
			if (whosAttacking != null && character.target != whosAttacking
			&& whosAttacking.IsConnected(nameof(Character.NotifyAttack), character, nameof(Character.OnAttacked)))
			{
				whosAttacking.Disconnect(nameof(Character.NotifyAttack), character, nameof(Character.OnAttacked));
			}
		}
	}
}