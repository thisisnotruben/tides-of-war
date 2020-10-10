using System.Collections.Generic;
using Game.Database;
using Godot;
namespace Game.Actor.State
{
	public class FSM : Node
	{
		private const int MAX_HISTORY = 10;
		public enum State
		{
			IDLE, ATTACK, ALIVE, DEAD, IDLE_DEAD, STUN,
			PLAYER_MOVE, PLAYER_MOVE_DEAD,
			NPC_MOVE_ROAM, NPC_MOVE_ATTACK, NPC_MOVE_RETURN
		}
		private readonly Dictionary<State, StateBehavior> stateMap = new Dictionary<State, StateBehavior>();
		private readonly Stack<State> stateHistory = new Stack<State>();

		public override void _Ready()
		{
			stateMap[State.IDLE] = (Idle)GetChild((int)State.IDLE);
			stateMap[State.ATTACK] = (Attack)GetChild((int)State.ATTACK);
			stateMap[State.ALIVE] = (Alive)GetChild((int)State.ALIVE);
			stateMap[State.DEAD] = (Dead)GetChild((int)State.DEAD);
			stateMap[State.IDLE_DEAD] = (IdleDead)GetChild((int)State.IDLE_DEAD);
			stateMap[State.STUN] = (Stun)GetChild((int)State.STUN);
			stateMap[State.PLAYER_MOVE] = (MovePlayer)GetChild((int)State.PLAYER_MOVE);
			stateMap[State.PLAYER_MOVE_DEAD] = (MovePlayerDead)GetChild((int)State.PLAYER_MOVE_DEAD);
			stateMap[State.NPC_MOVE_ROAM] = (MoveNpcRoam)GetChild((int)State.NPC_MOVE_ROAM);
			stateMap[State.NPC_MOVE_ATTACK] = (MoveNpcAttack)GetChild((int)State.NPC_MOVE_ATTACK);
			stateMap[State.NPC_MOVE_RETURN] = (MoveNpcReturn)GetChild((int)State.NPC_MOVE_RETURN);
		}
		public void Init(Character character)
		{
			foreach (StateBehavior stateBehavior in GetChildren())
			{
				stateBehavior.Init(this, character);
			}

			// call when map has finished loading
			CallDeferred(nameof(SetState),
				UnitDB.HasUnitData(character.Name) && UnitDB.GetUnitData(character.Name).path.Length > 0
				? State.NPC_MOVE_ROAM
				: State.IDLE);
		}
		public void ChangeState(State state)
		{
			stateMap[GetState()].Exit();
			SetState(state);
		}
		private void SetState(State state)
		{
			stateHistory.Push(state);
			stateMap[state].Start();
		}
		public State GetState() { return (stateHistory.Count == 0) ? State.IDLE : stateHistory.Peek(); }
		public State GetLastState()
		{
			if (stateHistory.Count == 1)
			{
				return stateHistory.Peek();
			}
			State currentState = stateHistory.Pop();
			State lastState = stateHistory.Peek();
			stateHistory.Push(currentState);
			return lastState;
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
		public void Harm(int damage) { (stateMap[GetState()] as TakeDamage)?.Harm(damage); }
		public void UnhandledInput(InputEvent @event) { stateMap[GetState()].UnhandledInput(@event); }
	}
}