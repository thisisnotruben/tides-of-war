using System;
using System.Collections.Generic;
using System.Linq;
using Game.Ability;
using Game.Database;
using Game.Loot;
using Game.Missile;
using Game.Actor.Doodads;
using Game.Utils;
using Godot;
namespace Game.Actor
{
    public abstract class Character : WorldObject, ISaveable
    {
        public static readonly PackedScene footStepScene = (PackedScene)GD.Load("res://src/character/doodads/footstep.tscn");
        public static readonly PackedScene buffAnimScene = (PackedScene)GD.Load("res://src/character/doodads/buff_anim.tscn");
        public static readonly PackedScene missileScene = (PackedScene)GD.Load("res://src/missile/missile.tscn");
        public enum States { ALIVE, DEAD, MOVING, IDLE, ATTACKING, RETURNING }
        public virtual States state { get; private set; }
        private string swingType;
        private bool bumping;
        public bool enemy { get; private protected set; }
        private protected bool engaging;
        public bool dead { get; private set; }
        private byte _level;
        public byte level
        {
            get
            {
                return _level;
            }
            set
            {
                _level = value;
                Dictionary<string, double> stats = Stats.UnitMake((double)level,
                    Stats.GetMultiplier(this is Npc, GetNode<Sprite>("img").Texture.ResourcePath));
                foreach (string attribute in stats.Keys)
                {
                    Set(attribute, (short)stats[attribute]);
                }
                if (level > Stats.MAX_LEVEL)
                {
                    _level = Stats.MAX_LEVEL;
                }
            }
        }
        public short armor;
        private short _hp;
        public short hp
        {
            get
            {
                return _hp;
            }
            set
            {
                _hp += value;
                if (hp >= hpMax)
                {
                    _hp = hpMax;
                    if (this is Npc && !spellQueue.Any() && IsInGroup(Globals.SAVE_GROUP))
                    {
                        RemoveFromGroup(Globals.SAVE_GROUP);
                    }
                }
                else if (hp <= 0)
                {
                    _hp = 0;
                    SetState(States.DEAD);
                }
                else if (this is Npc && !IsInGroup(Globals.SAVE_GROUP))
                {
                    AddToGroup(Globals.SAVE_GROUP);
                }
                if (hp > 0 && dead)
                {
                    SetState((short)States.ALIVE);
                }
                EmitSignal(nameof(UpdateHud), nameof(hp), worldName, hp, hpMax);
            }
        }
        public short hpMax;
        private short _mana;
        public short mana
        {
            get
            {
                return _mana;
            }
            set
            {
                _mana += value;
                if (mana >= manaMax)
                {
                    _mana = manaMax;
                    if (this is Npc && !spellQueue.Any() && IsInGroup(Globals.SAVE_GROUP))
                    {
                        RemoveFromGroup(Globals.SAVE_GROUP);
                    }
                }
                else if (this is Npc && !IsInGroup(Globals.SAVE_GROUP))
                {
                    AddToGroup(Globals.SAVE_GROUP);
                }
                if (mana < 0)
                {
                    _mana = 0;
                }
                EmitSignal(nameof(UpdateHud), nameof(mana), worldName, mana, manaMax);
            }
        }
        public short manaMax;
        public short regenTime;
        public short minDamage;
        public short maxDamage;
        public short stamina { get; private set; }
        public short intellect { get; private set; }
        public short agility { get; private set; }
        public bool ranged  { get; private protected set; }
        public float animSpeed = 1.0f;
        public ushort weaponRange = Stats.WEAPON_RANGE_MELEE;
        public float weaponSpeed = 1.3f;
        private string weaponMaterial;
        private string weaponSnd;
        public Item weapon = null;
        public Item vest = null;
        public Spell spell { get; private protected set; }
        public virtual Character target { get; set; }
        private protected Speaker2D snd;
        private protected Vector2 origin = new Vector2();
        private protected List<Vector2> path = new List<Vector2>();
        private List<Spell> spellQueue = new List<Spell>();
        private protected Dictionary<Character, short> targetList = new Dictionary<Character, short>();
        private Dictionary<Character, short> attackerTable = new Dictionary<Character, short>();
        public Dictionary<string, List<Item>> buffs = new Dictionary<string, List<Item>>
        { { "active", new List<Item>() },
            { "pending", new List<Item>() }
        };
        [Signal]
        public delegate void Talked();
        [Signal]
        public delegate void Died();
        [Signal]
        public delegate void UpdateHud(string type, string worldName, short amount, short maxAmount);
        [Signal]
        public delegate void UpdateHudIcon(string worldName, Pickable pickable, float seek);
        public override void _Ready()
        {
            snd = GetNode<Speaker2D>("snd");
            // Connect(nameof(Talked), Globals.worldQuests,
            //     nameof(Game.Quests.WorldQuests.UpdateQuestCharacter), new Godot.Collections.Array { this });
            // Connect(nameof(Died), Globals.worldQuests,
            //     nameof(Game.Quests.WorldQuests.UpdateQuestCharacter), new Godot.Collections.Array { this });
            GetNode<RayCast2D>("ray").AddException(GetNode<Area2D>("area"));
        }
        public virtual void Attack(bool ignoreArmor = false)
        {
            if (target != null && !target.dead && !dead)
            {
                if (ranged)
                {
                    return; // TODO
                    Bolt missile = (Bolt)missileScene.Instance();
                    GetParent().AddChild(missile);
                    missile.SetUp(this, GlobalPosition, spell);
                }
                else
                {
                    GD.Randomize();
                    ushort diceRoll = (ushort)(GD.Randi() % 100 + 1);
                    short damage = (short)Math.Round(GD.RandRange(
                        (double)minDamage, (double)maxDamage));
                    ushort sndIdx = Globals.WEAPON_TYPE[weaponSnd.ToString()];
                    string sndName = weaponSnd.ToString().ToLower() + sndIdx.ToString();
                    CombatText.TextType hitType;
                    Dictionary<string, ushort> attackTable = Stats.attackTable["MELEE"];
                    if (spell != null && !spell.casted)
                    {
                        attackTable = spell.GetAttackTable();
                        if (diceRoll <= attackTable["CRITICAL"])
                        {
                            ignoreArmor = spell.ignoreArmor;
                            damage = (short)Math.Round((float)damage * spell.Cast());
                            target.SetSpell(spell);
                        }
                        else
                        {
                            mana = spell.manaCost;
                        }
                    }
                    if (diceRoll <= attackTable["HIT"])
                    {
                        hitType = CombatText.TextType.HIT;
                    }
                    else if (diceRoll <= attackTable["CRITICAL"])
                    {
                        hitType = CombatText.TextType.CRITICAL;
                        damage *= 2;
                    }
                    else if (diceRoll <= attackTable["DODGE"])
                    {
                        hitType = CombatText.TextType.DODGE;
                        damage = 0;
                        sndName = swingType + GD.Randi() % Globals.WEAPON_TYPE[swingType.ToUpper()];
                    }
                    else if (diceRoll <= attackTable["PARRY"] && !ranged && !target.ranged)
                    {
                        hitType = CombatText.TextType.PARRY;
                        damage = 0;
                        sndName = (weaponMaterial.Equals(target.weaponMaterial)) ? $"block_{weaponMaterial}_{weaponMaterial}" : "block_metal_wood";
                        sndName += GD.Randi() % Globals.WEAPON_TYPE[sndName.ToUpper()];
                    }
                    else
                    {
                        hitType = CombatText.TextType.MISS;
                        damage = 0;
                        sndName = swingType + GD.Randi() % Globals.WEAPON_TYPE[swingType.ToUpper()];
                    }
                    target.TakeDamage(damage, ignoreArmor, this, hitType);
                    Globals.PlaySound(sndName, this, snd);
                }
                GetNode<Timer>("timer").Start();
            }
            GetNode<Sprite>("img").Frame = 0;
        }
        public virtual void TakeDamage(short damage, bool ignoreArmor, WorldObject worldObject, CombatText.TextType textType)
        {
            if (!ignoreArmor)
            {
                damage -= armor;
            }
            if (state != States.ATTACKING)
            {
                // Stops regen timer
                GetNode<Timer>("timer").Stop();
            }
            if (worldObject is Character)
            {
                Character attacker = (Character)(worldObject);
                engaging = true;
                if (attackerTable.ContainsKey(attacker))
                {
                    attackerTable[attacker] += damage;
                }
                else
                {
                    attackerTable.Add(attacker, damage);
                }
                if (attacker is Player && this is Npc || this is Player)
                {
                    CombatText combatText = (CombatText)Globals.combatText.Instance();
                    AddChild(combatText);
                    if (damage > 0)
                    {
                        hp = (short) - damage;
                        combatText.SetType($"-{damage}", CombatText.TextType.HIT, GetNode<Node2D>("img").Position);
                        if (!dead && state == States.ATTACKING || state == States.IDLE)
                        {
                            Bump(GetDirection(GlobalPosition, attacker.GlobalPosition).Rotated((float)Math.PI) / 4.0f);
                        }
                    }
                    else
                    {
                        switch (textType)
                        {
                            case CombatText.TextType.DODGE:
                            case CombatText.TextType.PARRY:
                            case CombatText.TextType.MISS:
                                combatText.SetType(textType.ToString(),
                                    CombatText.TextType.HIT, GetNode<Node2D>("img").Position);
                                break;
                        }
                    }
                }
            }
        }
        public async void Cast()
        {
            if (spell != null)
            {
                path.Clear();
                SetProcess(false);
                spell.Cast();
                await ToSignal(GetNode<AnimationPlayer>("anim"), "animation_finished");
                GetNode<Sprite>("img").Frame = 0;
                SetProcess(true);
            }
        }
        public Vector2 GetCenterPos()
        {
            return GetNode<Node2D>("img").GlobalPosition;
        }
        public async void Bump(Vector2 direction)
        {
            if (bumping || direction.Equals(new Vector2()))
            {
                return;
            }
            bumping = true;
            SetProcess(false);
            Tween tween = GetNode<Tween>("tween");
            tween.InterpolateProperty(this, ":global_position", GlobalPosition, GlobalPosition + direction,
                GlobalPosition.DistanceTo(GlobalPosition + direction) / 10.0f, Tween.TransitionType.Elastic, Tween.EaseType.Out);
            tween.Start();
            await ToSignal(tween, "tween_completed");
            Vector2 gridPos = Globals.map.GetGridPosition(GlobalPosition);
            tween.InterpolateProperty(this, ":global_position", GlobalPosition, gridPos,
                gridPos.DistanceTo(GlobalPosition) / 10.0f, Tween.TransitionType.Elastic, Tween.EaseType.In);
            tween.Start();
            await ToSignal(tween, "tween_completed");
            bumping = false;
            SetProcess(true);
        }
        public void _OnTweenCompleted(Godot.Object obj, NodePath nodePath)
        {
            Tween tween = GetNode<Tween>("tween");
            if (obj is Node2D)
            {
                Node2D node2Obj = (Node2D)obj;
                if (!node2Obj.Scale.Equals(new Vector2(1.0f, 1.0f)))
                {
                    // Reverts to original scale when unit is clicked on
                    tween.InterpolateProperty(node2Obj, nodePath, node2Obj.Scale,
                        new Vector2(1.0f, 1.0f), 0.5f, Tween.TransitionType.Cubic, Tween.EaseType.Out);
                    tween.Start();
                }
            }
            if (tween.PauseMode == PauseModeEnum.Process)
            {
                // Condition when merchant/trainer are selected to let their
                // animation run through it's course when game is paused
                tween.PauseMode = PauseModeEnum.Inherit;
            }
            else if (nodePath == ":global_position")
            {
                // When unit is done making a move, this allows him to move again
                SetProcess(true);
            }
        }
        public void _OnTimerTimeout()
        {
            if (dead)
            {
                if (this is Npc)
                {
                    SetState(States.ALIVE);
                }
            }
            else if (state == States.ATTACKING)
            {
                if (target != null && !dead && !target.dead)
                {
                    Sprite img = GetNode<Sprite>("img");
                    Node2D missile = GetNode<Node2D>("img/missile");
                    Vector2 missilePos = missile.Position;
                    if (target.GlobalPosition.x - GlobalPosition.x > 0.0f)
                    {
                        img.FlipH = false;
                        missilePos.x = Math.Abs(missilePos.x);
                    }
                    else
                    {
                        img.FlipH = true;
                        missilePos.x = Math.Abs(missilePos.x) * -1.0f;
                    }
                    missile.Position = missilePos;
                    GetNode<AnimationPlayer>("anim").Play("attacking", -1.0f, animSpeed);
                }
            }
            else if (!engaging && (hp < hpMax || mana < manaMax))
            {
                string imgPath = GetNode<Sprite>("img").Texture.ResourcePath;
                short regenAmount = Stats.HpManaRegenAmount(level, Stats.GetMultiplier(this is Npc, imgPath));
                hp = regenAmount;
                mana = regenAmount;
            }
        }
        private protected static Vector2 GetDirection(Vector2 currentPos, Vector2 targetPos)
        {
            Vector2 myPos = Globals.map.GetGridPosition(currentPos);
            targetPos = Globals.map.GetGridPosition(targetPos);
            Vector2 direction = new Vector2();
            if (myPos.x > targetPos.x)
            {
                direction.x--;
            }
            else if (myPos.x < targetPos.x)
            {
                direction.x++;
            }
            if (myPos.y > targetPos.y)
            {
                direction.y--;
            }
            else if (myPos.y < targetPos.y)
            {
                direction.y++;
            }
            return direction;
        }
        public void Move(Vector2 targetPosition, float speedModifier = Stats.SPEED)
        {
            SetProcess(false);
            Tween tween = GetNode<Tween>("tween");
            tween.StopAll();
            tween.InterpolateProperty(this, ":global_position", GlobalPosition, targetPosition,
                GlobalPosition.DistanceTo(targetPosition) / 16.0f * speedModifier,
                Tween.TransitionType.Linear, Tween.EaseType.InOut);
            tween.Start();
        }
        public abstract void MoveTo(Vector2 WorldPosition, List<Vector2> route);
        public abstract void _OnSelectPressed();
        public virtual void SetState(States state, bool overrule = false)
        {
            this.state = state;
        }
        public virtual async void SetDead(bool dead)
        {
            this.dead = dead;
            Area2D area2D = GetNode<Area2D>("area");
            area2D.SetCollisionLayerBit(Globals.Collision["CHARACTERS"], !dead);
            area2D.SetCollisionLayerBit(Globals.Collision["DEAD_CHARACTERS"], !dead);
            if (dead)
            {
                EmitSignal(nameof(Died));
                SetProcess(false);
                target = null;
                GetNode<Tween>("tween").RemoveAll();
                GetNode<Timer>("timer").Stop();
                GetNode<AnimationPlayer>("anim").Play("dying");
                SetProcessUnhandledInput(false);
                path.Clear();
                await ToSignal(GetNode<AnimationPlayer>("anim"), "animation_finished");
                Sprite img = GetNode<Sprite>("img");
                img.Frame = 0;
                img.FlipH = false;
                img.Modulate = new Color("#ffffff");
                Modulate = new Color(1.0f, 1.0f, 1.0f, 0.8f);
                if (hp > 0)
                {
                    hp = (short)(-hpMax);
                }
                mana = (short)(-manaMax);
                foreach (Spell spell in spellQueue)
                {
                    spell.UnMake();
                }
                foreach (Node node in GetChildren())
                {
                    if (node is CombatText)
                    {
                        node.QueueFree();
                    }
                }
            }
            else
            {
                Modulate = new Color("#ffffff");
                SetTime(regenTime);
                foreach (Area2D area2d in GetNode<Area2D>("sight").GetOverlappingAreas())
                {
                    Character unit = (Character)area2d.Owner;
                    if (unit is Npc)
                    {
                        ((Npc)unit).CheckSight(area2D);
                    }
                }
            }
        }
        public void SetCurrentSpell(Spell spell)
        {
            this.spell = spell;
        }
        public void SetSpell(Spell spell, float seek = 0.0f)
        {
            foreach (Spell otherSpell in spellQueue)
            {
                if (otherSpell.Equals(spell))
                {
                    otherSpell.UnMake();
                }
            }
            if (spell.duration > 0.0f)
            {
                spellQueue.Add(spell);
                spell.Connect(nameof(Spell.Unmake), this, nameof(RemoveFromSpellQueue), new Godot.Collections.Array() { spell });
            }
            EmitSignal(nameof(UpdateHudIcon), worldName, spell, seek);
        }
        public void RemoveFromSpellQueue(Spell spell)
        {
            spellQueue.Remove(spell);
        }
        public void SetBuff(List<Item> buffPool = null, float seek = 0.0f)
        {
            if (buffPool == null)
            {
                buffPool = buffs["pending"];
            }
            foreach (Item buff in buffPool)
            {
                if (seek == 0.0f)
                {
                    BuffAnim buffAnim = (BuffAnim)buffAnimScene.Instance();
                    GetNode("img").AddChild(buffAnim);
                    buffAnim.item = buff;
                    buff.Connect(nameof(Item.UnMake), buffAnim, nameof(QueueFree));
                }
                buff.GetNode<Timer>("timer").Start();
                foreach (Item currentBuff in buffs["active"])
                {
                    if (currentBuff.subType == buff.subType && !buff.worldName.ToLower().Contains("potion"))
                    {
                        currentBuff.ConfigureBuff(this, true);
                    }
                }
                if (buff.subType != Item.WorldTypes.HEALING && buff.subType != Item.WorldTypes.MANA)
                {
                    EmitSignal(nameof(UpdateHudIcon), worldName, buff, seek);
                }
                buffs["active"].Remove(buff);
                buffPool.Remove(buff);
            }
        }
        public void SetTime(float time, bool oneShot = false)
        {
            if (time > 0.0f)
            {
                Timer timer = GetNode<Timer>("timer");
                timer.Stop();
                timer.WaitTime = time;
                timer.OneShot = oneShot;
                timer.Start();
            }
            else
            {
                GD.Print($"Error: {worldName} set invalid time");
            }
        }
        public void PlaceFootStep(bool step)
        {
            if (!dead)
            {
                FootStep footStep = (FootStep)footStepScene.Instance();
                Vector2 stepPos = GlobalPosition;
                stepPos.y -= 3;
                stepPos.x += (step) ? 1.0f : -4.0f;
                Globals.map.GetNode("ground").AddChild(footStep);
                footStep.GlobalPosition = stepPos;
            }
        }
        public void UpdateHUD()
        {
            EmitSignal(nameof(UpdateHud), "HP", worldName, hp, hpMax);
            EmitSignal(nameof(UpdateHud), "MANA", worldName, mana, manaMax);
            // EmitSignal(nameof(UpdateHud), "ICON_HIDE", worldName, hp, hpMax);
            foreach (Spell spell in spellQueue)
            {
                EmitSignal(nameof(UpdateHudIcon), spell, spell.GetTimeLeft());
            }
        }
        public void SetImg(string imgName, bool loaded = false)
        {
            Dictionary<string, string> imageData = ImageDB.GetImageData(imgName);
            swingType = imageData["swing"];
            Sprite img = GetNode<Sprite>("img");
            img.Texture = (Texture)GD.Load($"res://asset/img/character/{imgName}.png");
            img.Hframes = int.Parse(imageData["total"]);
            AnimationPlayer anim = GetNode<AnimationPlayer>("anim");
            Animation animRes = anim.GetAnimation("attacking");
            animRes.TrackSetKeyValue(0, 0, int.Parse(imageData["moving"]) + int.Parse(imageData["dying"]));
            animRes.TrackSetKeyValue(0, 1, int.Parse(imageData["attacking"]) - 1);
            animRes = anim.GetAnimation("moving");
            animRes.TrackSetKeyValue(0, 0, 0);
            animRes.TrackSetKeyValue(0, 1, int.Parse(imageData["moving"]));
            animRes = anim.GetAnimation("dying");
            animRes.TrackSetKeyValue(0, 0, int.Parse(imageData["moving"]));
            animRes.TrackSetKeyValue(0, 1, int.Parse(imageData["dying"]));
            animRes = anim.GetAnimation("casting");
            animRes.TrackSetKeyValue(0, 0, int.Parse(imageData["moving"]));
            animRes.TrackSetKeyValue(0, 1, int.Parse(imageData["moving"]) + 3);
            if (imageData["attacking"].Equals("0"))
            {
                anim.RemoveAnimation("attacking");
            }
            weaponRange = (bool.Parse(imageData["melee"])) ? Stats.WEAPON_RANGE_MELEE : Stats.WEAPON_RANGE_RANGE;
            weaponSnd = imageData["weapon"].ToLower();
            switch (weaponSnd)
            {
                case "magic":
                case "claw":
                case "sickle":
                    weaponSnd = "dagger";
                    break;
                case "spear":
                case "hatchet":
                    weaponSnd = "sword";
                    break;
                case "staff":
                case "mace":
                case "rock":
                    weaponSnd = "club";
                    break;
            }
            AtlasTexture texture = (AtlasTexture)GD.Load($"res://asset/img/character/resource/bodies/body-{imageData["body"]}.tres");
            TouchScreenButton select = GetNode<TouchScreenButton>("select");
            Vector2 textureSize = texture.Region.Size;
            Node2D sightDistance = GetNode<Node2D>("sight/distance");
            Node2D areaBody = GetNode<Node2D>("area/body");
            Node2D head = GetNode<Node2D>("head");
            select.Normal = texture;
            img.Position = new Vector2(0.0f, -img.Texture.GetHeight() / 2.0f);
            head.Position = new Vector2(0.0f, -texture.GetHeight());
            select.Position = new Vector2(-textureSize.x / 2.0f, -textureSize.y);
            areaBody.Position = new Vector2(-0.5f, -textureSize.y / 2.0f);
            sightDistance.Position = areaBody.Position;
            Dictionary<string, double> stats = Stats.UnitMake((double)level, Stats.GetMultiplier(this is Npc, imgName));
            foreach (string attribute in stats.Keys)
            {
                Set(attribute, (short)stats[attribute]);
            }
            SetTime(regenTime);
            if (!loaded)
            {
                hp = hpMax;
                mana = manaMax;
            }
        }
        public virtual void SetSaveData(Godot.Collections.Dictionary saveData)
        {
            GD.Print("TODO: Save Not Implemented");
        }
        public virtual Godot.Collections.Dictionary GetSaveData()
        {
            GD.Print("TODO: Save Not Implemented");
            return new Godot.Collections.Dictionary();
        }
    }
}