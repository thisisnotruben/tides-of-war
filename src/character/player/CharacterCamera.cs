using Godot;
namespace Game.Actor
{
	public class CharacterCamera : Camera2D
	{
		public enum Effect : byte { DEATH }

		private Control[] effects;

		public override void _Ready()
		{
			effects = new Control[GetChildCount()];
			effects[(int)Effect.DEATH] = GetNode<Control>("canvasLayer/deathEffect");
		}
		public void SetEffect(Effect effect) { effects[(int)effect].Visible = Current; }
		public void ResetEffects()
		{
			foreach (Control effect in effects)
			{
				effect.Visible = false;
			}
		}
	}
}