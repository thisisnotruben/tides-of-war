using System.Collections.Generic;
using Game.Misc.Loot;
using Godot;
namespace Game.Ui
{
    public class ItemSlot : TextureButton
    {
        public enum SlotType : byte { BAG_SLOT, HUD_SLOT, SHORTCUT }
        public SlotType slotType = SlotType.BAG_SLOT;
        private bool allowCoolDown;
        private bool stacking;
        private List<Pickable> itemStack = new List<Pickable>();
        private float time;
        private ushort stackSize;
        [Signal]
        public delegate void SlotSelected(int index);
        [Signal]
        public delegate void StackSizeChanged(ItemSlot itemSlot, string worldName, ushort itemStackSize);
        [Signal]
        public delegate void Cooldown(float time, float seek);
        [Signal]
        public delegate void SyncSlot(ItemSlot itemSlot, Pickable pickable);
        [Signal]
        public delegate void ShortcutPressed(ItemSlot itemSlot, Pickable pickable);
        public void _OnItemSlotPressed()
        {
            if (slotType == SlotType.BAG_SLOT && GetItem() != null && GetTree().IsPaused())
            {
                EmitSignal(nameof(SlotSelected), GetPositionInParent());
            }
        }
        public void _OnShortcutPressed()
        {
            GD.Print(slotType);
            if (slotType == SlotType.SHORTCUT && GetItem() != null)
            {
                EmitSignal(nameof(ShortcutPressed), this, GetItem());
            }
        }
        public void _OnItemSlotButtonDown()
        {
            GetNode<Control>("m/icon").SetScale(new Vector2(0.8f, 0.8f));
        }
        public void _OnItemSlotButtonUp()
        {
            GetNode<Control>("m/icon").SetScale(new Vector2(1.0f, 1.0f));
        }
        public void _OnTweenCompleted(Godot.Object obj, NodePath nodePath)
        {
            if (slotType == SlotType.HUD_SLOT)
            {
                SetItem(null, false, true);
                Hide();
            }
            MoveChild(GetNode("count"), 2);
            allowCoolDown = false;
            GetNode<Control>("m/label").Hide();
            GetNode<Control>("m/icon/overlay").SetScale(new Vector2(1.0f, 1.0f));
        }
        public void _OnTweenStep(Godot.Object obj, NodePath nodePath, float elapsed, Godot.Object value)
        {
            if (allowCoolDown)
            {
                Label label = GetNode<Label>("m/label");
                label.SetText(Mathf.Round(time - elapsed).ToString());
                if (!label.IsVisible())
                {
                    label.Show();
                }
            }
        }
        public void _OnLabelDraw()
        {
            GetNode<Control>("m/icon/overlay").Show();
        }
        public void _OnLabelHide()
        {
            GetNode<Control>("m/icon/overlay").Hide();
        }
        public void _OnSyncShortcut(ItemSlot slot, Pickable pickable)
        {
            SetItem(pickable, false);
            if (pickable == null)
            {
                slot.Disconnect(nameof(SyncSlot), this, nameof(_OnSyncShortcut));
            }
        }
        public void SetItem(Pickable pickable, bool shuffle = true, bool forceClear = false, bool funnel = false)
        {
            if (pickable == null)
            {
                EmitSignal(nameof(SyncSlot), this, pickable);
                if (forceClear)
                {
                    GetNode<TextureRect>("m/icon").SetTexture(null);
                }
                else
                {
                    itemStack.RemoveAt(0);
                    GetNode<Label>("count").SetText(itemStack.Count.ToString());
                    if (itemStack.Count == 1)
                    {
                        GetNode<Label>("count").Hide();
                    }
                    else if (itemStack.Count == 0)
                    {
                        GetNode<TextureRect>("m/icon").SetTexture(null);
                    }
                    else if (!funnel)
                    {
                        EmitSignal(nameof(StackSizeChanged), GetItem().GetWorldName(), itemStack.Count, this);
                    }
                }
                if (GetNode<TextureRect>("m/icon").GetTexture() == null)
                {
                    SetNormalTexture((Texture)GD.Load("res://asset/img/ui/brown_bg_icon.res"));
                    foreach (Godot.Collections.Dictionary link in GetSignalConnectionList(nameof(SyncSlot)))
                    {
                        Disconnect(nameof(SyncSlot), (Godot.Object)link["target"], nameof(_OnSyncShortcut));
                    }
                    allowCoolDown = false;
                    stacking = false;
                    itemStack.Clear();
                    stackSize = 0;
                    GetNode<Control>("count").Hide();
                    GetNode<Control>("m/label").Hide();
                    GetNode<Tween>("tween").StopAll();
                    GetNode<Control>("m/icon/overlay").SetScale(new Vector2(1.0f, 1.0f));
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
                    $"{((slotType == SlotType.SHORTCUT) ? 0 : 1)}.res";
                if (!GetNormalTexture().GetPath().Equals(texPath))
                {
                    SetNormalTexture((Texture)GD.Load(texPath));
                }
                GetNode<TextureRect>("m/icon").SetTexture(pickable.GetIcon());
                if (pickable.GetStackSize() > 0)
                {
                    stackSize = pickable.GetStackSize();
                    stacking = true;
                    if (itemStack.Count > 0 && !pickable.GetWorldName().Equals(itemStack[0].GetWorldName()))
                    {
                        itemStack.Clear();
                    }
                    itemStack.Add(pickable);
                    if (itemStack.Count > 1)
                    {
                        GetNode<Label>("count").SetText(itemStack.Count.ToString());
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
                    stacking = false;
                    stackSize = 0;
                    itemStack.Clear();
                    itemStack.Add(pickable);
                }
            }
        }
        public Pickable GetItem()
        {
            return (itemStack.Count > 0) ? itemStack[0] : null;
        }
        public void CoolDown(Pickable itm, float value, float seek)
        {
            if (GetItem() != null && itm != null && GetItem().GetWorldName().Equals(itm.GetWorldName()) &&
                !allowCoolDown && value > 0.0f && value != seek)
            {
                allowCoolDown = true;
                time = value;
                GetNode<Label>("m/label").SetText(Mathf.Round(value).ToString());
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
        public bool IsFull()
        {
            return itemStack.Count == stackSize;
        }
        public float GetCoolDownTimeLeft()
        {
            return GetNode<Tween>("tween").Tell();
        }
        public float GetCoolDownInitialTime()
        {
            return time;
        }
        public bool IsStacking()
        {
            return stacking;
        }
        public bool IsCoolingDown()
        {
            return GetCoolDownTimeLeft() > 0.0f;
        }
        public List<Pickable> GetItemStack()
        {
            return itemStack;
        }
    }
}