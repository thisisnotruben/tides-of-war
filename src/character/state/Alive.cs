using System;
using Game.Database;
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

			// show combat text
			character.combatTextHandler.Show();

			if (character is Player)
			{
				// changing character detection for player
				character.hitBox.CollisionLayer = Character.COLL_MASK_PLAYER + Character.COLL_MASK_NPC;

				// reset health
				character.hp = (int)Math.Round(character.stats.hpMax.value * GD.RandRange(Stats.HP_MANA_RESPAWN_MIN_LIMIT, 1.0));
				character.mana = (int)Math.Round(character.stats.manaMax.value * GD.RandRange(Stats.HP_MANA_RESPAWN_MIN_LIMIT, 1.0));
			}
			else
			{
				// changing character detection for npc
				character.hitBox.CollisionLayer = Character.COLL_MASK_NPC;

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
				(otherHitBox.Owner as Npc)?._OnCharacterEnteredSight(character.hitBox);
			}

			fsm.ChangeState(UnitDB.HasUnitData(character.Name)
				&& UnitDB.GetUnitData(character.Name).path.Length > 0
				? FSM.State.NPC_MOVE_ROAM
				: FSM.State.IDLE);
		}
		public override void Exit() { }
	}
}