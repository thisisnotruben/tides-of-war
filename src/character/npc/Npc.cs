using Game.Database;
using Game.Actor.State;
using Godot;
namespace Game.Actor
{
	public class Npc : Character
	{
		public override void _Ready()
		{
			base._Ready();
			UnitDB.UnitNode unitNode = UnitDB.GetUnitData(Name);
			enemy = unitNode.enemy;
			level = unitNode.level;
			if (!unitNode.name.Empty())
			{
				worldName = unitNode.name;
			}
			if (ContentDB.HasContent(Name))
			{
				ContentDB.ContentNode contentNode = ContentDB.GetContentData(Name);
				enemy = contentNode.enemy;
				level = contentNode.level;
			}
			SetImg(unitNode.img);
		}
		public void _OnCharacterEnteredSight(Area2D area2D)
		{
			Character character = area2D.Owner as Character;
			if (character != null)
			{
				Npc npc = character as Npc;

				if ((character is Player && enemy)
				|| (npc != null && npc.enemy != enemy))
				{
					// attack
					target = character;
					state = FSM.State.ATTACK;
				}
			}
		}
		public void _OnCharacterExitedSight(Area2D area2D)
		{
			Character character = area2D.Owner as Character;

			if (character != null && character == target && !attacking)
			{
				target = null;
			}
		}
		public virtual void _OnSelectPressed()
		{
			if (Player.player.state == FSM.State.IDLE || Player.player.moving)
			{
				Player.player.menu.NpcInteract(this);
			}
		}
		public void _OnTweenCompleted(Godot.Object obj, NodePath nodePath)
		{
			Tween tween = GetNode<Tween>("tween");
			if (obj is Node2D)
			{
				Node2D node2Obj = (Node2D)obj;
				if (!node2Obj.Scale.Equals(new Vector2(1.0f, 1.0f)))
				{
					// Reverts to original scale when unit is clicked on
					tween.InterpolateProperty(node2Obj, nodePath, node2Obj.Scale,
						new Vector2(1.0f, 1.0f), 0.5f, Tween.TransitionType.Cubic, Tween.EaseType.Out);
					tween.Start();
				}
			}
			if (tween.PauseMode == PauseModeEnum.Process)
			{
				// Condition when merchant/trainer are selected to let their
				// animation run through it's course when game is paused
				tween.PauseMode = PauseModeEnum.Inherit;
			}
		}
		// makes sure player clicks on unit and not mistakingly select the tile to move to
		public void _OnAreaMouseEnteredExited(bool entered) { Player.player.SetProcessUnhandledInput(!entered); }
	}
}