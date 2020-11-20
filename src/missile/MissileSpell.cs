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
			if (!spellWorldName.Equals(string.Empty) && MissileSpellDB.Instance.HasData(spellWorldName))
			{
				MissileSpellDB.SpellMissileData spellMissileData = MissileSpellDB.Instance.GetData(spellWorldName);
				img.Texture = spellMissileData.img;
				hitboxBody.Shape = spellMissileData.hitBox;
			}
		}
		public virtual void Init(Character character, Character target, string spellWorldName)
		{
			this.spellWorldName = spellWorldName ?? string.Empty;

			Init(character, target);

			if (MissileSpellDB.Instance.HasData(spellWorldName))
			{
				MissileSpellDB.SpellMissileData spellMissileData = MissileSpellDB.Instance.GetData(spellWorldName);

				moveBehavior = (float delta) =>
				{
					if (!hit && spellMissileData.rotate)
					{
						LookAt(target.pos);
					}

					MoveMissile(GlobalPosition, target.pos);
				};
			}

			if (SpellEffectDB.Instance.HasData(spellWorldName))
			{
				SpellEffect spellEffect = (SpellEffect)SpellEffectDB.Instance.GetData(
					SpellDB.Instance.GetData(spellWorldName).spellEffect).Instance();

				spellEffect.Init(target, spellWorldName, this);
				Connect(nameof(OnHit), spellEffect, nameof(SpellEffect.OnHit));
			}
		}
		public override void OnMissileFadeFinished(string animName) { timer.Start(); }
	}
}