using Game.Actor;
using Game.Ui;
using Game.Database;
using Godot;
using System;
namespace Game.Map.Doodads
{
	public class InteractItem : Sprite, ICollectable
	{
		private TextureButton select;
		private Tween tween;
		private Control dialogue;
		private ShaderMaterial shaderMaterial;

		private string interactType = string.Empty,
			value = string.Empty;

		public override void _Ready()
		{
			Sprite highlighter = GetNode<Sprite>("highlighter");
			highlighter.Texture = Texture;
			highlighter.RegionRect = RegionRect;
			highlighter.Offset = Offset;
			shaderMaterial = (ShaderMaterial)highlighter.Material;

			Area2D area2D = GetNode<Area2D>("area2D");
			area2D.Connect("area_entered", this, nameof(OnPlayerEntered));
			area2D.Connect("area_exited", this, nameof(OnPlayerExited));

			select = GetNode<TextureButton>("select");
			select.Connect("pressed", this, nameof(OnSelectPressed));
			select.Hide();

			tween = GetNode<Tween>("tween");
			tween.Connect("tween_all_completed", this, nameof(OnTweenAllCompleted));

			Hide();
		}
		public void SetInteractType(string interactType, string value)
		{
			this.interactType = interactType;
			this.value = value;
		}
		protected virtual void OnPlayerEntered(Area2D area2D)
		{
			Player player = area2D.Owner as Player;
			if (player != null && !player.dead)
			{
				ShowInteractAnim(true);
			}
		}
		protected virtual void OnPlayerExited(Area2D area)
		{
			OnDialogueHide();
			ShowInteractAnim(false);
		}
		protected void ShowInteractAnim(bool interact, bool bounce = false)
		{
			select.Visible = interact;
			shaderMaterial.SetShaderParam("energy", interact ? 0.13f : 0.1f);

			if (bounce)
			{
				if (interact)
				{
					tween.RemoveAll();
				}

				tween.InterpolateProperty(this, "scale", Scale, new Vector2(1.1f, 1.1f),
					0.5f, Tween.TransitionType.Bounce, Tween.EaseType.In);
				tween.Start();
			}
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
		protected virtual void OnSelectPressed()
		{
			switch (interactType)
			{
				case "talk":
					ShowDialogue(value);
					break;
				case "loot":
					// TODO: play sound to signaify loot
					Player.player.menu.LootInteract(this, value);
					break;
			}
		}
		public void Collect()
		{
			Hide();
			// TODO: set a timer for it pop back into existance?
		}
		protected void ShowDialogue(string DialogueName)
		{
			dialogue = DialogicSharp.Start(DialogueName);
			dialogue.Connect("dialogic_signal", this, nameof(OnDialogueSignalCallback));
			dialogue.Connect("tree_exited", this, nameof(ClearDialoguePtr));
			dialogue.Connect("hide", this, nameof(OnDialogueHide));

			Player.player.menu.ShowTransitionDialogue(dialogue);
		}
		protected void OnDialogueHide()
		{
			dialogue?.QueueFree();
			ClearDialoguePtr();
		}
		protected void ClearDialoguePtr() { dialogue = null; }
		protected virtual void OnDialogueSignalCallback(object value)
		{
			// util
			MenuMasterController menuMaster = Player.player.menu;
			Action<string> showError = (string errorMsg) =>
			{
				Globals.soundPlayer.PlaySound(NameDB.UI.CLICK6);
				menuMaster.errorPopup.ShowError(errorMsg);
			};

			// add to inventory/spellbook if not full
			string itemName = value.ToString().Trim();
			if (Globals.itemDB.HasData(itemName) && menuMaster.playerMenu.playerInventory.AddCommodity(itemName) == -1)
			{
				showError("Inventory Full!");
			}
			else if (Globals.spellDB.HasData(itemName) && menuMaster.playerMenu.playerSpellBook.AddCommodity(itemName) == -1)
			{
				showError("Spellbook Full!");
			}
			else
			{
				GD.PrintErr($"{GetPath()}: {itemName} not in database.");
			}
		}
	}
}