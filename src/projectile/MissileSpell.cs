using Game.Database;
using Game.Ability;
using Game.ItemPoto;
using Game.Actor;
using Godot;
namespace Game.Projectile
{
	public class MissileSpell : Missile
	{
		public new static PackedScene scene = (PackedScene)GD.Load("res://src/projectile/MissileSpell.tscn");

		private Timer timer;
		protected SpellProto spell;

		public override void _Ready()
		{
			base._Ready();
			Connect(nameof(OnHit), this, nameof(StartSpell));
			timer = GetNode<Timer>("timer");

			// can't add in Init due to this section needing nodes
			if (spell != null && SpellDB.HasSpellMissile(spell.worldName))
			{
				SpellDB.SpellMissileNode spellMissileNode = SpellDB.GetSpellMissileData(spell.worldName);
				img.Texture = spellMissileNode.img;
				hitboxBody.Shape = spellMissileNode.hitBox;
			}
		}
		public void Init(Character character, Character target, string spellWorldName)
		{
			Init(character, target);
			// TODO: spell = (SpellProto)new SpellCreator().MakeCommodity(character, spellWorldName);

			if (SpellDB.HasSpellMissile(spellWorldName))
			{
				SpellDB.SpellMissileNode spellMissileNode = SpellDB.GetSpellMissileData(spellWorldName);

				if (spellMissileNode.instantSpawn)
				{
					spawnPos = GlobalPosition = target.pos;
				}

				moveBehavior = () =>
				{
					if (spellMissileNode.rotate)
					{
						LookAt(target.pos);
					}

					if (spellMissileNode.reverse)
					{
						MoveMissile(target.pos, GlobalPosition);
					}
					else
					{
						MoveMissile(GlobalPosition, target.pos);
					}
				};
			}

			// instance spell effect
			if (SpellDB.HasSpellEffect(spellWorldName))
			{
				SpellEffect spellEffect = SpellDB.GetSpellEffect(
					SpellDB.GetSpellData(spellWorldName).spellEffect);

				AddChild(spellEffect);
				spellEffect.Owner = this;

				spellEffect.Init(character, spellWorldName);
				Connect(nameof(OnHit), spellEffect, nameof(SpellEffect.OnHit));
			}
		}
		public void StartSpell() { spell?.Start(); }
		public override void OnMissileFadeFinished(string animName) { timer.Start(); }
	}
}