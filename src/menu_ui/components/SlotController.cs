using System;
using System.Linq;
using Godot;
using GC = Godot.Collections;
using Game.Database;
using Game.GameItem;
using Game.Actor;
namespace Game.Ui
{
	public class SlotController : Control
	{
		[Signal] public delegate void OnSlotPressed(string itemName);
		[Signal] public delegate void OnSlotDragMoved(string itemName, NodePath slotFrom, NodePath slotTo);
		[Export] private bool hudSlot;

		protected Tween tween;
		protected ColorRect cooldownOverlay;
		protected Label coolDownText, stackCount;
		protected TextureRect icon;
		protected BaseButton _button;
		public BaseButton button { get { return _button; } }
		public bool allowDrag = true;
		protected bool coolDownActive;
		public string commodityWorldName = string.Empty;

		public override void _EnterTree()
		{
			base._EnterTree();
			if (hudSlot)
			{
				AddToGroup(Globals.HUD_SHORTCUT_GROUP);
			}
		}
		public override void _Ready()
		{
			tween = GetNode<Tween>("tween");
			cooldownOverlay = GetNode<ColorRect>("margin/cooldownOverlay");
			coolDownText = GetNode<Label>("margin/cooldownText");
			stackCount = GetNode<Label>("stackCount");
			icon = GetNode<TextureRect>("margin/icon");
			_button = GetNode<BaseButton>("button");

			button.Connect("pressed", this, nameof(_OnSlotPressed));
			((SlotDrag)button).slot = this;

			// default display
			ClearDisplay();
		}
		public void Display(string worldName, int currentStack)
		{
			commodityWorldName = worldName;
			icon.Texture = PickableDB.GetIcon(worldName);
			stackCount.Text = currentStack.ToString();
			stackCount.Visible = currentStack > 1;
		}
		public void ClearDisplay()
		{
			icon.Texture = null;
			stackCount.Text = commodityWorldName = string.Empty;
			cooldownOverlay.Visible = coolDownText.Visible = stackCount.Visible = coolDownActive = false;
			tween.RemoveAll();
		}
		public void SetCooldown(float time)
		{
			if ((int)time <= 0)
			{
				return;
			}

			tween.RemoveAll();
			cooldownOverlay.Visible = coolDownText.Visible = coolDownActive = true;

			tween.InterpolateMethod(this, nameof(SetCooldownText),
				time, 0.0f, time,
				Tween.TransitionType.Linear, Tween.EaseType.In);
			tween.Start();
		}
		public bool HasCommodity(string worldName) { return !commodityWorldName.Empty() && worldName.Equals(commodityWorldName); }
		public bool IsAvailable() { return icon.Texture == null; }
		public void OnTweenCompleted(Godot.Object gObject, NodePath key) { cooldownOverlay.Visible = coolDownText.Visible = false; }
		public void OnButtonChanged(bool down) { icon.RectScale = down ? new Vector2(0.8f, 0.8f) : Vector2.One; }
		protected void SetCooldownText(float time) { coolDownText.Text = Math.Round(time, 0).ToString(); }
		public new object GetDragData(Vector2 position)
		{
			if (allowDrag && !IsAvailable())
			{
				TextureRect dragIcon = new TextureRect();
				dragIcon.Texture = icon.Texture;
				dragIcon.RectSize = icon.RectSize;
				dragIcon.Expand = true;
				SetDragPreview(dragIcon);

				return new GC.Dictionary()
				{
					{"itemName", commodityWorldName},
					{"stack", stackCount.Text.Equals(string.Empty) ? 0 : stackCount.Text.ToInt()},
					{"isCoolingDown", cooldownOverlay.Visible},
					{"slotPath", GetPath()}
				};
			}

			return base.GetDragData(position);
		}
		public new bool CanDropData(Vector2 position, object data)
		{
			GC.Dictionary dropData = data as GC.Dictionary;

			if (!(allowDrag && dropData != null
			&& dropData.Contains("itemName")
			&& dropData.Contains("stack")
			&& dropData.Contains("isCoolingDown")
			&& dropData.Contains("slotPath")))
			{
				return false;
			}

			SlotController slotController = GetNode<SlotController>((NodePath)dropData["slotPath"]);
			return slotController != this
			&& ((slotController.hudSlot == hudSlot)
			|| (!slotController.hudSlot && hudSlot));
		}
		public new void DropData(Vector2 position, object data)
		{
			GC.Dictionary dropData = data as GC.Dictionary;
			if (!allowDrag || dropData == null)
			{
				return;
			}

			int stack = (int)dropData["stack"];
			string itemName = (string)dropData["itemName"];
			SlotController slotFrom = GetNode<SlotController>((NodePath)dropData["slotPath"]);

			if (hudSlot)
			{
				InventoryModel inventoryModel = Globals.spellDB.HasData(itemName)
					? Player.player.menu.gameMenu.playerSpellBook
					: Player.player.menu.gameMenu.playerInventory;

				stack = PickableDB.GetStackSize(itemName) > 1
					? (from commodityName in inventoryModel.GetCommodities()
					   where itemName.Equals(commodityName)
					   select commodityName).Count()
					: 1;

				// remove duplicates
				if (slotFrom.hudSlot)
				{
					slotFrom.ClearDisplay();
				}
				else
				{
					SlotController hudSlotController;
					foreach (Node node in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
					{
						hudSlotController = node as SlotController;
						if (hudSlotController != null
						&& itemName.Equals(hudSlotController.commodityWorldName))
						{
							hudSlotController.ClearDisplay();
						}
					}
				}
			}
			else
			{
				slotFrom.ClearDisplay();
			}

			EmitSignal(nameof(OnSlotDragMoved), itemName, slotFrom.GetPath(), GetPath());
			ClearDisplay();
			Display(itemName, stack);

			if ((bool)dropData["isCoolingDown"])
			{
				SetCooldown(Commodity.GetCoolDown(Player.player.GetPath(), itemName));
			}
		}
		public void _OnSlotPressed() { EmitSignal(nameof(OnSlotPressed), commodityWorldName); }
	}
}