using GC = Godot.Collections;
using Game.Actor;
using Game.Actor.State;
using Game.Database;
using System;
using Godot.Collections;

namespace Game.Ability
{
	public class Stomp : SpellAreaEffect
	{
		private readonly Random rand = new Random();
		private bool stunned;

		protected override void StartAreaEffect(Character character)
		{
			base.StartAreaEffect(character);

			if (20 >= rand.Next(1, 101))
			{
				StunCharacter();
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
		private void StunCharacter()
		{
			character.state = FSM.State.STUN;
			character.fsm.lockState = stunned = true;
		}
		public override GC.Dictionary Serialize()
		{
			GC.Dictionary payload = base.Serialize();
			payload[NameDB.SaveTag.STATE] = stunned;
			return payload;
		}
		public override void Deserialize(Dictionary payload)
		{
			base.Deserialize(payload);
			if ((bool)payload[NameDB.SaveTag.STATE])
			{
				StunCharacter();
			}
		}
	}
}