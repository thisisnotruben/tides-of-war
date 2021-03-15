using System.Collections.Generic;
using System.Linq;
using Godot;
using Game.Ability;
using Game.Projectile;
namespace Game.Actor.State
{
	public class FSM : Node
	{
		private const int HISTORY_MAX = 10, HISTORY_CUT = HISTORY_MAX / 2;
		public enum State
		{
			IDLE, ATTACK, ALIVE, DEAD, IDLE_DEAD, STUN, CAST,
			PLAYER_MOVE, PLAYER_MOVE_DEAD,
			NPC_MOVE_ROAM, NPC_MOVE_ATTACK, NPC_MOVE_RETURN
		}
		private readonly Dictionary<State, StateBehavior> stateMap = new Dictionary<State, StateBehavior>();
		private Stack<State> stateHistory = new Stack<State>();

		string characterEditorName = string.Empty;
		public bool lockState;

		[Signal] public delegate void StateChanged(State state);

		public override void _Ready()
		{
			stateMap[State.IDLE] = (Idle)GetChild((int)State.IDLE);
			stateMap[State.ATTACK] = (Attack)GetChild((int)State.ATTACK);
			stateMap[State.ALIVE] = (Alive)GetChild((int)State.ALIVE);
			stateMap[State.DEAD] = (Dead)GetChild((int)State.DEAD);
			stateMap[State.IDLE_DEAD] = (IdleDead)GetChild((int)State.IDLE_DEAD);
			stateMap[State.STUN] = (Stun)GetChild((int)State.STUN);
			stateMap[State.CAST] = (Cast)GetChild((int)State.CAST);
			stateMap[State.PLAYER_MOVE] = (MovePlayer)GetChild((int)State.PLAYER_MOVE);
			stateMap[State.PLAYER_MOVE_DEAD] = (MovePlayerDead)GetChild((int)State.PLAYER_MOVE_DEAD);
			stateMap[State.NPC_MOVE_ROAM] = (MoveNpcRoam)GetChild((int)State.NPC_MOVE_ROAM);
			stateMap[State.NPC_MOVE_ATTACK] = (MoveNpcAttack)GetChild((int)State.NPC_MOVE_ATTACK);
			stateMap[State.NPC_MOVE_RETURN] = (MoveNpcReturn)GetChild((int)State.NPC_MOVE_RETURN);
		}
		public void Init(Character character)
		{
			this.characterEditorName = character.Name;

			bool isPlayer = character is Player;
			foreach (StateBehavior stateBehavior in GetChildren())
			{
				stateBehavior.Init(this, character);

				if (isPlayer
				&& stateBehavior != stateMap[State.PLAYER_MOVE]
				&& stateBehavior != stateMap[State.PLAYER_MOVE_DEAD])
				{
					stateBehavior.Connect(nameof(StateBehavior.PlayerWantsToMove),
						stateMap[State.PLAYER_MOVE], nameof(MovePlayer.OnPlayerWantsToMove));
					stateBehavior.Connect(nameof(StateBehavior.PlayerWantsToMove),
						stateMap[State.PLAYER_MOVE_DEAD], nameof(MovePlayerDead.OnPlayerWantsToMove));
				}
			}
			stateMap[State.ATTACK].Connect(nameof(Attack.CastSpell), stateMap[State.CAST], nameof(Cast.SetSpell));

			// call when map has finished loading
			CallDeferred(nameof(SetState),
				Globals.unitDB.HasData(characterEditorName) && Globals.unitDB.GetData(characterEditorName).path.Length > 0
				? State.NPC_MOVE_ROAM
				: State.IDLE);
		}
		public void ConnectMissileHit(Missile missile, Character character, Character target, Spell spell = null)
		{
			missile.Connect(nameof(Missile.OnHit), stateMap[State.ATTACK], nameof(Attack.AttackHit),
				new Godot.Collections.Array() { character, target, spell });
		}
		public void OnPlayerWantsToCast(Spell spell, bool requiresTarget)
		{
			State state;
			if (requiresTarget)
			{
				state = State.ATTACK;
				((Attack)stateMap[state]).SetPlayerSpell(spell);
				if (state != GetState())
				{
					ChangeState(state);
				}
			}
			else
			{
				state = State.CAST;
				((Cast)stateMap[state]).SetSpell(spell);
				ChangeState(state);
			}
		}
		public void ChangeState(State state)
		{
			if (lockState)
			{
				return;
			}

			stateMap[GetState()].Exit();
			SetState(state);
		}
		private void SetState(State state)
		{
			stateHistory.Push(state);
			stateMap[state].Start();

			EmitSignal(nameof(StateChanged), state);

			if (stateHistory.Count == HISTORY_MAX)
			{
				stateHistory = new Stack<State>(stateHistory.ToArray().Take(HISTORY_CUT));
			}
		}
		public State GetState() { return (stateHistory.Count == 0) ? State.IDLE : stateHistory.Peek(); }
		public void RevertState()
		{
			State currentState = stateHistory.Pop(),
			lastState = stateHistory.Count > 0
				? stateHistory.Peek()
				: State.IDLE;

			switch (lastState)
			{
				case State.DEAD:
					lastState = State.IDLE_DEAD;
					break;

				case State.ALIVE:
					lastState = Globals.unitDB.HasData(characterEditorName)
						&& Globals.unitDB.GetData(characterEditorName).path.Length > 0
						? State.NPC_MOVE_ROAM
						: State.IDLE;
					break;

				case State.STUN:
					lastState = State.IDLE;

					// cycle through until you find a legal state
					Stack<State> prevStates = new Stack<State>();
					while (stateHistory.Count > 0)
					{
						prevStates.Push(stateHistory.Pop());
						if (!IsDead() && prevStates.Peek() != State.PLAYER_MOVE_DEAD)
						{
							lastState = prevStates.Peek();
							break;
						}
					}

					// add states back to correct position
					while (prevStates.Count > 0)
					{
						stateHistory.Push(prevStates.Pop());
					}

					break;
			}

			stateHistory.Push(currentState);
			ChangeState(lastState);
		}
		public bool IsDead()
		{
			switch (GetState())
			{
				case State.DEAD:
				case State.IDLE_DEAD:
				case State.PLAYER_MOVE_DEAD:
					return true;
				default:
					return false;
			}
		}
		public bool IsAtacking()
		{
			switch (GetState())
			{
				case State.ATTACK:
				case State.NPC_MOVE_ATTACK:
					return true;
				default:
					return false;
			}
		}
		public bool IsMoving()
		{
			switch (GetState())
			{
				case State.NPC_MOVE_ATTACK:
				case State.NPC_MOVE_RETURN:
				case State.NPC_MOVE_ROAM:
				case State.PLAYER_MOVE:
				case State.PLAYER_MOVE_DEAD:
					return true;
				default:
					return false;
			}
		}
		// * Delegated functions for state
		public void OnAttacked(Character whosAttacking) { stateMap[GetState()].OnAttacked(whosAttacking); }
		public void Harm(int damage, Vector2 direction) { (stateMap[GetState()] as TakeDamage)?.Harm(damage, direction); }
		public void UnhandledInput(InputEvent @event) { stateMap[GetState()].UnhandledInput(@event); }
	}
}