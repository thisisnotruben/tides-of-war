using System.Collections.Generic;
using System.Linq;
using System;
using Game.Actor.Doodads;
using Game.Database;
using Game.Quest;
using Godot;
using GC = Godot.Collections;
namespace Game.Actor.State
{
	public class Dead : StateBehavior
	{
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
			Globals.TryLinkSignal(character.anim, "animation_finished", this, nameof(DieEnd), true);
			character.regenTimer.Stop();
			DieStart();
		}
		public override void Exit() { Globals.TryLinkSignal(character.anim, "animation_finished", this, nameof(DieEnd), false); }
		public override void OnAttacked(Character whosAttacking) { ClearOnAttackedSignals(whosAttacking); }
		public override void UnhandledInput(InputEvent @event) { }
		private void DieStart()
		{
			Map.Map.map.OccupyCell(character.GlobalPosition, false);

			// changing character detection
			character.hitBox.CollisionLayer = Character.COLL_MASK_DEAD;
			if (character is Player)
			{
				character.hitBox.CollisionLayer += Character.COLL_MASK_PLAYER;
			}

			// stop health/mana regenerations
			character.regenTimer.Stop();

			// clear targets
			if (character is Npc)
			{
				Globals.questMaster.CheckQuests(character.worldName, QuestDB.QuestType.KILL, true);

				if (character.target != null
				&& character.target is Player
				&& character.target.target == character)
				{
					((Player)character.target).menu.ClearTarget();
				}
			}
			else
			{
				(character as Player)?.menu.ClearTarget();
			}

			// play death animation
			character.anim.Play(Character.ANIM_DIE);
		}
		private void DieEnd(string animName)
		{
			if (!animName.Equals(Character.ANIM_DIE))
			{
				return;
			}

			character.Modulate = new Color("bfffffff");

			// hide combat text
			character.combatTextHandler.Hide();

			Player player = character as Player;
			if (player != null)
			{
				((Tomb)SceneDB.tomb.Instance()).Init(player, Map.Map.map.GetGridPosition(character.GlobalPosition));

				// spawn to nearest graveyard
				Dictionary<int, Vector2> graveSites = new Dictionary<int, Vector2>();
				foreach (Node2D graveyard in GetTree().GetNodesInGroup(Globals.GRAVE_GROUP))
				{
					graveSites[Map.Map.map.getAPath(character.GlobalPosition, graveyard.GlobalPosition).Count] = graveyard.GlobalPosition;
				}
				character.GlobalPosition = Map.Map.map.GetGridPosition(graveSites[graveSites.Keys.Min()]);

				fsm.ChangeState(FSM.State.IDLE_DEAD);
			}
			else
			{
				// TODO: drop loot

				// npc
				character.Visible = character.sight.Monitoring = false;
				character.GlobalPosition = Globals.unitDB.GetData(character.Name).spawnPos;

				// set spawn timer
				timer.Start(new Random().Next(60, 241));
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
		public override GC.Dictionary Serialize()
		{
			GC.Dictionary payload = base.Serialize();
			payload[NameDB.SaveTag.TIME_LEFT] = timer.TimeLeft;
			return payload;
		}
		public override void Deserialize(GC.Dictionary payload)
		{
			base.Deserialize(payload);

			float animPos = (float)payload[NameDB.SaveTag.ANIM_POSITION],
				timeLeft = (float)payload[NameDB.SaveTag.TIME_LEFT];

			if (animPos > 0.0
			&& animPos < character.anim.GetAnimation(Character.ANIM_DIE).Length)
			{
				character.anim.Seek(animPos, true);
			}
			else if (timeLeft > 0.0f)
			{
				character.anim.Stop();
				DieEnd(Character.ANIM_DIE);
				timer.Start(timeLeft);
			}
		}
	}
}