using System;
using System.Linq;
using Godot;
using GC = Godot.Collections;
using Game.Database;
using Game.Actor;
namespace Game.Ui
{
	public class SlotController : Control, ISerializable
	{
		public enum SlotTypes : byte { NORMAL, HUD, EQUIP }

		[Signal] public delegate void OnSlotPressed(string itemName);
		[Signal] public delegate void OnSlotDragMoved(string itemName, NodePath slotFrom, NodePath slotTo);
		[Export] public SlotTypes slotType;
		[Export] public ItemDB.ItemType slotEquipType;
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
			if (slotType == SlotTypes.HUD)
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
		public void Display(string worldName, int currentStack = 1)
		{
			commodityWorldName = worldName;
			icon.Texture = PickableDB.GetIcon(worldName);
			stackCount.Text = currentStack.ToString();
			stackCount.Visible = currentStack > 1;
		}
		public void ClearDisplay()
		{
			icon.Texture = null;
			stackCount.Text = "1";
			commodityWorldName = string.Empty;
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

			SetCooldownText(time);
			tween.InterpolateMethod(this, nameof(SetCooldownText),
				time, 0.0f, time,
				Tween.TransitionType.Linear, Tween.EaseType.In);
			tween.Start();
		}
		public bool HasCommodity(string worldName) { return !commodityWorldName.Empty() && worldName.Equals(commodityWorldName); }
		public bool IsAvailable() { return icon.Texture == null; }
		public void OnTweenAllCompleted() { cooldownOverlay.Visible = coolDownText.Visible = false; }
		public virtual void OnButtonChanged(bool down) { icon.RectScale = down ? new Vector2(0.8f, 0.8f) : Vector2.One; }
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
					{"stack", stackCount.Text.Empty() ? 0 : stackCount.Text.ToInt()},
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

			SlotController slotFrom = GetNode<SlotController>((NodePath)dropData["slotPath"]);

			if (slotFrom == this
			|| (slotFrom.slotType == SlotTypes.HUD
			&& slotType == SlotTypes.NORMAL))
			{
				return false;
			}

			string itemName = dropData["itemName"].ToString();

			return slotType == SlotTypes.EQUIP
				? Globals.itemDB.HasData(itemName)
					&& Globals.itemDB.GetData(itemName).type == slotEquipType
				: true;
		}
		public new void DropData(Vector2 position, object data)
		{
			GC.Dictionary dropData = data as GC.Dictionary;
			if (!allowDrag || dropData == null)
			{
				return;
			}

			int stack = (int)dropData["stack"];
			string itemName = dropData["itemName"].ToString();
			SlotController slotFrom = GetNode<SlotController>((NodePath)dropData["slotPath"]);

			slotFrom.OnButtonChanged(false);

			if (slotType == SlotTypes.HUD)
			{
				InventoryModel inventoryModel = Globals.spellDB.HasData(itemName)
					? Player.player.menu.playerMenu.playerSpellBook
					: Player.player.menu.playerMenu.playerInventory;

				stack = PickableDB.GetStackSize(itemName) > 1
					? (from commodityName in inventoryModel.GetCommodities()
					   where itemName.Equals(commodityName)
					   select commodityName).Count()
					: 1;

				// remove duplicates
				if (slotFrom.slotType == SlotTypes.HUD)
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
			else if (slotFrom.slotType != SlotTypes.HUD)
			{
				slotFrom.ClearDisplay();
			}

			EmitSignal(nameof(OnSlotDragMoved), itemName, slotFrom.GetPath(), GetPath());
			ClearDisplay();
			Display(itemName, stack);

			if ((bool)dropData["isCoolingDown"])
			{
				SetCooldown(Globals.cooldownMaster.GetCoolDown(Player.player.GetPath(), itemName));
			}

			SaveLoadModel.dirty = true;
		}
		public void _OnSlotPressed() { EmitSignal(nameof(OnSlotPressed), commodityWorldName); }
		public GC.Dictionary Serialize()
		{
			return new GC.Dictionary()
			{
				{NameDB.SaveTag.NAME, commodityWorldName},
				{NameDB.SaveTag.STACK, stackCount.Text.ToString().ToInt()},
			};
		}
		public void Deserialize(GC.Dictionary payload)
		{
			Display(payload[NameDB.SaveTag.NAME].ToString(),
				payload[NameDB.SaveTag.STACK].ToString().ToInt());
		}
	}
}