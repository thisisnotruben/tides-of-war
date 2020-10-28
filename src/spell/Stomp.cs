using Game.Actor;
using Game.Actor.State;
using System;
namespace Game.Ability
{
	public class Stomp : SpellAreaEffect
	{
		private readonly Random rand = new Random();

		public Stomp(Character character, string worldName) : base(character, worldName) { }
		protected override void StartAreaEffect(Character character)
		{
			base.StartAreaEffect(character);

			if (20 >= rand.Next(1, 101))
			{
				character.state = FSM.State.STUN;
				character.fsm.lockState = true;
			}
		}
		protected override void ExitAreaEffect(Character character)
		{
			base.ExitAreaEffect(character);

			if (character.state.Equals(FSM.State.STUN))
			{
				character.fsm.lockState = false;
				character.fsm.RevertState();
			}
		}
	}
}