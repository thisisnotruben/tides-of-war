using Godot;
namespace Game.Ability
{
	public class explosive_arrow_effect : SpellEffect
	{
		public override void OnHit(Spell spell = null)
		{
			base.OnHit(spell);
			light.Show();
			tween.Start();
			timer.Start();
		}
	}
}