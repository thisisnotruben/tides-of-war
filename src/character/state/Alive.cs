using System;
using Godot;
namespace Game.Actor.State
{
	public class Alive : StateBehavior
	{
		private Tween tween = new Tween();

		public override void _Ready()
		{
			base._Ready();
			AddChild(tween);
			GD.Randomize();
		}
		public override void Start()
		{
			// animate transparency
			tween.InterpolateProperty(character, ":modulate", character.Modulate,
				new Color("ffffff"), 0.5f, Tween.TransitionType.Quart, Tween.EaseType.Out);
			tween.Start();

			// reset clock for health/mana regen
			character.regenTimer.Start();

			if (character is Player)
			{
				// changing character detection for player
				character.hitBox.CollisionLayer = (uint)Character.CollMask.PLAYER + (uint)Character.CollMask.NPC;

				// reset health
				character.hp = (int)Math.Round(character.stats.hpMax.value * GD.RandRange(Stats.HP_MANA_RESPAWN_MIN_LIMIT, 1.0));
				character.mana = (int)Math.Round(character.stats.manaMax.value * GD.RandRange(Stats.HP_MANA_RESPAWN_MIN_LIMIT, 1.0));
			}
			else
			{
				// changing character detection for npc
				character.hitBox.CollisionLayer = (uint)Character.CollMask.NPC;

				// reset health
				character.hp = character.stats.hpMax.valueI;
				character.mana = character.stats.manaMax.valueI;
				if (IsInGroup(Globals.SAVE_GROUP))
				{
					RemoveFromGroup(Globals.SAVE_GROUP);
				}
			}

			// notify those you see around you
			foreach (Area2D otherHitBox in character.sight.GetOverlappingAreas())
			{
				Npc otherNpc = otherHitBox.Owner as Npc;
				if (otherNpc != null)
				{
					otherNpc._OnCharacterEnteredSight(character.hitBox);
				}
			}

			fsm.ChangeState(FSM.State.IDLE);
		}
		public override void Exit() { }
	}
}