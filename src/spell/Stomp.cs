using System.Collections.Generic;
using Game.Actor;
using Game.Actor.State;
using Godot;
namespace Game.Ability
{
	public class Stomp : SpellAreaEffect
	{
		private readonly Dictionary<Character, FSM.State> prevState = new Dictionary<Character, FSM.State>();

		public Stomp(Character character, string worldName) : base(character, worldName)
		{
			GD.Randomize();
		}
		protected override void StartAreaEffect(Character character)
		{
			base.StartAreaEffect(character);

			if (20 >= GD.Randi() % 100 + 1)
			{
				prevState[character] = character.state;
				character.state = FSM.State.STUN;
			}
		}
		protected override void ExitAreaEffect(Character character)
		{
			base.ExitAreaEffect(character);

			if (prevState.ContainsKey(character) && character.state.Equals(FSM.State.STUN))
			{
				character.state = prevState[character];
			}
		}
	}
}