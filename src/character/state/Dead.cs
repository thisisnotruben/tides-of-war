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
		private const string deathAnim = "dying";
		private Timer timer = new Timer();

		public override void _Ready()
		{
			base._Ready();
			timer.OneShot = true;
			AddChild(timer);
			timer.Connect("timeout", this, nameof(OnNpcRespawn));
		}
		public override void Start()
		{
			character.anim.Connect("animation_finished", this, nameof(DieEnd));
			character.regenTimer.Stop();
			GD.Randomize();
			DieStart();
		}
		public override void Exit() { character.anim.Disconnect("animation_finished", this, nameof(DieEnd)); }
		public override void UnhandledInput(InputEvent @event) { }
		private void DieStart()
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
			character.anim.Play(deathAnim);
		}
		private void DieEnd(string animName)
		{
			if (!animName.Equals(deathAnim))
			{
				return;
			}

			character.Modulate = new Color("bfffffff");

			foreach (Spell spell in character.spellQueue)
			{
				spell.UnMake();
			}

			// hide combat text
			character.combatTextHandler.Hide();

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

				// set spawn timer
				timer.WaitTime = (float)GD.RandRange(60.0, 240.0);
				timer.Start();
			}
		}
		private void OnNpcRespawn()
		{
			if (character is Npc)
			{
				character.sight.Monitoring = true;
				fsm.ChangeState(FSM.State.ALIVE);
			}
		}
	}
}