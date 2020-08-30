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
			IDLE, MOVE, ATTACK, ALIVE, DEAD,
			PLAYER_DEAD_IDLE, PLAYER_DEAD_MOVE,
			NPC_MOVE_ROAM, NPC_MOVE_ATTACK, NPC_MOVE_RETURN
		}
		private Dictionary<State, StateBehavior> stateMap = new Dictionary<State, StateBehavior>();
		private Stack<State> stateHistory = new Stack<State>();

		public override void _Ready()
		{
			stateMap[State.IDLE] = (StateBehavior)GetChild((int)State.IDLE);
			stateMap[State.MOVE] = (StateBehavior)GetChild((int)State.MOVE);
			stateMap[State.ATTACK] = (StateBehavior)GetChild((int)State.ATTACK);
			stateMap[State.ALIVE] = (StateBehavior)GetChild((int)State.ALIVE);
			stateMap[State.DEAD] = (StateBehavior)GetChild((int)State.DEAD);
			stateMap[State.PLAYER_DEAD_IDLE] = (StateBehavior)GetChild((int)State.PLAYER_DEAD_IDLE);
			stateMap[State.PLAYER_DEAD_MOVE] = (StateBehavior)GetChild((int)State.PLAYER_DEAD_MOVE);
			stateMap[State.NPC_MOVE_ROAM] = (StateBehavior)GetChild((int)State.NPC_MOVE_ROAM);
			stateMap[State.NPC_MOVE_ATTACK] = (StateBehavior)GetChild((int)State.NPC_MOVE_ATTACK);
			stateMap[State.NPC_MOVE_RETURN] = (StateBehavior)GetChild((int)State.NPC_MOVE_RETURN);

			Character character = Owner as Character;
			foreach (StateBehavior stateBehavior in GetChildren())
			{
				stateBehavior.Init(this, character);
			}

			// call when map has finished loading
			CallDeferred(nameof(SetState),
				(UnitDB.HasUnitData(character.Name) && UnitDB.GetUnitData(character.Name).path.Count > 0)
				? State.NPC_MOVE_ROAM
				: State.IDLE);
		}
		public void ChangeState(State state)
		{
			stateMap[GetState()].Exit();
			SetState(state);
		}
		public void SetState(State state)
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
				case State.PLAYER_DEAD_IDLE:
				case State.PLAYER_DEAD_MOVE:
					return true;
				default:
					return false;
			}
		}
		// * Delegated functions for state
		public void Harm(int damage)
		{
			TakeDamage takeDamageState = stateMap[GetState()] as TakeDamage;
			if (takeDamageState != null)
			{
				takeDamageState.Harm(damage);
			}
		}
		public void UnhandledInput(InputEvent @event) { stateMap[GetState()].UnhandledInput(@event); }
	}
}