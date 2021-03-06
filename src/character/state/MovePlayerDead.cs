using Godot;
namespace Game.Actor.State
{
	public class MovePlayerDead : MovePlayer
	{
		public override void Harm(int damage, Vector2 direction) { }
	}
}