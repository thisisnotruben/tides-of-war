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
			if (!spellWorldName.Equals(string.Empty) && Globals.missileSpellDB.HasData(spellWorldName))
			{
				MissileSpellDB.SpellMissileData spellMissileData = Globals.missileSpellDB.GetData(spellWorldName);
				img.Texture = spellMissileData.img;
				hitboxBody.Shape = spellMissileData.hitBox;
			}
		}
		public virtual void Init(Character character, Character target, string spellWorldName)
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
				SpellEffect spellEffect = (SpellEffect)Globals.spellEffectDB.GetData(
					Globals.spellDB.GetData(spellWorldName).spellEffect).Instance();

				spellEffect.Init(target, spellWorldName, this);
				Connect(nameof(OnHit), spellEffect, nameof(SpellEffect.OnHit));
			}
		}
		public override void OnMissileFadeFinished(string animName) { timer.Start(); }
		public override Godot.Collections.Dictionary Serialize()
		{
			Godot.Collections.Dictionary payload = base.Serialize();
			payload[NameDB.SaveTag.SPELL] = spellWorldName;
			return payload;
		}
		public override void Deserialize(Godot.Collections.Dictionary payload)
		{
			// the same code as parent class, except for the Init call

			string characterPath = payload[NameDB.SaveTag.CHARACTER].ToString(),
				targetPath = payload[NameDB.SaveTag.TARGET].ToString();

			if ((bool)payload[NameDB.SaveTag.HIT] || !HasNode(characterPath) || !HasNode(targetPath))
			{
				Delete();
			}

			Init(GetNode<Character>(characterPath), GetNode<Character>(targetPath), payload[NameDB.SaveTag.SPELL].ToString());

			Godot.Collections.Array pos = (Godot.Collections.Array)payload[NameDB.SaveTag.SPAWN_POSITION];
			spawnPos = new Vector2((float)pos[0], (float)pos[1]);
		}
	}
}