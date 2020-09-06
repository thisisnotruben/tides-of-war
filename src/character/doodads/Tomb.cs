using Game.Utils;
using Game.Actor.State;
using Godot;
namespace Game.Actor.Doodads
{
	public class Tomb : Node2D
	{
		private Player deceasedPlayer;
		private Tween tween;
		private Area2D sight;

		public override void _Ready()
		{
			tween = GetNode<Tween>("tween");
			sight = GetNode<Area2D>("img/sight");
		}
		public void Init(Player deceasedPlayer)
		{
			this.deceasedPlayer = deceasedPlayer;
			this.deceasedPlayer.menu.GetNode<BaseButton>("c/osb/m/cast").Connect("pressed", this, nameof(Revive));
			this.deceasedPlayer.menu.GetNode<Control>("c/osb/").SetPosition(new Vector2(0.0f, 180.0f));
			this.deceasedPlayer.menu.GetNode<Label>("c/osb/m/cast/label").Text = "Revive";

			// set on map correctly
			GlobalPosition = Map.Map.map.GetGridPosition(this.deceasedPlayer.GlobalPosition);
		}
		public void _OnAreaEntered(Area2D area2D)
		{
			deceasedPlayer.menu.GetNode<CanvasItem>("c/osb").Show();
		}
		public void _OnAreaExited(Area2D area2D)
		{
			if (area2D.Owner == deceasedPlayer)
			{
				deceasedPlayer.menu.GetNode<CanvasItem>("c/osb").Hide();
			}
		}
		public async void Revive()
		{
			Globals.PlaySound("click2", this, new Speaker());

			// hide button to avoid double click
			sight.Monitoring = false;
			deceasedPlayer.menu.GetNode<CanvasItem>("c/osb").Hide();

			// revive
			Map.Map.map.SetVeil(false);
			deceasedPlayer.state = FSM.State.ALIVE;

			// fade out tomb
			tween.InterpolateProperty(this, ":modulate", Modulate,
				new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.75f, Tween.TransitionType.Circ, Tween.EaseType.Out);
			tween.Start();

			await ToSignal(tween, "tween_completed");

			// delete tomb
			QueueFree();
		}
	}
}