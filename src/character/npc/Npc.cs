using Game.Database;
using Godot;
namespace Game.Actor
{
	public class Npc : Character
	{
		public override void _Ready()
		{
			base._Ready();
			base.init();
			init();
		}
		public override void init()
		{
			UnitDB.UnitNode unitNode = UnitDB.GetUnitData(Name);
			worldName = unitNode.name;
			SetImg(unitNode.img);
			enemy = unitNode.enemy;
			if (ContentDB.HasContent(Name))
			{
				enemy = ContentDB.GetContentData(Name).enemy;
			}
			SetAttributes();
			hp = hpMax;
			mana = manaMax;
		}
		public void _OnSightAreaEnteredExited(Area2D area2D, bool entered)
		{
			if (entered)
			{
				Character character = area2D.Owner as Character;
				if (character != null && !dead && (target == null || character.dead) && character != this)
				{
					if (!enemy && character is Player)
					{
						// friendly npcs' don't attack player
						return;
					}
					else if (!engaging && enemy != character.enemy)
					{
						target = character;
						SetProcess(true);
					}
				}
			}
			else if (!engaging)
			{
				target = null;
			}
		}
		public void _OnAreaMouseEnteredExited(bool entered) { Player.player.SetProcessUnhandledInput(!entered); }
		public void _OnScreenEnteredExited(bool entered) { SetProcess(entered); }
		public void CheckSight(Area2D area2D) { _OnSightAreaEnteredExited(area2D, true); }
		public override void _OnSelectPressed() { Player.player.GetMenu().NpcInteract(this); }
	}
}