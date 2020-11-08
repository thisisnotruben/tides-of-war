using Game.Database;
using Game.Ability;
using Game.Actor;
using Godot;
namespace Game.Projectile
{
	public class MissileSpell : Missile
	{
		private Timer timer;
		protected string spellWorldName = string.Empty;

		public override void _Ready()
		{
			base._Ready();
			timer = GetNode<Timer>("timer");

			// can't add in Init due to this section needing nodes
			if (!spellWorldName.Equals(string.Empty) && SpellDB.HasSpellMissile(spellWorldName))
			{
				SpellDB.SpellMissileData spellMissileData = SpellDB.GetSpellMissileData(spellWorldName);
				img.Texture = spellMissileData.img;
				hitboxBody.Shape = spellMissileData.hitBox;
			}
		}
		public virtual void Init(Character character, Character target, string spellWorldName)
		{
			this.spellWorldName = spellWorldName ?? string.Empty;

			Init(character, target);

			if (SpellDB.HasSpellMissile(spellWorldName))
			{
				SpellDB.SpellMissileData spellMissileData = SpellDB.GetSpellMissileData(spellWorldName);

				moveBehavior = (float delta) =>
				{
					if (!hit && spellMissileData.rotate)
					{
						LookAt(target.pos);
					}

					MoveMissile(GlobalPosition, target.pos);
				};
			}

			if (SpellDB.HasSpellEffect(spellWorldName))
			{
				SpellEffect spellEffect = SpellDB.GetSpellEffect(
					SpellDB.GetSpellData(spellWorldName).spellEffect);

				spellEffect.Init(target, spellWorldName, this);
				Connect(nameof(OnHit), spellEffect, nameof(SpellEffect.OnHit));
			}
		}
		public override void OnMissileFadeFinished(string animName) { timer.Start(); }
	}
}