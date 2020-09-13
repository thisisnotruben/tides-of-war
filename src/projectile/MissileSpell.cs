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

		protected SpellProto spell;

		public override void _Ready()
		{
			base._Ready();
			Connect(nameof(OnHit), this, nameof(StartSpell));
		}
		public void Init(Character character, Character target, string spellWorldName)
		{
			Init(character, target);
			// TODO: spell = (SpellProto)new SpellCreator().MakeCommodity(character, spellWorldName);

			if (SpellDB.HasSpellMissile(spellWorldName))
			{
				SpellDB.SpellMissileNode spellMissileNode = SpellDB.GetSpellMissileData(spellWorldName);
				img.Texture = spellMissileNode.img;
				hitboxBody.Shape = spellMissileNode.hitBox;

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
		public async override void OnHitBoxEntered(Area2D area2D)
		{
			if (DidHitTarget(area2D))
			{
				hit = true;
				ZIndex = 1;
				CallDeferred("set", hitbox.Monitoring, false);

				EmitSignal(nameof(OnHit));

				anim.Play("missileFade");
				await ToSignal(anim, "animation_finished");

				// allow spell it animation if any to process then delete
				await ToSignal(GetTree().CreateTimer(2.5f), "timeout");

				tween.RemoveAll();
				SetProcess(false);
				QueueFree();
			}
		}
	}
}