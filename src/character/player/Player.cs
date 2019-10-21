using Game.Ui;
using Godot;
using System.Collections.Generic;
using Game.Misc.Other;

namespace Game.Actor
{
    public class Player : Character
    {
        private static readonly PackedScene moveCursor = (PackedScene)GD.Load("res://src/menu_ui/move_cursor.tscn");
        private List<Vector2> reservedPath = new List<Vector2>();
        private Vector2 gravePos;
        public short xp;
        private short gold;
        [Signal]
        public delegate void PosChanged();

        public override void _Ready()
        {
            Globals.player = this;
            SetImg("res://asset/img/character/goblin/bow-null-16.png");
            EmitSignal(nameof(UpdateHud), new Godot.Collections.Array() { "hp", GetWorldName(), hp, hpMax });
            EmitSignal(nameof(UpdateHud), new Godot.Collections.Array() { "mana", GetWorldName(), mana, manaMax });
        }
        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventScreenTouch || Input.IsActionJustPressed("ui_click"))
            {
                if (!@event.IsPressed() || @event.IsEcho())
                {
                    return;
                }
                Vector2 eventGlobalPosition = Globals.GetMap().GetGridPosition(GetGlobalMousePosition());
                if (Globals.GetMap().IsValidMove(eventGlobalPosition))
                {
                    if (GetState() == States.MOVING && path.Count > 0 && GetNode<Tween>("tween").IsActive())
                    {
                        List<Vector2> _path = Globals.GetMap().getAPath(
                            Globals.GetMap().GetGridPosition(GetGlobalPosition()), eventGlobalPosition);
                        if (_path[0] != path[0])
                        {
                            GetNode<Tween>("tween").Remove(this, "global_position");
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
                        path = Globals.GetMap().getAPath(Globals.GetMap().GetGridPosition(GetGlobalPosition()), eventGlobalPosition);
                    }
                    EmitSignal(nameof(PosChanged));
                    MoveCursor cursor = (MoveCursor)moveCursor.Instance();
                    cursor.AddToMap(this, eventGlobalPosition);
                }
            }
        }
        public override void _Process(float delta)
        {
            if (path.Count > 0)
            {
                SetState(States.MOVING);
                MoveTo(path[0]);
                return;
            }
            else if (target != null && target.IsEnemy(this) && GetCenterPos().DistanceTo(target.GetCenterPos()) <= weaponRange)
            {
                SetState(States.ATTACKING);
                return;
            }
            SetState(States.IDLE);
        }
        public void _OnAnimFinished(string animName)
        {
            if (animName.Equals("attacking") && spell != null)
            {
                switch (weaponType)
                {
                    case WorldTypes.BOW:
                    case WorldTypes.MAGIC:
                        weaponRange = Stats.WEAPON_RANGE_RANGE;
                        break;
                    default:
                        weaponRange = Stats.WEAPON_RANGE_MELEE;
                        break;
                }
            }
            else if (animName.Equals("cast"))
            {
                SetProcess(true);
            }
        }
        public override void _OnSelectPressed()
        {
        }
        public override void MoveTo(Vector2 worldPosition)
        {
            Vector2 direction = GetDirection(worldPosition);
            GD.PrintT("direction:", direction);
            if (!direction.Equals(new Vector2()))
            {
                RayCast2D ray = GetNode<RayCast2D>("ray");
                worldPosition = Globals.GetMap().RequestMove(GetGlobalPosition(), direction);
                ray.LookAt(worldPosition);
                GD.Print("world position: ", worldPosition);
                if (!worldPosition.Equals(new Vector2()))
                {
                    GD.Print(Stats.MapAnimMoveSpeed(animSpeed));
                    reservedPath.Add(worldPosition);
                    Move(worldPosition, Stats.MapAnimMoveSpeed(animSpeed));
                    path.RemoveAt(0);
                }
                else if (ray.IsColliding())
                {
                    path.Clear();
                }
                else if (path.Count > 0)
                {
                    path.RemoveAt(0);
                }
            }
            else
            {
                path.RemoveAt(0);
            }
        }
        public override void Attack(bool ignoreArmor, Dictionary<string, Dictionary<string, ushort>> attackTable = null)
        {
            base.Attack(ignoreArmor, attackTable);
            if (weapon != null)
            {
                weapon.TakeDamage(dead);
            }
        }
        public override void TakeDamage(short damage, bool ignoreArmor, WorldObject worldObject, CombatText.TextType textType)
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
        public override void SetState(States state)
        {
            if (GetState() == state)
            {
                return;
            }
            AnimationPlayer anim = GetNode<AnimationPlayer>("anim");
            Sprite img = GetNode<Sprite>("img");
            switch (state)
            {
                case States.IDLE:
                    anim.Stop();
                    img.SetFrame(0);
                    img.SetFlipH(false);
                    SetTime(regenTime);
                    engaging = false;
                    break;
                case States.MOVING:
                    img.SetFlipH(false);
                    anim.Play("moving", -1, animSpeed);
                    anim.Seek(0.3f, true);
                    break;
                case States.ATTACKING:
                    SetState(States.IDLE);
                    SetTime(weaponSpeed, true);
                    engaging = false;
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
        public override void SetTarget(Character character)
        {
            if (target != null)
            {
                target.Disconnect(nameof(UpdateHud), GetMenu(), nameof(Character.UpdateHUD));
                if (target is Npc)
                {
                    Npc npc = (Npc)target;
                    switch (npc.GetWorldType())
                    {
                        case WorldTypes.TRAINER:
                            npc.SetUpShop(GetMenu(), false, GetWorldType());
                            break;
                        case WorldTypes.MERCHANT:
                            npc.SetUpShop(GetMenu(), false, GetWorldType());
                            break;
                    }
                }
            }
            if (character != null)
            {
                GetMenu().hpMana.GetNode<Control>("m/h/u").Show();
                character.Connect(nameof(UpdateHud), GetMenu(), nameof(Character.UpdateHUD));
                character.UpdateHUD();
                if (character is Npc)
                {
                    Npc npc = (Npc)character;
                    switch (npc.GetWorldType())
                    {
                        case WorldTypes.TRAINER:
                            npc.SetUpShop(GetMenu(), false, GetWorldType());
                            break;
                        case WorldTypes.MERCHANT:
                            npc.SetUpShop(GetMenu(), false, GetWorldType());
                            break;
                    }
                }
                else
                {
                    GetMenu().hpMana.GetNode<Control>("m/h/u").Hide();
                }
                base.SetTarget(character);
            }
        }
        public void SetXP(short addedXP, bool showLabel = true, bool fromSaveFile = false)
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
                combatText.SetType(string.Format("+{0:n0}", xp), (short)CombatText.TextType.XP, GetCenterPos());
            }
            short _level = Stats.CheckLevel(xp);
            if (GetLevel() != _level && GetLevel() < Stats.MAX_LEVEL)
            {
                SetLevel(_level);
                if (!fromSaveFile)
                {
                    Globals.PlaySound("level_up", this, new AudioStreamPlayer());
                }
                if (GetLevel() > Stats.MAX_LEVEL)
                {
                    SetLevel(Stats.MAX_LEVEL);
                }
                Dictionary<string, double> stats = Stats.UnitMake((double)GetLevel(),
                    Stats.GetMultiplier(false, GetNode<Sprite>("img").GetTexture().GetPath()));
                foreach (string attribute in stats.Keys)
                {
                    Set(attribute, (short)stats[attribute]);
                }
            }
        }
        public void SetGold(short gold)
        {
            this.gold += gold;
        }
        public short GetGold()
        {
            return gold;
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
                Grave grave = (Grave)Globals.grave.Instance();
                AddChild(grave);
                grave.SetDeceasedPlayer(this);
                gravePos = grave.GetGlobalPosition();
                Globals.GetMap().SetVeil(true);
                path.Clear();
                Dictionary<float, Vector2> graveSites = new Dictionary<float, Vector2>();
                List<float> graveDist = new List<float>();
                foreach (Node2D graveyard in GetTree().GetNodesInGroup("gravesite"))
                {
                    float graveDistanceToPlayer = GetGlobalPosition().DistanceTo(graveyard.GetGlobalPosition());
                    graveSites.Add(graveDistanceToPlayer, graveyard.GetGlobalPosition());
                    graveDist.Add(graveDistanceToPlayer);
                }
                float minVal = graveDist[0];
                for (int i = 1; i < graveSites.Count; i++)
                {
                    minVal = Mathf.Min(minVal, graveDist[i]);
                }

                SetGlobalPosition(Globals.GetMap().GetGridPosition(graveSites[minVal]));
                SetProcessUnhandledInput(true);
                SetProcess(true);
            }
            else
            {
                GD.Randomize();
                SetHp((short)(hpMax * GD.RandRange(Stats.HP_MANA_RESPAWN_MIN_LIMIT, 1.0)));
                SetMana((short)(manaMax * GD.RandRange(Stats.HP_MANA_RESPAWN_MIN_LIMIT, 1.0)));
                gravePos = new Vector2();
            }
        }
        public InGameMenu GetMenu()
        {
            return GetNode<InGameMenu>("igm");
        }
        public Vector2 GetGravePos()
        {
            return gravePos;
        }
    }
}