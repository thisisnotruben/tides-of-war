using Game.Database;
using Game.Ability;
using Game.Actor;
using Game.Factory;
using Godot;
using GC = Godot.Collections;
namespace Game.Projectile
{
	public class MissileSpell : Missile
	{
		private Timer timer;
		protected string spellWorldName = string.Empty;
		protected SpellEffect spellEffect;

		public override void _Ready()
		{
			base._Ready();
			timer = GetNode<Timer>("timer");

			// can't add in Init due to this section needing nodes
			if (!spellWorldName.Equals(string.Empty) && Globals.missileSpellDB.HasData(spellWorldName))
			{
				MissileSpellDB.SpellMissileData spellMissileData = Globals.missileSpellDB.GetData(spellWorldName);
				img.Texture = spellMissileData.img;
				hitboxBody.Shape = spellMissileData.hitBox;
			}
		}
		public virtual Missile Init(Character character, Character target, string spellWorldName)
		{
			this.spellWorldName = spellWorldName ?? string.Empty;

			Init(character, target);

			if (Globals.missileSpellDB.HasData(spellWorldName))
			{
				MissileSpellDB.SpellMissileData spellMissileData = Globals.missileSpellDB.GetData(spellWorldName);

				moveBehavior = (float delta) =>
				{
					if (!hit && spellMissileData.rotate)
					{
						LookAt(target.pos);
					}

					MoveMissile(GlobalPosition, target.pos);
				};
			}

			if (Globals.spellEffectDB.HasData(spellWorldName))
			{
				spellEffect = Globals.spellEffectDB.GetData(
					Globals.spellDB.GetData(spellWorldName).spellEffect).Instance<SpellEffect>()
					.Init(target, spellWorldName, this);

				Connect(nameof(OnHit), spellEffect, nameof(SpellEffect.OnHit));
			}

			return this;
		}
		public override void OnMissileFadeFinished(string animName) { timer.Start(); }
		public override GC.Dictionary Serialize()
		{
			GC.Dictionary payload = base.Serialize();
			payload[NameDB.SaveTag.TIME_LEFT] = timer.TimeLeft;
			payload[NameDB.SaveTag.SPELL] = spellWorldName;
			return payload;
		}
		public override void Deserialize(GC.Dictionary payload)
		{
			hit = (bool)payload[NameDB.SaveTag.HIT];

			GC.Array pos = (GC.Array)payload[NameDB.SaveTag.SPAWN_POSITION];
			spawnPos = new Vector2((float)pos[0], (float)pos[1]);

			pos = (GC.Array)payload[NameDB.SaveTag.POSITION];
			GlobalPosition = new Vector2((float)pos[0], (float)pos[1]);

			if (hit)
			{
				float timeLeft = (float)payload[NameDB.SaveTag.TIME_LEFT],
					animPos = (float)payload[NameDB.SaveTag.ANIM_POSITION];

				if (animPos > 0.0f && animPos < anim.GetAnimation(ANIM_NAME).Length)
				{
					spellEffect.OnHit();
					anim.Play(ANIM_NAME);
					anim.Seek(animPos, true);
				}
				else if (timeLeft > 0.0f)
				{
					spellEffect.OnHit();
					timer.Start(timeLeft);
				}
				else
				{
					Delete();
				}
			}
			else
			{
				character.fsm.ConnectMissileHit(this, character, target,
					new SpellFactory().Make(character, spellWorldName));
			}
		}
	}
}