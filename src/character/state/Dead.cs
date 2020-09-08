using System.Collections.Generic;
using System.Linq;
using Game.Actor.Doodads;
using Game.Ability;
using Game.Database;
using Godot;
namespace Game.Actor.State
{
	public class Dead : StateBehavior
	{
		public override void Start()
		{
			character.regenTimer.Stop();
			GD.Randomize();
			Die();
		}
		public override void Exit() { }
		public override void UnhandledInput(InputEvent @event) { }
		private async void Die()
		{
			// changing character detection
			character.hitBox.CollisionLayer = (uint)Character.CollMask.DEAD;
			if (character is Player)
			{
				character.hitBox.CollisionLayer += (uint)Character.CollMask.PLAYER;
			}

			// stop health/mana regenerations
			character.regenTimer.Stop();

			// clear targets
			(character as Player)?.menu.ClearTarget();
			if (character is Npc
			&& character.target is Player
			&& character.target == character)
			{
				((Player)character.target).menu.ClearTarget();
				character.target.target = null;
			}

			// play death animation
			character.anim.Play("dying");
			await ToSignal(character.anim, "animation_finished");
			character.Modulate = new Color("bfffffff");

			foreach (Spell spell in character.spellQueue)
			{
				spell.UnMake();
			}
			foreach (Node node in character.GetChildren())
			{
				if (node is CombatText)
				{
					node.QueueFree();
				}
			}

			Player player = character as Player;
			if (player != null)
			{
				// instance tomb
				Tomb tomb = (Tomb)Tomb.scene.Instance();
				tomb.Init(player);
				Map.Map.map.AddZChild(tomb);
				tomb.GlobalPosition = Map.Map.map.GetGridPosition(character.GlobalPosition);

				// set map death effects
				Map.Map.map.SetVeil(true);

				// spawn to nearest graveyard
				Dictionary<int, Vector2> graveSites = new Dictionary<int, Vector2>();
				foreach (Node2D graveyard in GetTree().GetNodesInGroup("gravesite"))
				{
					graveSites[Map.Map.map.getAPath(character.GlobalPosition, graveyard.GlobalPosition).Count] = graveyard.GlobalPosition;
				}
				character.GlobalPosition = Map.Map.map.GetGridPosition(graveSites[graveSites.Keys.Min()]);

				fsm.ChangeState(FSM.State.IDLE_DEAD);
			}
			else
			{
				// npc
				character.Hide();
				character.sight.Monitoring = false;
				if (!IsInGroup(Globals.SAVE_GROUP))
				{
					AddToGroup(Globals.SAVE_GROUP);
				}
				character.GlobalPosition = UnitDB.GetUnitData(character.Name).spawnPos;

				// create spawn timer
				await ToSignal(GetTree().CreateTimer((float)GD.RandRange(60.0, 240.0), false), "timeout");

				character.sight.Monitoring = true;
				fsm.ChangeState(FSM.State.ALIVE);
			}

		}
	}
}