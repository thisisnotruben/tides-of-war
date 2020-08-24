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
		private static readonly PackedScene tombScene = (PackedScene)GD.Load("res://src/character/doodads/tomb.tscn");

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
			Area2D area2D = character.GetNode<Area2D>("area");
			area2D.SetCollisionLayerBit(Globals.Collision["CHARACTERS"], false);
			area2D.SetCollisionLayerBit(Globals.Collision["DEAD_CHARACTERS"], false);

			// stop health/mana regenerations
			character.GetNode<Timer>("timer").Stop();

			if (character is Npc && character.target is Player)
			{
				character.target.target = null;
			}

			// play death animation
			animationPlayer.Play("dying");
			await ToSignal(animationPlayer, "animation_finished");
			character.Modulate = new Color("#80ffffff");

			character.hp = 0;
			character.mana = 0;
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
				Tomb tomb = (Tomb)tombScene.Instance();
				Map.Map.map.AddZChild(tomb);
				tomb.SetDeceasedPlayer(player);
				tomb.GlobalPosition = Map.Map.map.GetGridPosition(character.GlobalPosition);
				// set map death effects
				Map.Map.map.SetVeil(true);
				// spawn to nearest graveyard
				Dictionary<float, Vector2> graveSites = new Dictionary<float, Vector2>();
				foreach (Node2D graveyard in GetTree().GetNodesInGroup("gravesite"))
				{
					graveSites.Add(character.GlobalPosition.DistanceTo(graveyard.GlobalPosition), graveyard.GlobalPosition);
				}
				character.GlobalPosition = Map.Map.map.GetGridPosition(graveSites[graveSites.Keys.Min()]);

				fsm.ChangeState(FSM.State.PLAYER_DEAD_IDLE);
			}
			else
			{
				// npc
				character.Hide();
				if (!IsInGroup(Globals.SAVE_GROUP))
				{
					AddToGroup(Globals.SAVE_GROUP);
				}
				character.GlobalPosition = UnitDB.GetUnitData(Name).spawnPos;

				// create spawn timer
				await ToSignal(GetTree().CreateTimer((float)GD.RandRange(60.0, 240.0), false), "timeout");
				fsm.ChangeState(FSM.State.ALIVE);
			}

		}
	}
}