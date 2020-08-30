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
		public override void Exit() { character.regenTimer.Start(); }
		public async void AttackTarget()
		{
			if (!ValidTarget())
			{
				return;
			}

			character.anim.Stop();
			character.img.Frame = 0;

			await ToSignal(GetTree().CreateTimer(character.stats.weaponSpeed.value, false), "timeout");

			if (!ValidTarget())
			{
				return;
			}

			Node2D missle = character.img.GetNode<Node2D>("missile");
			Vector2 missilePos = new Vector2();
			bool isLeft = character.target.GlobalPosition.x - character.GlobalPosition.x < 0.0f;
			character.img.FlipH = isLeft;
			missilePos.x = Mathf.Abs(missilePos.x) * ((isLeft) ? -1.0f : 1.0f);
			missle.Position = missilePos;

			character.anim.Play("attacking", -1.0f, character.stats.animSpeed.value);
			await ToSignal(character.anim, "animation_finished");

			if (!ValidTarget())
			{
				return;
			}

			uint diceRoll = GD.Randi() % 100 + 1;

			int damage = (int)Math.Round(GD.RandRange(character.stats.minDamage.valueI, character.stats.maxDamage.valueI));
			string soundName;
			CombatText.TextType hitType;
			Stats.AttackTableNode attackTable = Stats.ATTACK_TABLE[Stats.AttackTableType.MELEE];

			ImageDB.ImageNode imageNode = ImageDB.GetImageData(
				UnitDB.GetUnitData(character.Name).img);

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

			if (character is Player || character.target is Player)
			{
				character.target.SpawnCombatText((damage > 0) ? damage.ToString() : hitType.ToString(), hitType);
			}

			// set target to self
			if (character.target == null)
			{
				Player player = character.target as Player;
				if (player != null)
				{
					player.GetMenu().NpcInteract(character as Npc);
				}
				else
				{
					character.target = character;
				}
			}

			if (damage > 0)
			{
				character.target.Harm(damage);
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
	}
}