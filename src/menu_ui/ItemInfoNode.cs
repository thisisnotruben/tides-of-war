using Godot;
using System.Linq;
using System.Collections.Generic;
using Game.Loot;
using Game.Database;
namespace Game.Ui
{
    public class ItemInfoNode : GameMenu
    {
        public ItemList itemList;
        private protected Popup popup;
        private protected string pickableWorldName;
        
        public override void _Ready()
        {
            popup = GetNode<Popup>("popup");
            popup.GetNode<BaseButton>("m/add_to_slot/clear_slot")
                .Connect("pressed", this, nameof(_OnClearSlotPressed));
            popup.GetNode<BaseButton>("m/add_to_slot/back")
                .Connect("pressed", this, nameof(_OnAddToHudBack));
            popup.Connect("hide", this, nameof(_OnItemInfoNodeHide));
            for (int i = 1; i <= 4; i++)
            {
                popup.GetNode($"m/add_to_slot/slot_{i}").Connect("pressed",
                    this, nameof(_OnAddToHudConfirm), new Godot.Collections.Array() {i});
            }
        }
        public virtual void Display(string pickableWorldName, bool allowMove)
        {
            this.pickableWorldName = pickableWorldName;
            TextureButton left = GetNode<TextureButton>("s/h/left");
            TextureButton right = GetNode<TextureButton>("s/h/right");
            if (allowMove && itemList != null)
            {
                int currentIdx = itemList.GetItemSlot(pickableWorldName).GetIndex();
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
            if (!player.dead && itemList != null && ItemDB.HasItem(pickableWorldName))
            {
                showBttns = new string[] {"drop", ""};
                string itemType = ItemDB.GetItemData(pickableWorldName).type;
                switch (itemType)
                {
                    case "FOOD":
                    case "POTION":
                        if (!itemList.IsSlotCoolingDown(itemList.GetItemSlot(pickableWorldName).GetIndex()))
                        {
                            showBttns[1] = "use";
                            GetNode<Label>("s/h/buttons/use/label").Text = 
                                (itemType.Equals("FOOD")) ? "Eat" : "Drink";
                        }
                        break;
                    case "ARMOR":
                    case "WEAPON":
                        showBttns[1] = "equip";
                        break;
                }
            }
            HideExcept(showBttns);
            GetNode<Label>("s/v/header").Text = pickableWorldName;
            GetNode<TextureRect>("s/v/c/v/add_to_hud/m/icon").Texture = PickableDB.GetIcon(pickableWorldName);
            GetNode<RichTextLabel>("s/v/c/v/m/info_text").BbcodeText = PickableDB.GetDescription(pickableWorldName);
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
            BaseButton yesBttn = popup.GetNode<BaseButton>("m/yes_no/yes");
            string signal = "pressed";
            foreach (Godot.Collections.Dictionary connectionPacket in yesBttn.GetSignalConnectionList(signal))
            {
                yesBttn.Disconnect(signal, this, (string)connectionPacket["method"]);
            }
            yesBttn.Connect(signal, this, toMethod);
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
            GetNode<Control>("s").Hide();
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
                if (!itemSlot.GetItem().Empty() && itemSlot.GetItem().Equals(pickableWorldName))
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
                if (!shortcut.GetItem().Empty() && shortcut.GetItem().Equals(pickableWorldName))
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
                    string weaponWorldName = (weapon == null) ? "" : player.weapon.worldName;
                    string armorWorldName = (armor == null) ? "" : player.vest.worldName;
                    if (weaponWorldName.Equals(pickableWorldName))
                    {
                        shortcut.SetItem(weaponWorldName, false, false, false);
                    }
                    else if (armorWorldName.Equals(pickableWorldName))
                    {
                        shortcut.SetItem(armorWorldName, false, false, false);
                    }
                    else
                    {
                        ItemSlot itemSlot = itemList.GetItemSlot(pickableWorldName);
                        List<string> pickableStack = itemSlot.GetItemStack();
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
                if (itemSlot.GetItem() != null && itemSlot.GetItem().Equals(pickableWorldName))
                {
                    itemSlot.SetItem(null, false, true, false);
                }
            }
            popup.Hide();
        }
        public void _OnInfoTextDraw()
        {
            Vector2 correctSize = GetNode<Control>("s/v/c/v/m").GetRect().Size;
            GetNode<RichTextLabel>("s/v/c/v/m/info_text").RectMinSize = correctSize;
        }
        public virtual void _OnMovePressed(int by)
        {
            Globals.PlaySound("click2", this, speaker);
            Display(itemList.GetItemMetaData(
                itemList.GetItemSlot(pickableWorldName).GetIndex() + by), true);
        }
    }
}
