using Godot;
using GC = Godot.Collections;
using Game.Database;
using Game.Actor.State;
using Game.Actor;
namespace Game.Ability
{
	public class Volley : Spell
	{
		private int hit = 3;

		public override void _Ready()
		{
			base._Ready();
			character.fsm.Connect(nameof(FSM.StateChanged), this, nameof(CheckState));
			character.anim.Connect("animation_finished", this, nameof(OnCharacterAnimFinished));
		}
		public void CheckState(FSM.State state)
		{
			if (!state.Equals(FSM.State.ATTACK))
			{
				Exit();
			}
		}
		public void OnCharacterAnimFinished(string animName)
		{
			if (!animName.Equals(Character.ANIM_ATTACK) || --hit == 0)
			{
				Exit();
			}
		}
		public override GC.Dictionary Serialize()
		{
			GC.Dictionary payload = base.Serialize();
			payload[NameDB.SaveTag.HIT] = hit;
			return payload;
		}
		public override void Deserialize(GC.Dictionary payload)
		{
			hit = payload[NameDB.SaveTag.HIT].ToString().ToInt();
			base.Deserialize(payload);
		}
	}
}