using Game.Util;
using Game.Actor;
using Godot;
namespace Game.Loot
{
	public class TreasureChest : Node2D
	{
		public string commodityWorldName { get; private set; }
		private Timer timer;
		private Tween tween;
		private AnimationPlayer animationPlayer;
		private Node2D select;
		private Area2D sight;
		private Sprite img;
		private Speaker2D speaker2D;

		public override void _Ready()
		{
			timer = GetNode<Timer>("timer");
			tween = GetNode<Tween>("tween");
			animationPlayer = GetNode<AnimationPlayer>("anim");
			select = GetNode<Node2D>("select");
			sight = GetNode<Area2D>("sight");
			img = GetNode<Sprite>("img");
			speaker2D = GetNode<Speaker2D>("speaker2d");
		}
		public void Init(string commodityWorldName) { this.commodityWorldName = commodityWorldName; }
		private void Delete() { QueueFree(); }
		public void Collect()
		{
			// make sure to not double click
			select.Hide();
			sight.Monitoring = false;

			// start all animations/sounds
			Globals.PlaySound("chest_collect", this, speaker2D);
			animationPlayer.Queue("collect");
		}
		public void _OnSightAreaEntered(Area2D area2D)
		{
			// add a litte effect when player comes near
			tween.InterpolateProperty(this, ":scale", Scale, new Vector2(1.05f, 1.05f),
				0.5f, Tween.TransitionType.Bounce, Tween.EaseType.In);

			// allow player to select chest
			select.Show();

			// start all animations/sounds
			Globals.PlaySound("chest_open", this, speaker2D);
			animationPlayer.Queue("open_chest");
			tween.Start();
		}
		public void _OnSightAreaExited(Area2D area2D)
		{
			// doesn't allow player to select chest
			select.Hide();

			// start all animations/sounds
			Globals.PlaySound("chest_open", this, speaker2D);
			animationPlayer.Queue("close_chest");
		}
		public void _OnSelectPressed() { Player.player.menu.LootInteract(this); }
		public void OnTweenCompleted(Godot.Object gObject, NodePath key)
		{
			Node2D node2D = gObject as Node2D;
			if (key.Equals(":scale") && !node2D.Scale.Equals(Vector2.One))
			{
				// return to normal scale
				tween.InterpolateProperty(this, ":scale", this.Scale, Vector2.One,
					0.5f, Tween.TransitionType.Bounce, Tween.EaseType.Out);
				tween.Start();
			}
		}
		public void OnAnimFinished(string animName)
		{
			if (animName.Equals("collect"))
			{
				// makes a smooth animation if chest
				// is in middle of of another frame
				tween.InterpolateProperty(img, ":frame", img.Frame, 0,
					animationPlayer.GetAnimation("collect").Length,
					Tween.TransitionType.Linear, Tween.EaseType.In);
				tween.Start();

				timer.Start();
			}
		}
	}
}