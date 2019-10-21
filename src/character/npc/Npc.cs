using Godot;
using Game.Misc.Loot;
using System.Collections.Generic;
using Game.Ui;
using Game.Misc.Other;

namespace Game.Actor
{
    public class Npc : Character
    {
        private bool enemy = true;
        private bool patroller;
        private string text;
        private List<Vector2> patrolPath = new List<Vector2>();

        [Signal]
        public delegate void DropLoot(Npc npc, Vector2 worldPosition, int idk);

        public override void _Ready()
        {
            SetProcess(false);
        }
        public override void _Process(float delta)
        {
            if (engaging && target != null)
            {
                if (origin.DistanceTo(target.GetCenterPos()) > Stats.FLEE_DISTANCE || target.IsDead())
                {
                    SetState(States.RETURNING);
                    if (patroller)
                    {
                        if (GetGlobalPosition() != patrolPath[-1])
                        {
                            MoveTo(patrolPath[0]);
                            return;
                        }
                    }
                    else if (GetGlobalPosition() != origin)
                    {
                        MoveTo(origin);
                        return;
                    }
                    SetState(States.IDLE);
                }
                else if (!GetNode<AnimationPlayer>("anim").HasAnimation("attackig"))
                {
                    // flee code here
                }
                else if (GetCenterPos().DistanceTo(target.GetCenterPos()) > weaponRange)
                {
                    SetState(States.MOVING);
                    MoveTo(target.GetGlobalPosition());
                }
                else
                {
                    SetState(States.ATTACKING);
                }
            }
            else if (patroller)
            {
                SetState(States.MOVING);
                MoveTo(patrolPath[0]);
            }
            else
            {
                SetState(States.IDLE);
            }
        }
        public void _OnSightAreaEntered(Area2D area2D)
        {
            Character character = (Character)area2D.GetOwner();
            if (!dead && (target == null || character.IsDead()))
            {
                if (!enemy && character is Player)
                {
                    // friendly npcs' don't attack player
                    return;
                }
                else if (!engaging && enemy != character.IsEnemy(this))
                {
                    SetTarget(character);
                    engaging = true;
                    SetProcess(true);
                }
            }
        }
        public void _OnSightAreaExited(Area2D area2D)
        {
            if (!engaging)
            {
                SetTarget(null);
            }
        }
        public void _OnAreaMouseEntered()
        {
            Globals.player.SetProcessUnhandledInput(false);
        }
        public void _OnAreaMouseExited()
        {
            Globals.player.SetProcessUnhandledInput(true);
        }
        public override void _OnSelectPressed()
        {
            Player player = Globals.player;
            InGameMenu menu = player.GetMenu();
            if (player.GetTarget() == this)
            {
                player.SetTarget(null);
            }
            else if (!player.IsDead())
            {
                player.SetTarget(this);
                Sprite img = GetNode<Sprite>("img");
                Tween tween = GetNode<Tween>("tween");
                tween.InterpolateProperty(img, ":scale", img.GetScale(), new Vector2(1.03f, 1.03f),
                    0.5f, Tween.TransitionType.Elastic, Tween.EaseType.Out);
                tween.Start();
                if (!enemy && !engaging && GetNode<Area2D>("sight").OverlapsArea(Globals.player.GetNode<Area2D>("area")))
                {
                    switch (GetWorldType())
                    {
                        case WorldTypes.MERCHANT:
                        case WorldTypes.TRAINER:
                            if (GetWorldType() == WorldTypes.MERCHANT)
                            {
                                Globals.PlaySound("merchant_open", this, new AudioStreamPlayer());
                                menu.merchant.GetNode<Control>("s/v2/inventory").Show();
                                foreach (Pickable pickable in GetNode("inventory").GetChildren())
                                {
                                    pickable.SetUpShop(false);
                                }
                            }
                            else
                            {
                                Globals.PlaySound("turn_page", this, new AudioStreamPlayer());
                                menu.merchant.GetNode<Control>("s/v2/inventory").Hide();
                                foreach (Pickable pickable in GetNode("spells").GetChildren())
                                {
                                    pickable.SetUpShop(false);
                                }
                            }
                            tween.SetPauseMode(PauseModeEnum.Process);
                            menu.itemInfo.GetNode<TextureButton>("s/v/c/v/bg").SetDisabled(true);
                            menu.merchant.GetNode<Label>("s/v/label").SetText(GetWorldName());
                            menu.merchant.GetNode<Label>("s/v/label2").SetText(string.Format("Gold: {0:n0}", player.GetGold()));
                            menu.menu.Hide();
                            menu.merchant.Show();
                            menu.GetNode<Control>("c/game_menu").Show();
                            break;
                        default:
                            EmitSignal(nameof(Talked));
                            if (text.Empty())
                            {
                                return;
                            }
                            Globals.PlaySound("turn_page", this, new AudioStreamPlayer());
                            menu.menu.Hide();
                            if (GetWorldType() == WorldTypes.HEALER)
                            {
                                menu.dialogue.GetNode<Control>("s/s/v/heal").Show();
                            }
                            else
                            {
                                menu.dialogue.GetNode<Label>("s/s/label2").SetText(text);
                            }
                            menu.dialogue.GetNode<Label>("s/label").SetText(GetWorldName());
                            menu.dialogue.Show();
                            menu.GetNode<Control>("c/game_menu").Show();
                            break;
                    }
                    player.path.Clear();
                    return;
                }
                Globals.PlaySound("click4", this, new AudioStreamPlayer());
            }
        }
        public override void MoveTo(Vector2 worldPosition)
        {
            if (path.Count == 0 || path[-1].DistanceTo(worldPosition) > weaponRange)
            {
                path = Globals.GetMap().getAPath(GetGlobalPosition(), worldPosition);
            }
            else
            {
                Vector2 direction = GetDirection(path[0]);
                if (!direction.Equals(new Vector2()))
                {
                    worldPosition = Globals.GetMap().RequestMove(GetGlobalPosition(), direction);
                    if (!worldPosition.Equals(new Vector2()))
                    {
                        Move(worldPosition, Stats.MapAnimMoveSpeed(animSpeed));
                        path.RemoveAt(0);
                        SetProcess(false);
                    }
                    else
                    {
                        path.Clear();
                    }
                }
                else
                {
                    path.RemoveAt(0);
                }
            }
        }
        public override void TakeDamage(short damage, bool ignoreArmor, WorldObject worldObject, CombatText.TextType textType)
        {
            if (target == null && worldObject is Character)
            {
                SetTarget((Character)worldObject);
            }
            base.TakeDamage(damage, ignoreArmor, worldObject, textType);
            if (dead && targetList.Count > 0)
            {
                List<short> damageList = new List<short>();
                foreach (short dam in targetList.Values)
                {
                    damageList.Add(dam);
                }
                if (damageList.Count > 0)
                {
                    short mostDamage = damageList[0];
                    foreach (short dam in damageList)
                    {
                        if (dam > mostDamage)
                        {
                            mostDamage = dam;
                        }
                    }
                    foreach (Character character in targetList.Keys)
                    {
                        if (targetList[character] == mostDamage && character is Player)
                        {
                            short xp = Stats.GetXpFromUnitDeath((double)GetLevel(),
                                Stats.GetMultiplier(true, GetNode<Sprite>("img").GetTexture().GetPath()), (double)character.GetLevel());
                            if (xp > 0)
                            {
                                ((Player)character).SetXP(xp);
                            }
                        }
                    }
                }
            }
        }
        public override async void SetDead(bool dead)
        {
            if (this.dead == dead)
            {
                return;
            }
            base.SetDead(dead);
            await ToSignal(GetNode<AnimationPlayer>("anim"), "animation_finished");
            GetNode<CollisionShape2D>("sight/distance").SetDisabled(true);
            if (dead)
            {
                EmitSignal(nameof(DropLoot), this, GetGlobalPosition(), 0);
                Hide();
                SetProcess(false);
                SetTime((float)GD.RandRange(60.0, 240.0), true);
                if (!IsInGroup(Globals.SAVE_GROUP))
                {
                    AddToGroup(Globals.SAVE_GROUP);
                }
                SetGlobalPosition(origin);
            }
            else
            {
                SetHp(hpMax);
                SetMana(manaMax);
                SetProcess(true);
                if (IsInGroup(Globals.SAVE_GROUP))
                {
                    RemoveFromGroup(Globals.SAVE_GROUP);
                }
                Show();
                Sprite img = GetNode<Sprite>("img");
                Tween tween = GetNode<Tween>("tween");
                tween.InterpolateProperty(img, ":scale", img.GetScale(), new Vector2(1.03f, 1.03f),
                    0.5f, Tween.TransitionType.Elastic, Tween.EaseType.Out);
                tween.Start();
            }
        }
        public void CheckSight(Area2D area2D)
        {
            _OnSightAreaEntered(area2D);
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
                    SetProcess(false);
                    SetTarget(null);
                    SetTime(regenTime);
                    anim.Stop();
                    img.SetFlipH(false);
                    img.SetFrame(0);
                    engaging = false;
                    foreach (Area2D area2D in GetNode<Area2D>("sight").GetOverlappingAreas())
                    {
                        CheckSight(area2D);
                    }
                    break;
                case States.ATTACKING:
                    SetTime(weaponSpeed / 2.0f, true);
                    break;
                case States.MOVING:
                    img.SetFlipH(false);
                    anim.Play("moving", -1, animSpeed);
                    anim.Seek(0.3f, true);
                    break;
                case States.RETURNING:
                    SetState(States.MOVING);
                    SetTime(regenTime);
                    break;
                case States.DEAD:
                    if (target != null && target is Player)
                    {
                        target.SetTarget(null);
                    }
                    SetState(States.IDLE);
                    SetDead(true);
                    break;
            }
            base.SetState(state);
        }
        public void SetText(string text)
        {
            // possible bug here with "{0}"
            // probably get around this with Regex
            if (text.Contains("{0}") && GetWorldType() == WorldTypes.HEALER)
            {
                this.text = string.Format(text, Stats.HealerCost(Globals.player.GetLevel()));
            }
            else
            {
                this.text = text;
            }
        }
        public void SetUpShop(Node gameMenu, bool setUp, WorldTypes actorType)
        {
            Node bag = GetNode("inventory");
            if (actorType == WorldTypes.TRAINER)
            {
                bag = GetNode("spells");
            }
            foreach (Pickable pickable in bag.GetChildren())
            {
                if (setUp)
                {
                    pickable.Connect(nameof(Pickable.SetInMenu), gameMenu, "_OnSetPickableInMenu");
                    pickable.Connect(nameof(Pickable.DescribePickable), gameMenu, "_OnDescribePickable");
                }
                else
                {
                    pickable.Disconnect(nameof(Pickable.SetInMenu), gameMenu, "_OnSetPickableInMenu");
                    pickable.Disconnect(nameof(Pickable.DescribePickable), gameMenu, "_OnDescribePickable");
                }
            }
        }
        public void SetEnemy(bool enemy)
        {
            this.enemy = enemy;
        }
        public bool IsEnemy()
        {
            return enemy;
        }
    }
}