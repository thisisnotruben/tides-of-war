using Game.Actor;
using Game.Utils;
using Godot;
namespace Game.Ui
{
    public abstract class Menu : Control
    {
        public Control listOfMenus;
        public Control itemInfo;
        public AboutNode about;
        public Control merchant;
        public Control inventory;
        public Control dialogue;
        public Control menu;
        public Control questLog;
        public Control statsMenu;
        public Popup popup;
        public SaveLoadNode saveLoad;
        public ItemList inventoryBag;
        public ItemList merchantBag;
        public ItemList spellBook;
        public Speaker snd;
        public int selectedIdx;
        public Node selected;
        public Player player;
    }
}