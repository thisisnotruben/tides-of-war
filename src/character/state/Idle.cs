namespace Game.Actor.State
{
	public class Idle : TakeDamage
	{
		public override void Start()
		{
			animationPlayer.Stop();
			sprite.Frame = 0;
			sprite.FlipH = false;
		}
		public override void Exit() { }
	}
}