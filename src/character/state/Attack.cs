using System.Collections.Generic;
using System.Linq;
using System;
using Game.Actor.Doodads;
using Game.Database;
using Game.Projectile;
using Game.Factory;
using Game.Ability;
using Godot;
namespace Game.Actor.State
{
	public class Attack : TakeDamage
	{
		private Timer timer = new Timer();
		private Spell spell;

		[Signal] public delegate void CastSpell(Spell spell);

		public override void _Ready()
		{
			base._Ready();

			timer.Connect("timeout", this, nameof(OnAttackSpeedTimeout));
			timer.OneShot = true;
			AddChild(timer);
		}
		public override void Start()
		{
			if (!ValidTarget())
			{
				return;
			}

			Globals.TryLinkSignal(character.anim, "animation_finished", this, nameof(OnAttackAnimationFinished), true);
			character.regenTimer.Stop();
			AttackStart();

			Map.Map.map.OccupyCell(character.GlobalPosition, true);
		}
		public override void Exit()
		{
			timer.Stop();
			Globals.TryLinkSignal(character.anim, "animation_finished", this, nameof(OnAttackAnimationFinished), false);
			character.anim.Stop();
			character.regenTimer.Start();

			Map.Map.map.OccupyCell(character.GlobalPosition, false);
		}
		public override void OnAttacked(Character whosAttacking) { ClearOnAttackedSignals(whosAttacking); }
		public void AttackStart()
		{
			if (!ValidTarget())
			{
				return;
			}

			// get spell if any
			TryGetSpell(out spell, character);
			if (spell != null)
			{
				switch (Globals.spellDB.GetData(spell.worldName).type)
				{
					case SpellDB.SpellTypes.MOD_FRIENDLY:
					case SpellDB.SpellTypes.MOD_HOSTILE:
						EmitSignal(nameof(CastSpell), spell);
						fsm.ChangeState(FSM.State.CAST);
						return;
				}
			}

			if (!character.IsConnected(nameof(Character.NotifyAttack), character.target, nameof(Character.OnAttacked)))
			{
				character.Connect(nameof(Character.NotifyAttack), character.target, nameof(Character.OnAttacked),
				   new Godot.Collections.Array() { character });
			}

			character.anim.Stop();
			character.img.Frame = 0;

			// TODO: add spell mod here or.... hmmm...
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

			character.anim.Play(
				spell != null
				? Globals.spellDB.GetData(spell.worldName).characterAnim
				: "attacking",
				-1.0f, character.stats.animSpeed.value);
		}
		public void OnAttackAnimationFinished(string animName)
		{
			if (!ValidTarget())
			{
				return;
			}

			if (GetImageData(character).melee)
			{
				if (spell != null && Globals.spellEffectDB.HasData(spell.worldName))
				{
					InstancSpellEffect(spell.worldName, character.target);
				}

				AttackHit(character, character.target, spell);
			}
			else
			{
				Missile missile = new MissileFactory().Make(character, spell?.worldName ?? string.Empty);

				if (spell == null)
				{
					// TODO: make constants
					Globals.soundPlayer.PlaySoundRandomized("bow", character.player2D);
				}
				else
				{
					if (!(missile is MissileSpell) && Globals.spellEffectDB.HasData(spell.worldName))
					{
						InstancSpellEffect(spell.worldName, character.target);
					}
					if (Globals.missileSpellDB.HasData(spell.worldName))
					{
						string spellSound = Globals.missileSpellDB.GetData(spell.worldName).sound;
						if (spellSound.Equals(string.Empty))
						{
							// TODO: make constants
							Globals.soundPlayer.PlaySoundRandomized("bow", character.player2D);
						}
						else
						{
							Globals.soundPlayer.PlaySound(spellSound, character.player2D);
						}
					}
				}

				// sync missile to attack
				missile.Connect(nameof(Missile.OnHit), this, nameof(AttackHit),
					new Godot.Collections.Array() { character, character.target, spell });

				// add to scene
				Map.Map.map.AddZChild(missile);
				missile.LookAt(character.target.pos);
				missile.Owner = Map.Map.map;
			}
			spell = null;

			AttackStart();
		}
		public void AttackHit(Character character, Character target, Spell spell)
		{
			character.EmitSignal(nameof(Character.NotifyAttack));

			Random rand = new Random();

			int diceRoll = rand.Next(1, 101),
				damage = rand.Next(
					character.stats.minDamage.valueI,
					character.stats.maxDamage.valueI + 1);

			ImageDB.ImageData imageData = GetImageData(character),
				targetImageData = GetImageData(target);
			Stats.AttackTableNode attackTable = spell?.attackTable
				?? Stats.ATTACK_TABLE[
					imageData.melee
					? Stats.AttackTableType.MELEE
					: Stats.AttackTableType.RANGED];
			bool ignoreArmor = spell == null ? false
				: Globals.spellDB.GetData(spell.worldName).ignoreArmor;

			string soundName = imageData.weapon;
			CombatText.TextType hitType;

			if (diceRoll <= attackTable.hit)
			{
				hitType = CombatText.TextType.HIT;
			}
			else if (diceRoll <= attackTable.critical)
			{
				hitType = CombatText.TextType.CRITICAL;
				damage *= 2;
			}
			else if (diceRoll <= attackTable.dodge)
			{
				soundName = imageData.swing;
				hitType = CombatText.TextType.DODGE;
				damage = 0;
			}
			else if (diceRoll <= attackTable.parry && imageData.melee == targetImageData.melee)
			{
				// TODO: make constants
				if (imageData.weaponMaterial.Equals(targetImageData.weaponMaterial))
				{
					soundName = imageData.weaponMaterial.Equals("metal")
						? "block_metal_metal"
						: "block_wood_wood";
				}
				else
				{
					soundName = "block_metal_wood";
				}

				hitType = CombatText.TextType.PARRY;
				damage = 0;
			}
			else if (diceRoll <= attackTable.miss)
			{
				soundName = imageData.swing;
				hitType = CombatText.TextType.MISS;
				damage = 0;
			}
			else
			{
				return;
			}

			if (damage == 0)
			{
				spell?.QueueFree();
			}
			else
			{
				spell?.Start();
			}

			if (spell != null)
			{
				string spellSound = Globals.spellDB.GetData(spell.worldName).sound;
				if (!spellSound.Equals(string.Empty))
				{
					soundName = spellSound;
				}
			}

			Globals.soundPlayer.PlaySoundRandomized(soundName, character.player2D);

			if (character is Player || target is Player)
			{
				target.SpawnCombatText(damage > 0 ? damage.ToString() : hitType.ToString(), hitType);
			}

			if (!ignoreArmor)
			{
				damage -= target.stats.armor.valueI;
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
			else if (character.target?.dead ?? true)
			{
				Globals.TryLinkSignal(character, nameof(Character.NotifyAttack), character.target, nameof(Character.OnAttacked), false);
				character.target = null;

				FSM.State state;
				if (character is Npc)
				{
					state = Globals.unitDB.HasData(character.Name) && Globals.unitDB.GetData(character.Name).path.Length > 0
						? FSM.State.NPC_MOVE_ROAM
						: FSM.State.NPC_MOVE_RETURN;
				}
				else
				{
					state = FSM.State.IDLE;
					(character as Player)?.menu.ClearTarget();
				}

				fsm.ChangeState(state);
				return false;
			}
			else if (character.pos.DistanceTo(character.target.pos) > character.stats.weaponRange.value)
			{
				fsm.ChangeState(
					character is Npc
					? FSM.State.NPC_MOVE_ATTACK
					: FSM.State.IDLE);
				return false;
			}
			return true;
		}
		private static void TryGetSpell(out Spell spell, Character character)
		{
			Random rand = new Random();

			// TODO: need to have a chance table on when a spell can be casted
			if (50 >= rand.Next(1, 101)
			&& character is Npc
			&& Globals.contentDB.HasData(character.worldName)
			&& Globals.contentDB.GetData(character.worldName).spells.Length > 0)
			{

				string[] characterSpells = Globals.contentDB.GetData(character.worldName).spells;
				IEnumerable<SpellDB.SpellTypes> characterSpellTypes =
					from spellName in characterSpells
					select Globals.spellDB.GetData(spellName).type;

				Array spellTypes = Enum.GetValues(typeof(SpellDB.SpellTypes));
				SpellDB.SpellTypes spellType;
				IEnumerable<string> spells;

				do
				{
					spellType = (SpellDB.SpellTypes)spellTypes.GetValue(rand.Next(spellTypes.Length));
				} while (!characterSpellTypes.Contains(spellType));

				spells = from spellName in characterSpells
						 where Globals.spellDB.GetData(spellName).type.Equals(spellType)
						 select spellName;

				if (spellType.Equals(SpellDB.SpellTypes.HIT_HOSTILE))
				{
					// select only spells within range
					spells =
						from spellName in spells
						where Globals.spellDB.GetData(spellName).range >= character.pos.DistanceTo(character.target.pos)
						select spellName;
				}

				if (spells.Any())
				{
					spell = new SpellFactory().Make(character, spells.ElementAt(rand.Next(spells.Count())));

					switch (spellType)
					{
						case SpellDB.SpellTypes.HIT_HOSTILE:
							character.target.CallDeferred("add_child", spell);
							break;
						default:
							// MOD_HOSTILE || MOD_FRIENDLY
							character.CallDeferred("add_child", spell);
							break;
					}
					return;
				}
			}
			spell = null;
		}
		private static ImageDB.ImageData GetImageData(Character character)
		{
			return Globals.imageDB.GetData(character.img.Texture.ResourcePath.GetFile().BaseName());
		}
		private static void InstancSpellEffect(string spellName, Character target)
		{
			SpellEffect spellEffect = (SpellEffect)Globals.spellEffectDB.GetData(
				Globals.spellDB.GetData(spellName).spellEffect).Instance();

			spellEffect.Init(target, spellName, target);
			spellEffect.OnHit();
		}
	}
}