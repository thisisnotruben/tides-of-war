using Game.Util;
using Godot;
namespace Game.Actor
{
	public class TargetDummy : Npc
	{
		public override void _OnSelectPressed()
		{
			if (Player.player.target == this)
			{
				Player.player.target = null;
				target = null;
			}
			else
			{
				Globals.PlaySound("click4", this, new Speaker());
				Tween tween = GetNode<Tween>("tween");
				tween.InterpolateProperty(img, ":scale", img.Scale, new Vector2(1.03f, 1.03f),
					0.5f, Tween.TransitionType.Elastic, Tween.EaseType.Out);
				tween.Start();
				target = Player.player;
				Player.player.target = this;
			}
		}
	}
}