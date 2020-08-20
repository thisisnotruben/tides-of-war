using System.Collections.Generic;
using System;
using Game.Actor.Doodads;
using Game.Ui;
using Game.Utils;
using Game.Loot;
using Godot;
namespace Game.Actor
{
    public class Player : Character
    {
        public static Player player;
        private static readonly PackedScene moveCursorScene = (PackedScene)GD.Load("res://src/menu_ui/views/move_cursor.tscn");
        private static readonly PackedScene graveScene = (PackedScene)GD.Load("res://src/character/doodads/grave.tscn");
        private List<Vector2> reservedPath;
        public Vector2 gravePos { get; private set; }
        public int xp { get; private set; }
        public int gold;
        private Item _weapon;
        public Item weapon
        {
            set
            {
                if (value == null && weapon != null)
                {
                    Tuple<int, int> values = weapon.GetValues();
                    minDamage -= values.Item1;
                    maxDamage -= values.Item2;
                } 
                if (value != null)
                {
                    Tuple<int, int> values = value.GetValues();
                    minDamage += values.Item1;
                    maxDamage += values.Item2;
                }
                _weapon = value;
            }
            get
            {
                return _weapon;
            }
        }
        private Item _vest;
        public Item vest
        {
            set
            {
                if (value == null && vest != null)
                {
                    armor -= vest.value;
                } 
                if (value != null)
                {
                    armor += vest.value;
                }
                _vest = value;
            }
            get
            {
                return _vest;
            }
        }

        [Signal]
        public delegate void PosChanged();

