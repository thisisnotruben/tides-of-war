using System.Collections.Generic;
using Game.Database;
using Godot;
namespace Game.Ui
{
	public class ItemSlot : TextureButton
	{
		public enum SlotType : byte { BAG_SLOT, HUD_SLOT, SHORTCUT }
		public SlotType slotType = SlotType.BAG_SLOT;
		private bool allowCoolDown;
		private List<string> itemStack = new List<string>();
		private float time;
		private int stackSize;
		[Signal]
		public delegate void SlotSelected(int index);
		[Signal]
		public delegate void StackSizeChanged(ItemSlot itemSlot, string worldName, int itemStackSize);
		[Signal]
		public delegate void Cooldown(float time, float seek);
		[Signal]
		public delegate void SyncSlot(ItemSlot itemSlot, string pickableWorldName);
		[Signal]
		public delegate void ShortcutPressed(ItemSlot itemSlot, string pickableWorldName);

		public void _OnItemSlotPressed()
		{
			if (slotType == SlotType.BAG_SLOT && !GetItem().Empty() && GetTree().Paused)
			{
				EmitSignal(nameof(SlotSelected), GetPositionInParent());
			}
		}
		public void _OnShortcutPressed()
		{
			if (slotType == SlotType.SHORTCUT && !GetItem().Empty())
			{
				EmitSignal(nameof(ShortcutPressed), this, GetItem());
			}
		}
		public void _OnTweenCompleted(Godot.Object obj, NodePath nodePath)
		{
			if (slotType == SlotType.HUD_SLOT)
			{
				SetItem("", false, true);
				Hide();
			}
			MoveChild(GetNode("count"), 2);
			allowCoolDown = false;
			GetNode<Control>("m/label").Hide();
			GetNode<Control>("m/icon/overlay").RectScale = new Vector2(1.0f, 1.0f);
		}
		public void _OnTweenStep(Godot.Object obj, NodePath nodePath, float elapsed, Godot.Object value)
		{
			if (allowCoolDown)
			{
				Label label = GetNode<Label>("m/label");
				label.Text = Mathf.Round(time - elapsed).ToString();
				if (!label.Visible)
				{
					label.Show();
				}
			}
		}
		public void _OnSyncShortcut(ItemSlot slot, string pickableWorldName)
		{
			SetItem(pickableWorldName, false);
			if (pickableWorldName.Empty())
			{
				slot.Disconnect(nameof(SyncSlot), this, nameof(_OnSyncShortcut));
			}
		}
		public void SetItem(string pickableWorldName, bool shuffle = true, bool forceClear = false, bool funnel = false)
		{
			if (pickableWorldName.Empty())
			{
				EmitSignal(nameof(SyncSlot), this, pickableWorldName);
				if (forceClear)
				{
					GetNode<TextureRect>("m/icon").Texture = null;
				}
				else
				{
					itemStack.RemoveAt(0);
					GetNode<Label>("count").Text = itemStack.Count.ToString();
					if (itemStack.Count == 1)
					{
						GetNode<Label>("count").Hide();
					}
					else if (itemStack.Count == 0)
					{
						GetNode<TextureRect>("m/icon").Texture = null;
					}
					else if (!funnel)
					{
						EmitSignal(nameof(StackSizeChanged), GetItem(), itemStack.Count, this);
					}
				}
				if (GetNode<TextureRect>("m/icon").Texture == null)
				{
					TextureNormal = (Texture)GD.Load("res://asset/img/ui/brown_bg_icon.tres");
					foreach (Godot.Collections.Dictionary link in GetSignalConnectionList(nameof(SyncSlot)))
					{
						Disconnect(nameof(SyncSlot), (Godot.Object)link["target"], nameof(_OnSyncShortcut));
					}
					allowCoolDown = false;
					itemStack.Clear();
					stackSize = 0;
					GetNode<Control>("count").Hide();
					GetNode<Control>("m/label").Hide();
					GetNode<Tween>("tween").StopAll();
					GetNode<Control>("m/icon/overlay").RectScale = new Vector2(1.0f, 1.0f);
					if (shuffle)
					{
						GetParent().MoveChild(this, GetParent().GetChildCount() - 1);
					}
					if (slotType == SlotType.HUD_SLOT)
					{
						Hide();
					}
				}
			}
			else
			{
				string texPath = "res://asset/img/ui/black_bg_icon_used" +
					$"{((slotType == SlotType.SHORTCUT) ? 0 : 1)}.tres";
				if (!TextureNormal.ResourcePath.Equals(texPath))
				{
					TextureNormal = (Texture)GD.Load(texPath);
				}
				GetNode<TextureRect>("m/icon").Texture = PickableDB.GetIcon(pickableWorldName);
				int pickableStackSize = PickableDB.GetStackSize(pickableWorldName);
				if (pickableStackSize > 0)
				{
					stackSize = pickableStackSize;
					if (itemStack.Count > 0 && !pickableWorldName.Equals(GetItem()))
					{
						itemStack.Clear();
					}
					itemStack.Add(pickableWorldName);
					if (itemStack.Count > 1)
					{
						GetNode<Label>("count").Text = itemStack.Count.ToString();
						GetNode<Control>("count").Show();
					}
					else
					{
						GetNode<Control>("count").Hide();
					}
				}
				else
				{
					GetNode<Tween>("tween").StopAll();
					GetNode<Control>("count").Hide();
					stackSize = 0;
					itemStack.Clear();
					itemStack.Add(pickableWorldName);
				}
			}
		}
		public void CoolDown(string pickableWorldName, float value, float seek)
		{
			if (!GetItem().Empty() && !pickableWorldName.Empty() && GetItem().Equals(pickableWorldName) &&
			!allowCoolDown && value > 0.0f && value != seek)
			{
				allowCoolDown = true;
				time = value;
				GetNode<Label>("m/label").Text = Mathf.Round(value).ToString();
				MoveChild(GetNode("count"), 0);
				GetNode<Control>("m/label").Show();
				Tween tween = GetNode<Tween>("tween");
				tween.InterpolateProperty(GetNode<ColorRect>("m/icon/overlay"), ":rect_scale",
					new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f), time, Tween.TransitionType.Linear, Tween.EaseType.InOut);
				tween.Start();
				if (seek > 0.0f)
				{
					tween.Seek(seek);
				}
				EmitSignal(nameof(Cooldown), GetItem(), value, seek);
			}
		}
		public void _OnLabelDraw() { GetNode<Control>("m/icon/overlay").Show(); }
		public void _OnLabelHide() { GetNode<Control>("m/icon/overlay").Hide(); }
		public void _OnItemSlotButtonDown() { GetNode<Control>("m/icon").RectScale = new Vector2(0.8f, 0.8f); }
		public void _OnItemSlotButtonUp() { GetNode<Control>("m/icon").RectScale = new Vector2(1.0f, 1.0f); }
		public string GetItem() { return (itemStack.Count > 0) ? itemStack[0] : ""; }
		public bool IsFull() { return itemStack.Count == stackSize; }
		public float GetCoolDownTimeLeft() { return GetNode<Tween>("tween").Tell(); }
		public float GetCoolDownInitialTime() { return time; }
		public bool IsStacking() { return stackSize > 0; }
		public bool IsCoolingDown() { return GetCoolDownTimeLeft() > 0.0f; }
		public List<string> GetItemStack() { return itemStack; }
	}
}