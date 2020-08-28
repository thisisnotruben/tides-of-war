namespace Game.Actor.State
{
	public class Idle : TakeDamage
	{
		public override void Start()
		{
			character.anim.Stop();
			character.img.Frame = 0;
			character.img.FlipH = false;
		}
		public override void Exit() { }
	}
}