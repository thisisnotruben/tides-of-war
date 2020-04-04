using Godot;
using System.Linq;
using System.Collections.Generic;
using Game.Actor;
using Game.Loot;
using Game.Utils;
using Game.Database;
namespace Game.Ui
{
    public class ItemInfoNode : Control
    {
        public Player player;
        private Speaker _speaker;
        public Speaker speaker
        {
            set
            {
                _speaker = value;
                popup.speaker = value;
            }
            get
            {
                return _speaker;
            }
        }
        public ItemList itemList;
        private protected Popup popup;
        private protected Pickable pickable;
        
        public override void _Ready()
        {
            popup = GetNode<Popup>("popup");
            popup.GetNode<BaseButton>("m/add_to_slot/clear_slot")
                .Connect("pressed", this, nameof(_OnClearSlotPressed));
            popup.GetNode<BaseButton>("m/add_to_slot/back")
                .Connect("pressed", this, nameof(_OnAddToHudBack));
            for (int i = 1; i <= 4; i++)
            {
                popup.GetNode($"m/add_to_slot/slot_{i}").Connect("pressed",
                    this, nameof(_OnAddToHudConfirm), new Godot.Collections.Array() {i});
            }
        }
        public virtual void Display(Pickable pickable, bool allowMove)
        {
            this.pickable = pickable;
            TextureButton left = GetNode<TextureButton>("s/h/left");
            TextureButton right = GetNode<TextureButton>("s/h/right");
            if (allowMove && itemList != null)
            {
                int currentIdx = itemList.GetItemSlot(pickable).GetIndex();
                left.Disabled = currentIdx == 0;
                right.Disabled = currentIdx == itemList.GetItemCount() - 1;
                left.Show();
                right.Show();
            }
            else
            {
                left.Hide();
                right.Hide();
            }
            string[] showBttns = {};
            if (!player.dead && itemList != null)
            {
                showBttns = new string[] {"drop", ""};
                switch (pickable.worldType)
                {
                    case WorldObject.WorldTypes.FOOD:
                    case WorldObject.WorldTypes.POTION:
                        if (!itemList.IsSlotCoolingDown(itemList.GetItemSlot(pickable).GetIndex()))
                        {
                            showBttns[1] = "use";
                            GetNode<Label>("s/h/v/use/label").Text = 
                                (pickable.worldType == WorldObject.WorldTypes.FOOD) ? "Eat" : "Drink";
                        }
                        break;
                    case WorldObject.WorldTypes.ARMOR:
                    case WorldObject.WorldTypes.WEAPON:
                        showBttns[1] = "equip";
                        break;
                }
            }
            HideExcept(showBttns);
            ItemDB.ItemNode itemNode = ItemDB.GetItemData(pickable.worldName);
            GetNode<Label>("s/v/header").Text = pickable.worldName;
            GetNode<TextureRect>("s/v/c/v/add_to_hud/m/icon").Texture = itemNode.icon;
            GetNode<RichTextLabel>("s/v/c/v/m/info_text").BbcodeText = itemNode.description;
            Show();
        }
        private protected void HideExcept(params string[] nodesToShow)
        {
            foreach (Control node in GetNode("s/h/buttons").GetChildren())
            {
                if (!nodesToShow.Contains(node.Name) && !node.Name.Equals("back"))
                {
                    node.Hide();
                }
                else
                {
                    node.Show();
                }
            }
        }
        public void RouteConnections(string toMethod)
        {
            // TODO: need to clear pervious signal list before connecting
            popup.GetNode("m/yes_no/yes").Connect("pressed", this, toMethod);
        }
        public void _OnItemInfoNodeHide()
        {
            popup.Hide();
            GetNode<Control>("s").Show();
        }
        public void _OnSlotMoved(string nodePath, bool down)
        {
            float scale = (down) ? 0.8f : 1.0f;
            GetNode<Control>(nodePath).RectScale = new Vector2(scale, scale);
        }
        public void _OnAddToHudPressed()
        {
            Globals.PlaySound("click2", this, speaker);
            int count = 1;
            popup.GetNode<Control>("m/add_to_slot/clear_slot").Hide();
            foreach (ItemSlot itemSlot in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
            {
                Tween tween = itemSlot.GetNode<Tween>("tween");
                ColorRect colorRect = itemSlot.GetNode<ColorRect>("m/icon/overlay");
                Label label = itemSlot.GetNode<Label>("m/label");
                if (tween.IsActive())
                {
                    tween.SetActive(false);
                    colorRect.RectScale = new Vector2(1.0f, 1.0f);
                }
                colorRect.Color = new Color(1.0f, 1.0f, 0.0f, 0.75f);
                label.Text = count.ToString();
                label.Show();
                if (itemSlot.GetItem() != null && itemSlot.GetItem().Equals(pickable))
                {
                    popup.GetNode<Control>("m/add_to_slot/clear_slot").Show();
                }
                count++;
            }
            popup.GetNode<Control>("m/add_to_slot").Show();
            popup.Show();
        }
        public void _OnAddToHudConfirm(int index)
        {
            int amounttt = -1;
            ItemSlot buttonFrom = null;
            ItemSlot buttonTo = null;
            Hide();
            Globals.PlaySound("click1", this, speaker);
            foreach (ItemSlot shortcut in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
            {
                Tween itemSlotTween = shortcut.GetNode<Tween>("tween");
                shortcut.GetNode<ColorRect>("m/icon/overlay").Color = new Color(0.0f, 0.0f, 0.0f, 0.75f);
                shortcut.GetNode<Control>("m/label").Hide();
                itemSlotTween.SetActive(true);
                itemSlotTween.ResumeAll();
                if (shortcut.GetItem() != null && shortcut.GetItem().Equals(pickable))
                {
                    amounttt = shortcut.GetItemStack().Count;
                    shortcut.SetItem(null, false, true, false);
                }
                if (shortcut.Name.Equals(index.ToString()))
                {
                    buttonTo = shortcut;
                    if (shortcut.GetItem() != null)
                    {
                        shortcut.SetItem(null, false, true, false);
                    }
                    Item weapon = player.weapon;
                    Item armor = player.vest;
                    if (weapon == pickable)
                    {
                        shortcut.SetItem(weapon, false, false, false);
                    }
                    else if (armor == pickable)
                    {
                        shortcut.SetItem(armor, false, false, false);
                    }
                    else
                    {
                        ItemSlot itemSlot = itemList.GetItemSlot(pickable);
                        List<Pickable> pickableStack = itemSlot.GetItemStack();
                        buttonFrom = itemSlot;
                        if (amounttt == -1)
                        {
                            amounttt = pickableStack.Count;
                        }
                        for (int i = 0; i < amounttt; i++)
                        {
                            shortcut.SetItem(pickableStack[i]);
                        }
                    }
                }
            }
            if (buttonFrom != null && buttonTo != null)
            {
                foreach (Godot.Collections.Dictionary link in buttonFrom.GetSignalConnectionList(nameof(ItemSlot.SyncSlot)))
                {
                    buttonFrom.Disconnect(nameof(ItemSlot.SyncSlot), (Godot.Object)link["target"], nameof(ItemSlot._OnSyncShortcut));
                }
                buttonFrom.Connect(nameof(ItemSlot.SyncSlot), buttonTo, nameof(ItemSlot._OnSyncShortcut));
                if (buttonFrom.IsCoolingDown())
                {
                    buttonTo.CoolDown(buttonFrom.GetItem(), buttonFrom.GetCoolDownInitialTime(), buttonFrom.GetCoolDownTimeLeft());
                }
            }
        }
        public void _OnAddToHudBack()
        {
            foreach (ItemSlot itemSlot in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
            {
                Tween tween = itemSlot.GetNode<Tween>("tween");
                tween.SetActive(true);
                tween.ResumeAll();
                itemSlot.GetNode<ColorRect>("m/icon/overlay").Color = new Color(0.0f, 0.0f, 0.0f, 0.75f);
                itemSlot.GetNode<Control>("m/label").Hide();
            }
        }
        public void _OnClearSlotPressed()
        {
            foreach (ItemSlot itemSlot in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
            {
                if (itemSlot.GetItem() != null && itemSlot.GetItem().Equals(pickable))
                {
                    itemSlot.SetItem(null, false, true, false);
                }
            }
            _OnItemInfoNodeHide();
        }
        public void _OnInfoTextDraw()
        {
            Vector2 correctSize = GetNode<Control>("s/v/c/v/m").GetRect().Size;
            GetNode<RichTextLabel>("s/v/c/v/m/info_text").RectMinSize = correctSize;
        }
        public void _OnBackPressed()
        {
            Globals.PlaySound("click3", this, speaker);
            Hide();
        }
        // TODO: need to sift
    }
}
