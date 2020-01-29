using Game.Ability;
using Game.Actor;
using Game.Database;
using Game.Misc.Other;
using Game.Ui;
using Godot;
namespace Game.Misc.Loot
{
    public abstract class Pickable : WorldObject
    {
        private WorldTypes subType;
        private short goldDrop;
        private float duration;
        private protected float cooldown;
        private protected byte stackSize;
        private protected short goldWorth;
        private protected short level;
        private protected string pickableDescription;
        private protected AtlasTexture icon;
        [Signal]
        public delegate void DescribePickable(Pickable pickable, string PickableWorldDescription);
        [Signal]
        public delegate void SetInMenu(Pickable pickable, bool stack);
        [Signal]
        public delegate void Dropped(Pickable pickable);
        [Signal]
        public delegate void PickableExchanged(Pickable pickable, bool add);
        public override void _Ready()
        {
            // Connect(nameof(PickableExchanged), Globals.GetWorldQuests(),
            // nameof(Game.Quests.WorldQuests.UpdateQuestPickable));
        }
        public abstract void Init(string worldName);
        public abstract void _OnTimerTimeout();
        public virtual void _OnSightAreaEntered(Area2D area2D)
        {
            Character character = area2D.Owner as  Character;
            if (character != null && !character.IsDead() && character is Player)
            {
                Tween tween = GetNode<Tween>("tween");
                tween.InterpolateProperty(this, ":scale", Scale, new Vector2(1.05f, 1.05f),
                    0.5f, Tween.TransitionType.Bounce, Tween.EaseType.In);
                tween.Start();
                GetNode<Node2D>("select").Show();
                GetNode<AudioStreamPlayer2D>("snd").Stream = (AudioStreamSample)Globals.sndMeta["chest_open"];
                AnimationPlayer anim = GetNode<AnimationPlayer>("anim");
                if (anim.IsPlaying() && anim.CurrentAnimation.Equals("close_chest"))
                {
                    anim.Queue("open_chest");
                }
                else
                {
                    anim.Play("open_chest");
                }
            }
        }
        public virtual void _OnSightAreaExited(Area2D area2D)
        {
            Character character = area2D.Owner as  Character;
            if (character != null && !character.IsDead() && character is Player)
            {
                GetNode<AudioStreamPlayer2D>("snd").Stream = (AudioStreamSample)Globals.sndMeta["chest_open"];
                GetNode<Node2D>("select").Hide();
                AnimationPlayer anim = GetNode<AnimationPlayer>("anim");
                if (anim.IsPlaying() && anim.CurrentAnimation.Equals("open_chest"))
                {
                    anim.Queue("close_chest");
                }
                else
                {
                    anim.Play("close_chest");
                }
            }
        }
        public void _OnSelectPressed()
        {
            AnimationPlayer anim = GetNode<AnimationPlayer>("anim");
            InGameMenu igm = Globals.player.GetMenu();
            if (this is Item && igm.inventoryBag.IsFull())
            {
                GetTree().Paused = true;
                igm.popup.GetNode<Label>("m/error/label").Text = "Inventory\nFull!";
                igm.popup.GetNode<Control>("m/error").Show();
                igm.popup.Show();
                igm.GetNode<Control>("c/game_menu").Show();
            }
            else if (!anim.IsPlaying())
            {
                GetNode("select").SetBlockSignals(true);
                GetNode("sight").SetBlockSignals(true);
                EmitSignal(nameof(PickableExchanged), this, true);
                GetNode<AudioStreamPlayer2D>("snd").Stream = (AudioStreamSample)Globals.sndMeta["chest_collect"];
                anim.Play("select");
                GetPickable(Globals.player, true);
                if (goldDrop > 0)
                {
                    CombatText combatText = (CombatText)Globals.combatText.Instance();
                    Globals.player.AddChild(combatText);
                    combatText.SetType($"+{goldDrop}", CombatText.TextType.GOLD,
                        Globals.player.GetNode<Node2D>("img").Position);
                    Globals.player.SetGold(goldDrop);
                    goldDrop = 0;
                }
            }
        }
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
        public void _OnAnimFinished(string animName)
        {
            if (animName.Equals("select"))
            {
                GetNode<Node2D>("img").Hide();
                Modulate = new Color("#ffffff");
            }
        }
        public void _OnSndFinished()
        {
            if (PauseMode == PauseModeEnum.Process)
            {
                CallDeferred(nameof(GetPickable),
                    ((Node)(((Godot.Collections.Dictionary)GetSignalConnectionList(nameof(SetInMenu))[0])["target"])).Owner, false);
                PauseMode = PauseModeEnum.Inherit;
            }
        }
        public void Describe()
        {
            EmitSignal(nameof(DescribePickable), this);
        }
        public string GetPickableWorldDescription()
        {
            return pickableDescription;
        }
        public void SetUpShop(bool stack)
        {
            EmitSignal(nameof(SetInMenu), this, stack, InGameMenu.Bags.MERCHANT);
        }
        public virtual void GetPickable(Character character, bool addToBag)
        {
            Player player = character as Player;
            if (player != null && !IsConnected(nameof(DescribePickable), player.GetMenu(), nameof(InGameMenu._OnDescribePickable)))
            {
                Connect(nameof(DescribePickable), player.GetMenu(), nameof(InGameMenu._OnDescribePickable));
                Connect(nameof(SetInMenu), player.GetMenu(), nameof(InGameMenu._OnSetPickableInMenu));
                Item item = this as Item;
                if (item != null)
                {
                    item.Connect(nameof(Item.EquipItem), player.GetMenu(), nameof(InGameMenu._OnEquipItem));
                    Connect(nameof(Dropped), player.GetMenu(), nameof(InGameMenu._OnDropPickable));
                }
            }
            InGameMenu.Bags bag = (this is Item) ? InGameMenu.Bags.INVENTORY : InGameMenu.Bags.SPELL;
            if (Owner == Globals.GetMap() && addToBag)
            {
                PauseMode = PauseModeEnum.Process;
                EmitSignal(nameof(SetInMenu), this, stackSize > 0, bag);
            }
            else
            {
                if (GetParent() != null)
                {
                    if (Owner == Globals.GetMap())
                    {
                        Globals.GetMap().SetGetPickableLoc(GlobalPosition, false);
                    }
                    GetParent().RemoveChild(this);
                }
                else
                {
                    GetNode("sight").SetBlockSignals(true);
                    GetNode("select").SetBlockSignals(true);
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
                    EmitSignal(nameof(SetInMenu), this, stackSize, bag);
                }
            }
        }
        public virtual float GetTimeLeft()
        {
            Timer timer = GetNode<Timer>("timer");
            return timer.WaitTime - timer.TimeLeft;
        }
        public float GetInitialTime()
        {
            return GetNode<Timer>("timer").WaitTime;
        }
        public bool Equals(Pickable pickable)
        {
            return GetWorldName().Equals(pickable.GetWorldName());
        }
        public void Buy(Player buyer)
        {
            EmitSignal(nameof(PickableExchanged), this, true);
            Pickable pickable;
            if (this is Item)
            {
                pickable = PickableFactory.GetMakeItem(GetWorldName());
            }
            else
            {
                pickable = PickableFactory.GetMakeSpell(GetWorldName());
            }
            pickable.GetPickable(buyer, true);
            buyer.SetGold((short)(-pickable.GetGold()));
        }
        public void Sell(Player seller)
        {
            EmitSignal(nameof(PickableExchanged), this, false);
            seller.SetGold(GetGold());
            UnMake();
        }
        public void Drop()
        {
            EmitSignal(nameof(PickableExchanged), this, false);
            EmitSignal(nameof(Dropped), this);
            GetNode("sight").SetBlockSignals(false);
            GetNode("select").SetBlockSignals(false);
            GetNode<Node2D>("img").Show();
        }
        public virtual void UnMake()
        {
            EmitSignal(nameof(Unmake));
            QueueFree();
        }
        public void SetDuration(float duration)
        {
            this.duration = duration;
            if (duration > 0.0f)
            {
                GetNode<Timer>("timer").WaitTime = duration;
            }
        }
        public float GetDuration()
        {
            return duration;
        }
        public void SetPickableSubType(WorldTypes subType)
        {
            this.subType = subType;
        }
        public WorldTypes GetPickableSubType()
        {
            return subType;
        }
        public short GetLevel()
        {
            return level;
        }
        public short GetGold()
        {
            return goldWorth;
        }
        public ushort GetStackSize()
        {
            return stackSize;
        }
        public AtlasTexture GetIcon()
        {
            return icon;
        }
        public void UncoupleSlot(ItemSlot itemSlot)
        {
            itemSlot.Disconnect("hide", this, nameof(UncoupleSlot));
            Disconnect(nameof(Unmake), itemSlot, nameof(ItemSlot.SetItem));
        }
    }
}