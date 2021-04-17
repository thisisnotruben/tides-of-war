using Game.Ability;
using Game.Database;
using GC = Godot.Collections;
namespace Game.Actor.State
{
	public class Cast : TakeDamage
	{
		private Spell spell;

		public override void Start()
		{
			if (spell == null)
			{
				fsm.RevertState();
				return;
			}

			Map.Map.map.OccupyCell(character.GlobalPosition, true);
			Globals.TryLinkSignal(character.anim, "animation_finished", this, nameof(OnCastFinished), true);
			character.anim.Play(Character.ANIM_CAST, -1.0f, character.stats.animSpeed.value);

			if (Globals.spellEffectDB.HasData(spell.worldName))
			{
				((SpellEffect)Globals.spellEffectDB.GetData(Globals.spellDB.GetData(
					spell.worldName).spellEffect).Instance()).Init(character, spell.worldName).OnHit();
			}
		}
		public override void Exit()
		{
			Globals.TryLinkSignal(character.anim, "animation_finished", this, nameof(OnCastFinished), false);
			Map.Map.map.OccupyCell(character.GlobalPosition, false);
		}
		public void SetSpell(Spell spell)
		{
			this.spell = spell;
			character.CallDeferred("add_child", spell);
		}
		private void OnCastFinished(string animName)
		{
			if (spell != null)
			{
				spell.Start();

				Player player = character as Player;
				if (player != null)
				{
					player.menu.CheckHudSlots(player.menu.playerMenu.playerSpellBook, spell.worldName);
				}
			}
			fsm.RevertState();
		}
		public override GC.Dictionary Serialize()
		{
			GC.Dictionary payload = base.Serialize();
			if (spell != null)
			{
				payload[NameDB.SaveTag.SPELL] = spell.worldName;
			}
			return payload;
		}
		public override void Deserialize(GC.Dictionary payload)
		{
			base.Deserialize(payload);

			float animPos = (float)payload[NameDB.SaveTag.ANIM_POSITION];
			if (animPos > 0.0f
			&& animPos < character.anim.GetAnimation(Character.ANIM_CAST).Length)
			{
				character.anim.Seek(animPos, true);
			}
		}
	}
}