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

			if (Globals.unitDB.HasData(Name))
			{
				UnitDB.UnitData unitData = Globals.unitDB.GetData(Name);
				enemy = unitData.enemy;
				level = unitData.level;
				if (!unitData.name.Empty())
				{
					worldName = unitData.name;
				}
				SetImg(unitData.img);
			}
		}
		public void _OnCharacterEnteredSight(Area2D area2D)
		{
			Character character = area2D.Owner as Character;
			if (!attacking && character != null)
			{
				Npc npc = character as Npc;

				if ((character is Player && enemy)
				|| (npc != null && npc.enemy != enemy))
				{
					// attack
					target = character;

					state = pos.DistanceTo(character.pos) > stats.weaponRange.value
						? FSM.State.NPC_MOVE_ATTACK
						: FSM.State.ATTACK;
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
		public void _OnTweenCompleted(Godot.Object obj, NodePath nodePath)
		{
			Tween tween = GetNode<Tween>("tween");
			if (obj is Node2D)
			{
				Node2D node2Obj = (Node2D)obj;
				if (!node2Obj.Scale.Equals(Vector2.One))
				{
					// Reverts to original scale when unit is clicked on
					tween.InterpolateProperty(node2Obj, nodePath, node2Obj.Scale,
						Vector2.One, 0.5f, Tween.TransitionType.Cubic, Tween.EaseType.Out);
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
		public virtual void _OnSelectPressed() { Player.player.menu.NpcInteract(this); }
		public bool ShouldSerialize()
		{
			return hp < stats.hpMax.valueI || mana < stats.manaMax.valueI || dead || moving || attacking;
		}
	}
}