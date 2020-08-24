using System;
using Game.Actor.Doodads;
using Game.Database;
using Game.Utils;
using Godot;
namespace Game.Actor.State
{
	public class Attack : TakeDamage
	{
		public override void _Ready()
		{
			base._Ready();
			GD.Randomize();
		}
		public override void Start()
		{
			character.regenTimer.Stop();
			AttackTarget();
		}
		public override void Exit()
		{
			character.regenTimer.Start();
		}
		public async void AttackTarget()
		{
			if (!ValidTarget())
			{
				return;
			}

			animationPlayer.Stop();
			sprite.Frame = 0;
			sprite.FlipH = false;

			await ToSignal(GetTree().CreateTimer(character.weaponSpeed, false), "timeout");

			if (!ValidTarget())
			{
				return;
			}

			Node2D missle = sprite.GetNode<Node2D>("missile");
			Vector2 missilePos = new Vector2();
			bool isLeft = character.target.GlobalPosition.x - character.GlobalPosition.x < 0.0f;
			sprite.FlipH = isLeft;
			missilePos.x = Mathf.Abs(missilePos.x) * ((isLeft) ? -1.0f : 1.0f);
			missle.Position = missilePos;

			animationPlayer.Play("attacking", -1.0f, character.animSpeed);
			await ToSignal(animationPlayer, "animation_finished");

			if (!ValidTarget())
			{
				return;
			}

			GD.Randomize();
			uint diceRoll = GD.Randi() % 100 + 1;

			int damage = (int)Math.Round(GD.RandRange(character.minDamage, character.maxDamage));
			string soundName;
			CombatText.TextType hitType;
			Stats.AttackTableNode attackTable = Stats.ATTACK_TABLE[Stats.AttackTableType.MELEE];

			// TODO: spell
			// if (spell != null && !spell.casted)
			// {
			//     attackTable = spell.GetAttackTable();
			//     if (diceRoll <= attackTable["CRITICAL"])
			//     {
			//         ignoreArmor = spell.ignoreArmor;
			//         damage = (int)Math.Round((float)damage * spell.Cast());
			//         target.SetSpell(spell);
			//     }
			//     else
			//     {
			//         mana = spell.manaCost;
			//     }
			// }

			if (diceRoll <= attackTable.hit)
			{
				soundName = ImageDB.GetImageData(character.Name).weaponMaterial;
				hitType = CombatText.TextType.HIT;
			}
			else if (diceRoll <= attackTable.critical)
			{
				soundName = ImageDB.GetImageData(character.Name).weaponMaterial;
				hitType = CombatText.TextType.CRITICAL;
				damage *= 2;
			}
			else if (diceRoll <= attackTable.dodge)
			{
				soundName = ImageDB.GetImageData(character.Name).swing;
				hitType = CombatText.TextType.DODGE;
				damage = 0;
			}
			else if (diceRoll <= attackTable.parry)
			{
				// TODO: make sure unit/target can parry with weapon
				soundName = ImageDB.GetImageData(character.Name).weaponMaterial;
				hitType = CombatText.TextType.PARRY;
				damage = 0;
			}
			else if (diceRoll <= attackTable.miss)
			{
				soundName = ImageDB.GetImageData(character.Name).swing;
				hitType = CombatText.TextType.MISS;
				damage = 0;
			}
			else
			{
				return;
			}

			SoundPlayer.PlaySound(soundName, SoundPlayer.SoundType.RANDOM);

			if (character is Player || character.target is Player)
			{
				character.target.SpawnCombatText((damage > 0) ? damage.ToString() : hitType.ToString(), hitType);
			}

			if (damage > 0)
			{
				character.target.Harm(damage);

				Player player = character as Player;
				if (player != null)
				{
					player.weapon.TakeDamage();
				}
			}
			AttackTarget();
		}
		private bool ValidTarget()
		{
			if (character.target == null)
			{
				if (character is Npc)
				{
					fsm.ChangeState(
						(UnitDB.HasUnitData(character.Name) && UnitDB.GetUnitData(character.Name).path.Count > 0)
						? FSM.State.NPC_MOVE_ROAM
						: FSM.State.NPC_MOVE_RETURN);
				}
				else
				{
					fsm.ChangeState(FSM.State.IDLE);
				}
				return false;
			}
			else if (character.GetCenterPos().DistanceTo(character.target.GetCenterPos()) > character.weaponRange)
			{
				fsm.ChangeState(
					(character is Npc)
					? FSM.State.NPC_MOVE_ATTACK
					: FSM.State.IDLE);
				return false;
			}
			return true;
		}
	}
}