using Game.Missile;
using Godot;
namespace Game.Ability
{
	public class mind_blast_effect : SpellEffect
	{
		public override void OnHit(Spell spell = null)
		{
			base.OnHit(spell);
			Bolt bolt = Owner as Bolt;
			if (bolt != null)
			{
				bolt.GlobalPosition = bolt.target.head.GlobalPosition;
				tween.Start();
				timer.Start();
			}
			else
			{
				GD.Print("Owner not bolt in class MindBlast");
			}
		}
	}
}