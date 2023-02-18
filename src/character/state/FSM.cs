using System.Collections.Generic;
using System.Linq;
using System;
using Godot;
using GC = Godot.Collections;
using Game.Ability;
using Game.Projectile;
using Game.Database;
using Game.Factory;
namespace Game.Actor.State
{
	public class FSM : Node, ISerializable
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

		public Character character;
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
			this.character = character;

			bool isPlayer = character is Player;
			foreach (StateBehavior stateBehavior in GetChildren())
			{
				stateBehavior.fsm = this;

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

			// call when map/character has finished loading
			CallDeferred(nameof(DeferredSetState),
				Globals.unitDB.HasData(character.Name) && Globals.unitDB.GetData(character.Name).path.Length > 0
				? State.NPC_MOVE_ROAM
				: State.IDLE);
		}
		private void DeferredSetState(State state)
		{
			// if came from save file, then this wouldn't override saved state
			if (stateHistory.Count == 0)
			{
				SetState(state);
			}
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
				((Attack)stateMap[state]).SetSpell(spell);
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
					lastState = Globals.unitDB.HasData(character.Name)
						&& Globals.unitDB.GetData(character.Name).path.Length > 0
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
		public bool ShouldSerialize() { return stateMap[GetState()].Serialize().Count > 0; }
		public GC.Dictionary Serialize()
		{
			return new GC.Dictionary()
			{
				{NameDB.SaveTag.INDEX, GetState().ToString()},
				{NameDB.SaveTag.STATE, stateMap[GetState()].Serialize()}
			};
		}
		public void Deserialize(GC.Dictionary payload)
		{
			State state = (State)Enum.Parse(typeof(State), payload[NameDB.SaveTag.INDEX].ToString());
			GC.Dictionary package = (GC.Dictionary)payload[NameDB.SaveTag.STATE];
			StateBehavior stateBehavior = stateMap[state];

			string spellKey = NameDB.SaveTag.SPELL;
			switch (state)
			{
				case State.ATTACK:
					if (payload.Contains(spellKey))
					{
						((Attack)stateBehavior).SetSpell(new SpellFactory().Make(
							character, payload[NameDB.SaveTag.SPELL].ToString()));
						break;
					}
					break;

				case State.CAST:
					if (payload.Contains(spellKey))
					{
						((Cast)stateBehavior).SetSpell(new SpellFactory().Make(
							character, payload[NameDB.SaveTag.SPELL].ToString()));
					}
					break;

				case State.PLAYER_MOVE:
					MovePlayer movePlayer = (MovePlayer)stateBehavior;
					GC.Array pos = (GC.Array)package[NameDB.SaveTag.POSITION];
					movePlayer.desiredPosition = new Vector2((float)pos[0], (float)pos[1]);
					break;

				case State.NPC_MOVE_ROAM:
					MoveNpcRoam moveNpcRoam = (MoveNpcRoam)stateBehavior;
					moveNpcRoam.waypoints.Clear();
					GC.Array wayPointArr = (GC.Array)package[NameDB.SaveTag.WAY_POINTS];
					for (int i = 0; i < wayPointArr.Count - 1; i += 2)
					{
						moveNpcRoam.waypoints.Enqueue(new Vector2((float)wayPointArr[i], (float)wayPointArr[i + 1]));
					}
					break;
			}

			ChangeState(state);
			stateBehavior.Deserialize(package);
		}
	}
}