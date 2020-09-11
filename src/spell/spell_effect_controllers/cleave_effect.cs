using Godot;
namespace Game.Ability
{
	public class cleave_effect : SpellEffect
	{
		public override void OnHit(Spell spell = null)
		{
			base.OnHit(spell);
			Position = character.img.Position;
			tween.Start();
			timer.Start();
		}
	}
}