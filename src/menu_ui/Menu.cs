using Godot;
using Game.Actor;

namespace Game.Ui
{
    public abstract class Menu : Control
    {
        public Control listOfMenus;
        public Control itemInfo;
        public Control about;
        public Control merchant;
        public Control inventory;
        public Control dialogue;
        public Control menu;
        public Control questLog;
        public Control statsMenu;
        public Popup popup;
        public SaveLoad saveLoad;
        public ItemList inventoryBag;
        public ItemList merchantBag;
        public ItemList spellBook;
        public AudioStreamPlayer snd;
        public int selectedIdx;
        public Node selected;
        public Player player;
    }
}