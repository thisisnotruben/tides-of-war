using System.Collections.Generic;
using System.Linq;
using System;
using Game.Actor.Doodads;
using Game.Database;
using Game.Utils;
using Game.Projectile;
using Godot;
namespace Game.Actor.State
{
	public class Attack : TakeDamage
	{
		private Timer timer = new Timer();
		public bool attackIgnoreArmor;
		private string spellName = "";

		public override void _Ready()
		{
			base._Ready();

			timer.Connect("timeout", this, nameof(OnAttackSpeedTimeout));
			timer.OneShot = true;
			AddChild(timer);

			GD.Randomize();
		}
		public override void Start()
		{
			character.anim.Connect("animation_finished", this, nameof(OnAttackAnimationFinished));
			character.regenTimer.Stop();
			AttackStart();
		}
		public override void Exit()
		{
			timer.Stop();
			character.anim.Disconnect("animation_finished", this, nameof(OnAttackAnimationFinished));
			character.anim.Stop();
			character.regenTimer.Start();
			attackIgnoreArmor = false;
		}
		public void AttackStart()
		{
			if (!ValidTarget())
			{
				return;
			}

			if (!character.IsConnected(nameof(Character.NotifyAttack), character.target, nameof(Character.OnAttacked)))
			{
				character.Connect(nameof(Character.NotifyAttack), character.target, nameof(Character.OnAttacked),
				   new Godot.Collections.Array() { character });
			}

			character.anim.Stop();
			character.img.Frame = 0;

			timer.WaitTime = character.stats.weaponSpeed.value;
			timer.Start();
		}
		private void OnAttackSpeedTimeout()
		{
			if (!ValidTarget())
			{
				return;
			}

			// align missile spawn entry to image
			Vector2 missilePos = character.missileSpawnPos.Position;
			bool isLeft = character.target.GlobalPosition.x - character.GlobalPosition.x < 0.0f;
			character.img.FlipH = isLeft;
			missilePos.x = Mathf.Abs(missilePos.x) * ((isLeft) ? -1.0f : 1.0f);
			character.missileSpawnPos.Position = missilePos;

			// get spell if any and anim (if npc)
			spellName = TryGetSpell(character);

			character.anim.Play(
				SpellDB.HasSpell(spellName)
				? SpellDB.GetSpellData(spellName).characterAnim
				: "attacking",
				-1.0f, character.stats.animSpeed.value);
		}
		public void OnAttackAnimationFinished(string animName)
		{
			if (!ValidTarget())
			{
				return;
			}

			// if only melee
			if (GetImageNode(character).melee)
			{
				AttackHit(character, character.target);
			}
			else
			{
				Missile missile = MissileFactory.CreateMissile(character, spellName);

				// sync missile to attack
				missile.Connect(nameof(Missile.OnHit), this, nameof(AttackHit),
					new Godot.Collections.Array() { character, character.target });

				// add to scene
				Map.Map.map.AddZChild(missile);
				missile.Owner = Map.Map.map;
			}

			AttackStart();
		}
		public void AttackHit(Character character, Character target)
		{
			character.EmitSignal(nameof(Character.NotifyAttack));

			uint diceRoll = GD.Randi() % 100 + 1;

			int damage = (int)Math.Round(GD.RandRange(character.stats.minDamage.valueI, character.stats.maxDamage.valueI));
			string soundName;
			CombatText.TextType hitType;

			ImageDB.ImageNode imageNode = GetImageNode(character);
			Stats.AttackTableNode attackTable = Stats.ATTACK_TABLE[
				imageNode.melee
				? Stats.AttackTableType.MELEE
				: Stats.AttackTableType.RANGED];

			if (diceRoll <= attackTable.hit)
			{
				soundName = imageNode.weaponMaterial;
				hitType = CombatText.TextType.HIT;
			}
			else if (diceRoll <= attackTable.critical)
			{
				soundName = imageNode.weaponMaterial;
				hitType = CombatText.TextType.CRITICAL;
				damage *= 2;
			}
			else if (diceRoll <= attackTable.dodge)
			{
				soundName = imageNode.swing;
				hitType = CombatText.TextType.DODGE;
				damage = 0;
			}
			else if (diceRoll <= attackTable.parry)
			{
				// TODO: make sure unit/target can parry with weapon
				soundName = imageNode.weaponMaterial;
				hitType = CombatText.TextType.PARRY;
				damage = 0;
			}
			else if (diceRoll <= attackTable.miss)
			{
				soundName = imageNode.swing;
				hitType = CombatText.TextType.MISS;
				damage = 0;
			}
			else
			{
				return;
			}

			SoundPlayer.PlaySound(soundName, SoundPlayer.SoundType.RANDOM);

			if (character is Player || target is Player)
			{
				target.SpawnCombatText((damage > 0) ? damage.ToString() : hitType.ToString(), hitType);
			}


			if (!attackIgnoreArmor)
			{
				damage -= character.target.stats.armor.valueI;
			}
			if (damage > 0)
			{
				target.Harm(damage);
			}
		}
		private bool ValidTarget()
		{
			if (fsm.GetState() != FSM.State.ATTACK)
			{
				return false;
			}
			else if (character.target == null || character.target.dead)
			{
				if (character != null && character.IsConnected(nameof(Character.NotifyAttack), character.target, nameof(Character.OnAttacked)))
				{
					character.Disconnect(nameof(Character.NotifyAttack), character.target, nameof(Character.OnAttacked));
				}
				character.target = null;

				if (character is Npc)
				{
					fsm.ChangeState(
						(UnitDB.HasUnitData(character.Name) && UnitDB.GetUnitData(character.Name).path.Length > 0)
						? FSM.State.NPC_MOVE_ROAM
						: FSM.State.NPC_MOVE_RETURN);
				}
				else
				{
					fsm.ChangeState(FSM.State.IDLE);
				}
				return false;
			}
			else if (character.pos.DistanceTo(character.target.pos) > character.stats.weaponRange.value)
			{
				fsm.ChangeState(
					(character is Npc)
					? FSM.State.NPC_MOVE_ATTACK
					: FSM.State.IDLE);
				return false;
			}
			return true;
		}
		private static string TryGetSpell(Character character)
		{
			if (character is Npc && ContentDB.HasContent(character.worldName))
			{
				// select only spells within range
				IEnumerable<string> spells =
				from spellName in ContentDB.GetContentData(character.worldName).spells
				where SpellDB.GetSpellData(spellName).range >= character.pos.DistanceTo(character.target.pos)
				select spellName;

				// TODO: need to have a chance table on when a spell can be casted
				if (spells.Any() && GD.Randi() % 100 + 1 >= 50)
				{
					return spells.ElementAt((int)(GD.Randi() % spells.Count()));
				}
			}
			return "";
		}
		private static ImageDB.ImageNode GetImageNode(Character character) { return ImageDB.GetImageData(character.img.Texture.ResourcePath.GetFile().BaseName()); }
	}
}