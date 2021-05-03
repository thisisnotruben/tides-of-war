using Game.Actor.State;
using Game.Database;
using Game.Ui;
using Godot;
using GC = Godot.Collections;
namespace Game.Actor.Doodads
{
	public class Tomb : Node2D, ISerializable
	{
		private Player deceasedPlayer;
		private Tween tween;
		private Area2D sight;
		private HudPopupConfirmController confirmPopup;

		public override void _Ready()
		{
			tween = GetChild<Tween>(0);
			sight = GetNode<Area2D>("img/sight");
		}
		public Tomb Init(Player deceasedPlayer, Vector2 spawnPos)
		{
			this.deceasedPlayer = deceasedPlayer;
			this.confirmPopup = deceasedPlayer.menu.confirmPopup;

			Map.Map.map.AddZChild(this);

			GlobalPosition = spawnPos;

			Map.Map.map.SetVeil(true);
			deceasedPlayer.camera.SetEffect(CharacterCamera.Effect.DEATH);

			return this;
		}
		public void OnTweenAllCompleted() { QueueFree(); }
		public void _OnAreaEntered(Area2D area2D)
		{
			if (area2D.Owner == deceasedPlayer)
			{
				Globals.TryLinkSignal(confirmPopup.button, "pressed", this, nameof(Revive), true);
				confirmPopup.ShowConfirm("Revive?");
			}
		}
		public void _OnAreaExited(Area2D area2D)
		{
			if (area2D.Owner == deceasedPlayer)
			{
				Globals.TryLinkSignal(confirmPopup.button, "pressed", this, nameof(Revive), false);
				confirmPopup.Hide();
			}
		}
		public void Revive()
		{
			Globals.soundPlayer.PlaySound(NameDB.UI.CLICK2);

			// hide button to avoid double click
			sight.Monitoring = false;
			Globals.TryLinkSignal(confirmPopup.button, "pressed", this, nameof(Revive), false);
			confirmPopup.Hide();

			// revive
			Map.Map.map.SetVeil(false);
			deceasedPlayer.camera.ResetEffects();
			deceasedPlayer.state = FSM.State.ALIVE;

			// fade out tomb and delete
			tween.InterpolateProperty(this, "modulate",
				Modulate, Color.ColorN("white", 0.0f),
				0.75f, Tween.TransitionType.Circ, Tween.EaseType.Out);
			tween.Start();
		}
		public GC.Dictionary Serialize()
		{
			return new GC.Dictionary()
			{
				{NameDB.SaveTag.POSITION, new GC.Array() {GlobalPosition.x, GlobalPosition.y}}
			};
		}
		public void Deserialize(GC.Dictionary payload) { }
	}
}