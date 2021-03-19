using Godot;
namespace Game.Actor
{
	public class TargetDummy : Npc
	{
		public override void _Ready()
		{
			base._Ready();
			enemy = true;
			worldName = "Target Dummy";
			RemoveFromGroup(Globals.SAVE_GROUP);
		}
		public override void Harm(int damage, Vector2 direction) { }
		public override void OnAttacked(Character whosAttacking) { }
		public override void _OnCharacterEnteredSight(Area2D area2D) { }
		public override void _OnCharacterExitedSight(Area2D area2D) { }
		public override bool ShouldSerialize() { return false; }
	}
}