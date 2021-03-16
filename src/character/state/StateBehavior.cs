using Godot;
using GC = Godot.Collections;
using Game.Database;
namespace Game.Actor.State
{
	public abstract class StateBehavior : Node, ISerializable
	{
		public FSM fsm;
		protected Character character
		{
			get
			{
				return fsm.character;
			}
		}

		[Signal] public delegate void PlayerWantsToMove(Vector2 desiredPosition);

		public abstract void Start();
		public abstract void Exit();
		public virtual void UnhandledInput(InputEvent @event)
		{
			if (@event is InputEventScreenTouch && @event.IsPressed()
			&& Map.Map.map.IsValidMove(character.GlobalPosition, character.GetGlobalMousePosition()))
			{
				EmitSignal(nameof(PlayerWantsToMove), character.GetGlobalMousePosition());
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
		public virtual GC.Dictionary Serialize()
		{
			return new GC.Dictionary()
			{
				{NameDB.SaveTag.ANIM_POSITION,
					!character.anim.CurrentAnimation.Equals(string.Empty)
						? character.anim.CurrentAnimationPosition
						: 0.0f
				}
			};
		}
		public virtual void Deserialize(GC.Dictionary payload) { }
	}
}