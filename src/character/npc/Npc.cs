using System.Collections.Generic;
using Game.Database;
using Game.Actor.Doodads;
using Godot;
namespace Game.Actor
{
    public class Npc : Character
    {
        private Dictionary<string, List<Vector2>> cachedPatrolPath;

        public override void init()
        {
            cachedPatrolPath = new Dictionary<string, List<Vector2>>();
            cachedPatrolPath.Add("cachedPath", new List<Vector2>());
            cachedPatrolPath.Add("pathPoints", new List<Vector2>());
            cachedPatrolPath.Add("patrolPath", new List<Vector2>());
            UnitDB.UnitNode unitNode = UnitDB.GetUnitData(Name);
            worldName = unitNode.name;
            SetImg(unitNode.img);
            enemy = unitNode.enemy;
            if (unitNode.path.Count > 0)
            {
                cachedPatrolPath["cachedPath"] = unitNode.path;    
                cachedPatrolPath["pathPoints"] = cachedPatrolPath["cachedPath"].GetRange(0, cachedPatrolPath["cachedPath"].Count);
                SetProcess(true);
            }
            if (ContentDB.HasContent(Name))
            {
                enemy = ContentDB.GetContentData(Name).enemy;
            }
            SetAttributes();
            hp = hpMax;
            mana = manaMax;
        }
        public override void _Ready()
        {
            SetProcess(false);
            base.init();
            base._Ready();
            init();
            SetTime(regenTime);
        }
        public new void SetProcess(bool mode)
        {
            base.SetProcess((GetNode<VisibilityNotifier2D>("visible").IsOnScreen()) ? mode : false);
        }
        public override void _Process(float delta)
        {
            if (engaging && target != null)
            {
                Vector2 spawnPos = UnitDB.GetUnitData(Name).spawnPos;
                if (spawnPos.DistanceTo(target.GetCenterPos()) > Stats.FLEE_DISTANCE || target.dead)
                {
                    SetState(States.RETURNING);
                    if (cachedPatrolPath["cachedPath"].Count > 0)
                    {
                        // if (GlobalPosition != cachedPatrolPath["cachedPath"][patrolPath.Count - 1]) TODO
                        // {
                        // FollowPatrolPath();
                        // return;
                        // }
                    }
                    else if (GlobalPosition != spawnPos)
                    {
                        MoveTo(spawnPos, path);
                        return;
                    }
                    SetState(States.IDLE);
                }
                else if (!GetNode<AnimationPlayer>("anim").HasAnimation("attacking"))
                {
                    // flee code here
                }
                else if (GetCenterPos().DistanceTo(target.GetCenterPos()) > weaponRange)
                {
                    SetState(States.MOVING);
                    MoveTo(target.GlobalPosition, path);
                }
                else
                {
                    SetState(States.ATTACKING);
                }
            }
            else if (cachedPatrolPath["cachedPath"].Count > 0)
            {
                SetState(States.MOVING);
                FollowPatrolPath();
            }
            else
            {
                SetState(States.IDLE);
            }
        }
        public void _OnSightAreaEnteredExited(Area2D area2D, bool entered)
        {
            if (entered)
            {
                Character character = area2D.Owner as Character;
                if (character != null && !dead && (target == null || character.dead) && character != this)
                {
                    if (!enemy && character is Player)
                    {
                        // friendly npcs' don't attack player
                        return;
                    }
                    else if (!engaging && enemy != character.enemy)
                    {
                        engaging = true;
                        target = character;
                        SetProcess(true);
                    }
                }
            }
            else if (!engaging)
            {
                target = null;
            }
        }
        public void _OnAreaMouseEnteredExited(bool entered)
        {
            Player.player.SetProcessUnhandledInput(!entered);
        }
        public void _OnScreenEnteredExited(bool entered)
        {
            SetProcess(entered);
        }
        public override void _OnSelectPressed()
        {
            Player.player.GetMenu().NpcInteract(this);
        }
        private void FollowPatrolPath()
        {
            if (cachedPatrolPath["patrolPath"].Count == 0)
            {
                cachedPatrolPath["pathPoints"].RemoveAt(0);
                if (cachedPatrolPath["pathPoints"].Count == 0)
                {
                    cachedPatrolPath["cachedPath"].Reverse();
                    cachedPatrolPath["pathPoints"] = cachedPatrolPath["cachedPath"].GetRange(0, cachedPatrolPath["cachedPath"].Count);
                }
                cachedPatrolPath["patrolPath"] = Map.Map.map.getAPath(GlobalPosition, cachedPatrolPath["pathPoints"][0]);
            }
            MoveTo(cachedPatrolPath["patrolPath"][0], cachedPatrolPath["patrolPath"]);
        }
        public override void MoveTo(Vector2 worldPosition, List<Vector2> route)
        {
            if (route == path && (route.Count == 0 || route[route.Count - 1].DistanceTo(worldPosition) > weaponRange))
            {
                path = Map.Map.map.getAPath(GlobalPosition, worldPosition);
            }
            else
            {
                Vector2 direction = GetDirection(GlobalPosition, route[0]);
                if (!direction.Equals(new Vector2()))
                {
                    worldPosition = Map.Map.map.RequestMove(GlobalPosition, direction);
                    if (!worldPosition.Equals(new Vector2()))
                    {
                        Move(worldPosition, Stats.MapAnimMoveSpeed(animSpeed));
                        route.RemoveAt(0);
                        SetProcess(false);
                    }
                    else
                    {
                        route.Clear();
                    }
                }
                else
                {
                    route.RemoveAt(0);
                }
            }
        }
        public override void TakeDamage(int damage, bool ignoreArmor, WorldObject worldObject, CombatText.TextType textType)
        {
            if (target == null && worldObject is Character)
            {
                target = (Character)worldObject;
            }
            base.TakeDamage(damage, ignoreArmor, worldObject, textType);
            if (dead && targetList.Count > 0)
            {
                List<int> damageList = new List<int>();
                foreach (int dam in targetList.Values)
                {
                    damageList.Add(dam);
                }
                if (damageList.Count > 0)
                {
                    int mostDamage = damageList[0];
                    foreach (int dam in damageList)
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
                            int xp = Stats.GetXpFromUnitDeath((double)UnitDB.GetUnitData(Name).level,
                                Stats.GetMultiplier(true, GetNode<Sprite>("img").Texture.ResourcePath), (double)character.level);
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
            GetNode<CollisionShape2D>("sight/distance").Disabled = true;
            if (dead)
            {
                Hide();
                SetProcess(false);
                GD.Randomize();
                SetTime((float)GD.RandRange(60.0, 240.0), true);
                if (!IsInGroup(Globals.SAVE_GROUP))
                {
                    AddToGroup(Globals.SAVE_GROUP);
                }
                GlobalPosition = UnitDB.GetUnitData(Name).spawnPos;
            }
            else
            {
                hp = hpMax;
                mana = manaMax;
                SetProcess(true);
                if (IsInGroup(Globals.SAVE_GROUP))
                {
                    RemoveFromGroup(Globals.SAVE_GROUP);
                }
                Show();
                Sprite img = GetNode<Sprite>("img");
                Tween tween = GetNode<Tween>("tween");
                tween.InterpolateProperty(img, ":scale", img.Scale, new Vector2(1.03f, 1.03f),
                    0.5f, Tween.TransitionType.Elastic, Tween.EaseType.Out);
                tween.Start();
            }
        }
        public void CheckSight(Area2D area2D)
        {
            _OnSightAreaEnteredExited(area2D, true);
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
                        SetProcess(false);
                        target = null;
                        SetTime(regenTime);
                        anim.Stop();
                        img.FlipH = false;
                        img.Frame = 0;
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
                        img.FlipH = false;
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
                            target.target = null;
                        }
                        SetState(States.IDLE);
                        SetDead(true);
                        break;
                }
                base.SetState(state);
            }
        }
    }
}