using Game.Ability;
using Game.Actor;
using Game.Database;
using Game.Ui;
using Godot;
namespace Game.Loot
{
    public abstract class Pickable : WorldObject
    {
        public WorldTypes subType;
        private float _duration;
        public float duration
        {
            get
            {
                return _duration;
            }
            set
            {
                _duration = value;
                if (value > 0.0f)
                {
                    GetNode<Timer>("timer").WaitTime = value;
                }
            }
        }

        [Signal]
        public delegate void DescribePickable(Pickable pickable, string PickableWorldDescription);
        [Signal]
        public delegate void SetInMenu(Pickable pickable, bool stack);

        public abstract void Init(string worldName);
        public abstract void _OnTimerTimeout();
        public void _OnTweenCompleted(Godot.Object obj, NodePath nodePath)
        {
            if (obj is Node2D)
            {
                Node2D node2dObj = (Node2D)obj;
                if (node2dObj.Scale != new Vector2(1.0f, 1.0f))
                {
                    Tween tween = GetNode<Tween>("tween");
                    tween.InterpolateProperty(node2dObj, nodePath, node2dObj.Scale, new Vector2(1.0f, 1.0f),
                        0.5f, Tween.TransitionType.Bounce, Tween.EaseType.Out);
                    tween.Start();
                }
            }
        }
        public virtual void GetPickable(Character character, bool addToBag)
        {
            Player player = character as Player;
            if (player != null && !IsConnected(nameof(DescribePickable), player.GetMenu(), nameof(InGameMenu._OnDescribePickable)))
            {
                Connect(nameof(DescribePickable), player.GetMenu(), nameof(InGameMenu._OnDescribePickable));
                Connect(nameof(SetInMenu), player.GetMenu(), nameof(InGameMenu._OnSetPickableInMenu));
                Item item = this as Item;
            }
            InGameMenu.Bags bag = (this is Item) ? InGameMenu.Bags.INVENTORY : InGameMenu.Bags.SPELL;
            if (Owner == Globals.map && addToBag)
            {
                PauseMode = PauseModeEnum.Process;
                EmitSignal(nameof(SetInMenu), this, PickableDB.GetStackSize(worldName) > 0, bag);
            }
            else
            {
                if (GetParent() != null)
                {
                    if (Owner == Globals.map)
                    {
                        Globals.map.SetGetPickableLoc(GlobalPosition, false);
                    }
                    GetParent().RemoveChild(this);
                }
                else
                {
                    GetNode("sight").SetBlockSignals(true);
                }
                if (this is Item)
                {
                    character.GetNode("inventory").AddChild(this);
                }
                else if (this is Spell)
                {
                    character.GetNode("spells").AddChild(this);
                }
                Owner = character;
                if (addToBag)
                {
                    EmitSignal(nameof(SetInMenu), this, PickableDB.GetStackSize(worldName), bag);
                }
            }
        }
        public virtual float GetTimeLeft()
        {
            return GetInitialTime() - GetNode<Timer>("timer").TimeLeft;
        }
        public float GetInitialTime()
        {
            return GetNode<Timer>("timer").WaitTime;
        }
        public bool Equals(Pickable pickable)
        {
            return worldName.Equals(pickable.worldName);
        }
        public virtual void UnMake()
        {
            EmitSignal(nameof(Unmake));
            QueueFree();
        }
        public void UncoupleSlot(ItemSlot itemSlot)
        {
            itemSlot.Disconnect("hide", this, nameof(UncoupleSlot));
            Disconnect(nameof(Unmake), itemSlot, nameof(ItemSlot.SetItem));
        }
    }
}