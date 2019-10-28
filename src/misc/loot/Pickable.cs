using Godot;
using Game.Actor;
using Game.Misc.Other;
using Game.Ui;
using Game.Map;
using System;


namespace Game.Misc.Loot
{
    public abstract class Pickable : WorldObject
    {
        private WorldTypes type;
        private WorldTypes subType;
        private protected ushort stackSize = 0;
        private protected float cooldown = 0.0f;
        private float duration = 0.0f;
        private short goldDrop;
        private protected short goldWorth;
        private protected short level;
        private protected string pickableDescription;
        private protected AtlasTexture icon;

        [Signal]
        public delegate void DescribePickable(Pickable pickable, string PickableWorldDescription);
        [Signal]
        public delegate void SetInMenu(Pickable pickable, short stackSize);

        [Signal]
        public delegate void Dropped(Pickable pickable);
        [Signal]
        public delegate void PickableExchanged(Pickable pickable, bool add);

        public override void _Ready()
        {
            Connect(nameof(PickableExchanged), Globals.GetWorldQuests(), "UpdateQuestItem");
            Make();
        }
        public virtual void _OnSightAreaEntered(Area2D area2D)
        {
            Character character = (Character)area2D.GetOwner();
            if (character != null && !character.IsDead() && character is Player)
            {
                Tween tween = GetNode<Tween>("tween");
                tween.InterpolateProperty(this, ":scale", GetScale(), new Vector2(1.05f, 1.05f),
                    0.5f, Tween.TransitionType.Bounce, Tween.EaseType.In);
                tween.Start();
                GetNode<Node2D>("select").Show();
                GetNode<AudioStreamPlayer2D>("snd").SetStream((AudioStreamSample)Globals.sndMeta["chest_open"]);
                AnimationPlayer anim = GetNode<AnimationPlayer>("anim");
                if (anim.IsPlaying() && anim.GetCurrentAnimation().Equals("close_chest"))
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
            Character character = (Character)area2D.GetOwner();
            if (character != null && !character.IsDead() && character is Player)
            {
                GetNode<AudioStreamPlayer2D>("snd").SetStream((AudioStreamSample)Globals.sndMeta["chest_open"]);
                GetNode<Node2D>("select").Hide();
                AnimationPlayer anim = GetNode<AnimationPlayer>("anim");
                if (anim.IsPlaying() && anim.GetCurrentAnimation().Equals("open_chest"))
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
                GetTree().SetPause(true);
                igm.popup.GetNode<Label>("m/error/label").SetText("Inventory\nFull!");
                igm.popup.GetNode<Control>("m/error").Show();
                igm.popup.Show();
                igm.GetNode<Control>("c/game_menu").Show();
            }
            else if (!anim.IsPlaying())
            {
                GetNode("select").SetBlockSignals(true);
                GetNode("sight").SetBlockSignals(true);
                EmitSignal(nameof(PickableExchanged), this, true);
                GetNode<AudioStreamPlayer2D>("snd").SetStream((AudioStreamSample)Globals.sndMeta["chest_collect"]);
                anim.Play("select");
                GetPickable(Globals.player, true);
                if (goldDrop > 0)
                {
                    CombatText combatText = (CombatText)Globals.combatText.Instance();
                    Globals.player.AddChild(combatText);
                    combatText.SetType($"+{goldDrop}", CombatText.TextType.GOLD,
                        Globals.player.GetNode<Node2D>("img").GetPosition());
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
                if (node2dObj.GetScale() != new Vector2(1.0f, 1.0f))
                {
                    Tween tween = GetNode<Tween>("tween");
                    tween.InterpolateProperty(node2dObj, nodePath, node2dObj.GetScale(), new Vector2(1.0f, 1.0f),
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
                SetModulate(new Color("#ffffff"));
            }
        }
        public void _OnSndFinished()
        {
            if (GetPauseMode() == PauseModeEnum.Process)
            {
                CallDeferred(nameof(GetPickable),
                   ((Node2D)(((Godot.Collections.Dictionary)GetSignalConnectionList(nameof(SetInMenu))[0])["target"])).GetOwner(), false);
                SetPauseMode(PauseModeEnum.Inherit);
            }
        }
        public abstract void _OnTimerTimeout();
        public void Describe()
        {
            EmitSignal(nameof(DescribePickable), this, pickableDescription);
        }
        public string GetPickableWorldDescription()
        {
            return pickableDescription;
        }
        public void SetUpShop(bool stack)
        {
            EmitSignal(nameof(SetInMenu), this, stack, "merchant");
        }
        public virtual void GetPickable(Character character, bool addToBag)
        {
            if (character is Player)
            {
                Player player = (Player)character;
                if (!IsConnected(nameof(DescribePickable), player.GetMenu(), nameof(InGameMenu._OnDescribePickable)))
                {
                    Connect(nameof(DescribePickable), player.GetMenu(), nameof(InGameMenu._OnDescribePickable));
                    Connect(nameof(Dropped), player.GetMenu(), nameof(InGameMenu._OnSetPickableInMenu));
                    if (this is Item)
                    {
                        ((Item)this).Connect(nameof(Item.EquipItem), player.GetMenu(), nameof(InGameMenu._OnEquipItem));
                        Connect(nameof(Dropped), player.GetMenu(), nameof(InGameMenu._OnDropPickable));
                    }
                }
            }
            if (GetOwner() == Globals.GetMap() && addToBag)
            {
                SetPauseMode(PauseModeEnum.Process);
                EmitSignal(nameof(SetInMenu), this, stackSize);
            }
            else
            {
                if (GetParent() != null)
                {
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
                else if (this is Spell.Spell)
                {
                    character.GetNode("spells").AddChild(this);
                }
                SetOwner(character);
                if (addToBag)
                {
                    EmitSignal(nameof(SetInMenu), this, stackSize);
                }
            }
        }
        public virtual float GetTimeLeft()
        {
            Timer timer = GetNode<Timer>("timer");
            return timer.GetWaitTime() - timer.GetTimeLeft();
        }
        public float GetInitialTime()
        {
            return GetNode<Timer>("timer").GetWaitTime();
        }
        public bool Equals(Pickable pickable)
        {
            if (GetWorldName() == pickable.GetWorldName())
            {
                return true;
            }
            return false;
        }
        public void Buy(Player buyer)
        {
            EmitSignal(nameof(PickableExchanged), this, true);
            PackedScene pickableScene = (PackedScene)GD.Load(GetFilename());
            Pickable pickable = (Pickable)pickableScene.Instance();
            pickable.SetWorldTypes(type, subType, level);
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
                GetNode<Timer>("timer").SetWaitTime(duration);
            }
        }
        public float GetDuration()
        {
            return duration;
        }
        public void SetWorldTypes(WorldTypes type, WorldTypes subType, short level)
        {
            this.type = type;
            this.subType = subType;
            this.level = level;
        }
        public void SetWorldTypes(string type, string subType, short level)
        {
            type = type.ToUpper();
            subType = subType.ToUpper();
            if (Enum.IsDefined(typeof(WorldTypes), type) && Enum.IsDefined(typeof(WorldTypes), subType))
            {
                SetWorldTypes((WorldTypes)Enum.Parse(typeof(WorldTypes), type), (WorldTypes)Enum.Parse(typeof(WorldTypes), subType), level);
            }
            else
            {
                GD.PrintErr($"{type} and {subType} for object ({GetPath()}) are not defined in-game.");
            }
        }
        public WorldTypes GetPickableSubType()
        {
            return subType;
        }
        public WorldTypes GetPickableType()
        {
            return type;
        }
        public void SetLevel(short level)
        {
            this.level = level;
        }
        public short GetLevel()
        {
            return level;
        }
        public abstract void Make();
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
            itemSlot.Disconnect(nameof(Hide), this, nameof(UncoupleSlot));
            Disconnect(nameof(Unmake), itemSlot, nameof(ItemSlot.SetItem));
        }
    }
}
