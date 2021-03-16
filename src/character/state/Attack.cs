using System.Collections.Generic;
using System.Linq;
using System;
using Game.Actor.Doodads;
using Game.Database;
using Game.Projectile;
using Game.Factory;
using Game.Ability;
using Godot;
using GC = Godot.Collections;
namespace Game.Actor.State
{
	public class Attack : TakeDamage
	{
		private Timer timer;
		private Spell spell;

		[Signal] public delegate void CastSpell(Spell spell);

		public override void _Ready()
		{
			base._Ready();
			timer = GetChild<Timer>(0);
			timer.Connect("timeout", this, nameof(OnAttackSpeedTimeout));
			timer.OneShot = true;
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
			spell = null; // TODO: if player: play a 'spell cancel sound' if it wasn't null

			Map.Map.map.OccupyCell(character.GlobalPosition, false);
		}
		public override void OnAttacked(Character whosAttacking)
		{
			ClearOnAttackedSignals(whosAttacking);
			character.regenTimer.Stop();
		}
		public void AttackStart()
		{
			if (!ValidTarget())
			{
				return;
			}

			// get spell if any
			if (character is Player)
			{
				if (spell != null)
				{
					SetSpellNodeInTree(spell, character);
				}
			}
			else
			{
				TryGetNpcSpell(out spell, character);
			}

			if (spell != null && !Globals.spellDB.GetData(spell.worldName).requiresTarget)
			{
				EmitSignal(nameof(CastSpell), spell);
				fsm.ChangeState(FSM.State.CAST);
				return;
			}

			if (!character.IsConnected(nameof(Character.NotifyAttack), character.target, nameof(Character.OnAttacked)))
			{
				character.Connect(nameof(Character.NotifyAttack), character.target, nameof(Character.OnAttacked),
				   new Godot.Collections.Array() { character });
			}

			character.anim.Stop();
			character.img.Frame = 0;

			timer.Start(character.stats.weaponSpeed.value);
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
				if (spell != null)
				{
					if (Globals.spellEffectDB.HasData(spell.worldName))
					{
						InstancSpellEffect(spell.worldName, character.target);
					}

					spell.AddCooldown(character.GetPath(), spell.worldName,
						Globals.spellDB.GetData(spell.worldName).coolDown);
				}

				AttackHit(character, character.target, spell);
			}
			else
			{
				Missile missile = new MissileFactory().Make(character, spell?.worldName ?? string.Empty);

				if (spell == null)
				{
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
							Globals.soundPlayer.PlaySoundRandomized("bow", character.player2D);
						}
						else
						{
							Globals.soundPlayer.PlaySound(spellSound, character.player2D);
						}
					}

					spell.AddCooldown(character.GetPath(), spell.worldName,
						Globals.spellDB.GetData(spell.worldName).coolDown);
				}

				fsm.ConnectMissileHit(missile, character, character.target, spell);
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
				if (imageData.weaponMaterial.Equals(targetImageData.weaponMaterial))
				{
					soundName = imageData.weaponMaterial.Equals("metal")
						? NameDB.UI.BLOCK_METAL_METAL
						: NameDB.UI.BLOCK_WOOD_WOOD;
				}
				else
				{
					soundName = NameDB.UI.BLOCK_METAL_WOOD;
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
				target.Harm(damage, character.GlobalPosition);
			}
		}
		private bool ValidTarget()
		{
			if (fsm.GetState() != FSM.State.ATTACK)
			{
				character.target?.RemovePursuantUnitId(character);
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

				character.target?.RemovePursuantUnitId(character);
				fsm.ChangeState(state);
				return false;
			}
			else if (character.pos.DistanceTo(character.target.pos) > character.stats.weaponRange.value)
			{
				character.target?.RemovePursuantUnitId(character);
				fsm.ChangeState(
					character is Npc
					? FSM.State.NPC_MOVE_ATTACK
					: FSM.State.IDLE);
				return false;
			}
			return true;
		}
		public void SetSpell(Spell spell)
		{
			this.spell = spell;
			if (character is Player)
			{
				AttackStart();
			}
		}
		private static bool TryGetNpcSpell(out Spell spell, Character character)
		{
			Random rand = new Random();

			if (character is Npc
			&& Stats.CHANCE_NPC_SPELL >= rand.Next(1, 101)
			&& Globals.contentDB.HasData(character.worldName))
			{
				ContentDB.ContentData contentData = Globals.contentDB.GetData(character.worldName);
				if (contentData.spells.Length == 0)
				{
					spell = null;
					return false;
				}

				IEnumerable<SpellDB.SpellTypes> characterSpellTypes =
					from spellName in contentData.spells
					select Globals.spellDB.GetData(spellName).type;

				Array spellTypes = Enum.GetValues(typeof(SpellDB.SpellTypes));
				SpellDB.SpellTypes spellType;
				IEnumerable<string> spells;

				do
				{
					spellType = (SpellDB.SpellTypes)spellTypes.GetValue(rand.Next(spellTypes.Length));
				} while (!characterSpellTypes.Contains(spellType));

				spells = from spellName in contentData.spells
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
					SetSpellNodeInTree(spell, character);
					return true;
				}

			}
			spell = null;
			return false;
		}
		private static void SetSpellNodeInTree(Spell spell, Character character)
		{
			switch (Globals.spellDB.GetData(spell.worldName).type)
			{
				case SpellDB.SpellTypes.HIT_HOSTILE:
					character.target.CallDeferred("add_child", spell);
					break;
				default:
					// MOD_HOSTILE || MOD_FRIENDLY
					character.CallDeferred("add_child", spell);
					break;
			}
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
		public override GC.Dictionary Serialize()
		{
			GC.Dictionary payload = base.Serialize();
			payload[NameDB.SaveTag.TIME_LEFT] = timer.TimeLeft;
			if (spell != null)
			{
				payload[NameDB.SaveTag.SPELL] = spell.worldName;
			}
			return payload;
		}
		public override void Deserialize(GC.Dictionary payload)
		{
			base.Deserialize(payload);

			float timeLeft = (float)payload[NameDB.SaveTag.TIME_LEFT],
				animPos = (float)payload[NameDB.SaveTag.ANIM_POSITION];
			if (timeLeft > 0.0f)
			{
				timer.Start(timeLeft);
			}
			else if (animPos > 0.0f)
			{
				timer.Stop();
				OnAttackSpeedTimeout();
				character.anim.Seek(animPos, true);
			}
		}
	}
}