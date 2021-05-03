using Game.Actor;
using Game.Database;
using Game.Ui;
using Godot;
namespace Game.Map.Doodads
{
	public class TransitionSign : Sprite
	{
		private string mapPath = string.Empty;
		private TextureButton select;
		private Tween tween;
		private Control dialogue;
		private ShaderMaterial shaderMaterial;

		public override void _Ready()
		{
			Sprite highlighter = GetNode<Sprite>("highlighter");
			highlighter.Texture = Texture;
			highlighter.RegionRect = RegionRect;
			shaderMaterial = (ShaderMaterial)highlighter.Material;

			string sceneMapPath = string.Format(PathManager.sceneMapPath, Name.Split("-")[1]);
			mapPath = sceneMapPath;

			Area2D area2D = GetNode<Area2D>("area2D");
			area2D.Connect("area_entered", this, nameof(OnPlayerEntered));
			area2D.Connect("area_exited", this, nameof(OnPlayerExited));

			select = GetNode<TextureButton>("select");
			select.Connect("pressed", this, nameof(OnSelectPressed));
			select.Hide();

			tween = GetNode<Tween>("tween");
			tween.Connect("tween_all_completed", this, nameof(OnTweenAllCompleted));
		}
		private void OnPlayerEntered(Area2D area)
		{
			Player player = area.Owner as Player;
			if (!(player == null || player.dead || player.attacking))
			{
				ShowInteractAnim(true);
			}
		}
		private void OnPlayerExited(Area2D area)
		{
			OnDialogueHide();
			ShowInteractAnim(false);
		}
		private void ShowInteractAnim(bool interact)
		{
			select.Visible = interact;
			shaderMaterial.SetShaderParam("energy", interact ? 0.13f : 0.1f);
			if (interact)
			{
				tween.RemoveAll();
			}

			tween.InterpolateProperty(this, "scale", Scale, new Vector2(1.1f, 1.1f),
				0.5f, Tween.TransitionType.Bounce, Tween.EaseType.In);
			tween.Start();
		}
		private void OnTweenAllCompleted()
		{
			if (!Scale.Equals(Vector2.One))
			{
				tween.InterpolateProperty(this, "scale", Scale, Vector2.One,
					0.5f, Tween.TransitionType.Bounce, Tween.EaseType.Out);
				tween.Start();
			}
		}
		private void OnSelectPressed()
		{
			dialogue = Globals.dialogic.Start(Name);
			dialogue.Connect("dialogic_signal", this, nameof(OnDialogueSignalCallback));
			dialogue.Connect("tree_exited", this, nameof(ClearDialoguePtr));
			dialogue.Connect("hide", this, nameof(OnDialogueHide));

			Player.player.menu.ShowTransitionDialogue(dialogue);
		}
		private void OnDialogueHide()
		{
			dialogue?.QueueFree();
			ClearDialoguePtr();
		}
		private void ClearDialoguePtr() { dialogue = null; }
		private void OnDialogueSignalCallback(object value)
		{
			Name = Name + "-DELETE";
			Globals.sceneLoader.SetScene(mapPath, Map.map, true);
			Player.player.menu.ShowLoadView();
		}
	}
}