        public override void init()
        {
            worldType = WorldTypes.PLAYER;
            reservedPath = new List<Vector2>();
            gravePos = new Vector2();
            xp = 0;
            gold = 0;
            SetImg("human-6");
            SetAttributes();
            hp = hpMax;
            mana = manaMax;
            weapon = null;
            vest = null;
            gold = 10000;
            level = 10;
        }
        public Player()
        {
            player = this;
        }
        public override void _Ready()
        {
            GameMenu.player = this;
            base.init();
            base._Ready();
            init();
        }
        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventScreenTouch || Input.IsActionJustPressed("ui_click"))
            {
                if (!@event.IsPressed() || @event.IsEcho())
                {
                    return;
                }
                Vector2 eventGlobalPosition = Map.Map.map.GetGridPosition(GetGlobalMousePosition());
                if (Map.Map.map.IsValidMove(eventGlobalPosition))
                {
                    Tween tween = GetNode<Tween>("tween");
                    if (state == States.MOVING && path.Count > 0 && tween.IsActive())
                    {
                        Map.Map.map.ResetPath(reservedPath);
                        reservedPath.Clear();
                        List<Vector2> _path = Map.Map.map.getAPath(
                            Map.Map.map.GetGridPosition(GlobalPosition), eventGlobalPosition);
                        if (_path[0] != path[0])
                        {
                            tween.Remove(this, "global_position");
                        }
                        else
                        {
                            _path.RemoveAt(0);
                        }
                        path = _path;
                        SetProcess(true);
                    }
                    else
                    {
                        path = Map.Map.map.getAPath(Map.Map.map.GetGridPosition(GlobalPosition), eventGlobalPosition);
                    }
                    EmitSignal(nameof(PosChanged));
                    MoveCursor cursor = (MoveCursor)moveCursorScene.Instance();
                    cursor.AddToMap(this, eventGlobalPosition);
                }
            }
        }
        public override void _Process(float delta)
        {
            if (path.Count > 0)
            {
                SetState(States.MOVING);
                MoveTo(path[0], path);
            }
            else if (target != null && target.enemy && GetCenterPos().DistanceTo(target.GetCenterPos()) <= weaponRange)
            {
                SetState(States.ATTACKING);
            }
            else
            {
                SetState(States.IDLE);
            }
        }
        public void _OnAnimFinished(string animName)
        {
            if (animName.Equals("attacking") && spell != null)
            {
                weaponRange = (ranged) ? Stats.WEAPON_RANGE_RANGE : Stats.WEAPON_RANGE_MELEE;
            }
            else if (animName.Equals("casting"))
            {
                SetProcess(true);
            }
        }
        public override void _OnSelectPressed() { }
        public override void MoveTo(Vector2 worldPosition, List<Vector2> route)
        {
            Vector2 direction = GetDirection(GlobalPosition, worldPosition);
            if (!direction.Equals(new Vector2()))
            {
                RayCast2D ray = GetNode<RayCast2D>("ray");
                worldPosition = Map.Map.map.RequestMove(GlobalPosition, direction);
                ray.LookAt(worldPosition);
                if (!worldPosition.Equals(new Vector2()))
                {
                    reservedPath.Add(worldPosition);
                    Move(worldPosition, Stats.MapAnimMoveSpeed(animSpeed));
                    route.RemoveAt(0);
                }
                else if (ray.IsColliding())
                {
                    route.Clear();
                }
                else if (route.Count > 0)
                {
                    route.RemoveAt(0);
                }
            }
            else if (route.Count > 0)
            {
                route.RemoveAt(0);
            }
        }
        public override void Attack(bool ignoreArmor = false)
        {
            base.Attack(ignoreArmor);
            if (weapon != null)
            {
                weapon.TakeDamage(dead);
            }
        }
        public override void TakeDamage(int damage, bool ignoreArmor, WorldObject worldObject, CombatText.TextType textType)
        {
            base.TakeDamage(damage, ignoreArmor, worldObject, textType);
            if (vest != null)
            {
                vest.TakeDamage(dead);
            }
            if (dead && weapon != null)
            {
                weapon.TakeDamage(dead);
            }
        }
        public override void SetState(States state, bool overrule = false)
        {
            if (this.state != state || overrule)
            {
                AnimationPlayer anim = GetNode<AnimationPlayer>("anim");
                Sprite img = GetNode<Sprite>("img");
                switch (state)
                {
                    case States.IDLE:
                        anim.Stop();
                        img.Frame = 0;
                        img.FlipH = false;
                        SetTime(regenTime);
                        engaging = false;
                        break;
                    case States.MOVING:
                        img.FlipH = false;
                        anim.Play("moving", -1, animSpeed);
                        anim.Seek(0.3f, true);
                        break;
                    case States.ATTACKING:
                        SetState(States.IDLE);
                        SetTime(weaponSpeed, true);
                        engaging = true;
                        break;
                    case States.DEAD:
                        SetDead(true);
                        break;
                    case States.ALIVE:
                        SetDead(false);
                        break;
                }
                base.SetState(state);
            }
        }
        public void SetXP(int addedXP, bool showLabel = true, bool fromSaveFile = false)
        {
            xp += addedXP;
            if (xp > Stats.MAX_XP)
            {
                xp = Stats.MAX_XP;
            }
            else if (xp > 0 && xp < Stats.MAX_XP && showLabel)
            {
                CombatText combatText = (CombatText)Globals.combatText.Instance();
                AddChild(combatText);
                combatText.SetType($"+{xp}", CombatText.TextType.XP, GetNode<Node2D>("img").Position);
            }
            int _level = Stats.CheckLevel(xp);
            if (level != _level && level < Stats.MAX_LEVEL)
            {
                level = _level;
                if (!fromSaveFile)
                {
                    Globals.PlaySound("level_up", this, new Speaker());
                }
                if (level > Stats.MAX_LEVEL)
                {
                    level = Stats.MAX_LEVEL;
                }
                SetAttributes();
            }
        }
        public override async void SetDead(bool dead)
        {
            if (this.dead == dead)
            {
                return;
            }
            base.SetDead(dead);
            if (dead)
            {
                await ToSignal(GetNode<AnimationPlayer>("anim"), "animation_finished");
                Grave grave = (Grave)graveScene.Instance();
                Map.Map.map.AddZChild(grave);
                grave.SetDeceasedPlayer(this);
                grave.GlobalPosition = GlobalPosition;
                gravePos = grave.GlobalPosition;
                Map.Map.map.SetVeil(true);
                path.Clear();
                Dictionary<float, Vector2> graveSites = new Dictionary<float, Vector2>();
                List<float> graveDist = new List<float>();
                foreach (Node2D graveyard in GetTree().GetNodesInGroup("gravesite"))
                {
                    float graveDistanceToPlayer = GlobalPosition.DistanceTo(graveyard.GlobalPosition);
                    graveSites.Add(graveDistanceToPlayer, graveyard.GlobalPosition);
                    graveDist.Add(graveDistanceToPlayer);
                }
                float minVal = graveDist[0];
                for (int i = 1; i < graveSites.Count; i++)
                {
                    minVal = Mathf.Min(minVal, graveDist[i]);
                }
                GlobalPosition = Map.Map.map.GetGridPosition(graveSites[minVal]);
                SetProcessUnhandledInput(true);
                SetProcess(true);
            }
            else
            {
                GD.Randomize();
                hp = (int)(hpMax * GD.RandRange(Stats.HP_MANA_RESPAWN_MIN_LIMIT, 1.0));
                mana = (int)(manaMax * GD.RandRange(Stats.HP_MANA_RESPAWN_MIN_LIMIT, 1.0));
                gravePos = new Vector2();
            }
        }
        public MenuHandler GetMenu()
        {
            return GetNode<MenuHandler>("in_game_menu");
        }
    }
}