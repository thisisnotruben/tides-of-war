using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Game.Spell;
using Game.Misc.Other;
using Game.Misc.Missile;
using Game.Misc.Loot;

namespace Game.Actor
{
    public abstract class Character : WorldObject, ISaveable
    {
        public enum States { ALIVE, DEAD, MOVING, IDLE, ATTACKING, RETURNING };
        // public enum WorldTypes { AXE, BOW, CLUB, DAGGER, SWORD, STAFF, CLAW, MAGIC }
        States state;
        public WorldTypes weaponType;
        private short level;
        private String swingType;
        public Vector2 origin;
        private protected bool dead;
        private bool enemy;
        public short armor;
        private protected Character target;
        public ushort weaponRange;
        public short hp;
        public short hpMax;
        public short mana;
        public short manaMax;
        public short regenTime;
        private protected bool engaging;
        private protected float animSpeed = 1.0f;
        public float weaponSpeed = 1.3f;
        private protected Item weapon;
        private protected Item vest;
        private bool bumping;
        public short minDamage;
        public short maxDamage;
        public short stamina;
        public short intellect;
        public short agility;
        private protected Spell.Spell spell;
        public List<Vector2> path = new List<Vector2>();
        private List<Spell.Spell> spellQueue = new List<Spell.Spell>();
        private protected Dictionary<Character, short> targetList = new Dictionary<Character, short>();
        private Dictionary<Character, short> attackerTable = new Dictionary<Character, short>();
        public Dictionary<string, List<Item>> buffs = new Dictionary<string, List<Item>>
            {
                {"active", new List<Item>()},
                {"pending", new List<Item>()}
            };
        private protected AudioStreamPlayer2D snd;
        [Signal]
        public delegate void Talked();
        [Signal]
        public delegate void Died();
        [Signal]
        public delegate void UpdateHud(string type, string worldName, short amount, short maxAmount);


        public override void _Ready()
        {
            snd = GetNode<AudioStreamPlayer2D>("snd");
            Connect(nameof(Talked), Globals.GetWorldQuests(), "update_quest_unit", new Godot.Collections.Array { this });
            Connect(nameof(Died), Globals.GetWorldQuests(), "update_quest_unit", new Godot.Collections.Array { this });
            GetNode<RayCast2D>("ray").AddException(GetNode<Area2D>("area"));
        }
        public virtual void Attack(bool ignoreArmor, Dictionary<string, Dictionary<string, ushort>> attackTable = null)
        {
            if (attackTable == null)
            {
                attackTable = Stats.attackTable;
            }
            if (target != null && !target.IsDead() && !IsDead())
            {
                if (weaponType == WorldTypes.BOW || weaponType == WorldTypes.MAGIC)
                {
                    Bolt missile = (Bolt)Globals.missile.Instance();
                    GetParent().AddChild(missile);
                    missile.SetUp(this, GetGlobalPosition(), spell);
                }
                else
                {
                    GD.Randomize();
                    ushort diceRoll = (ushort)(GD.Randi() % 100 + 1);
                    short damage = (short)Math.Round(GD.RandRange(
                        (double)minDamage, (double)maxDamage));
                    CombatText.TextType hitType;
                    ushort sndIdx = Globals.WEAPON_TYPE[nameof(weaponType)];
                    string snd = nameof(weaponType) + sndIdx;

                    if (spell != null && !spell.Casted())
                    {
                        attackTable = spell.GetAttackTable();
                        if (diceRoll <= attackTable["MELEE"]["CRITICAL"])
                        {
                            ignoreArmor = spell.IsIgnoreArmor();
                            damage = (short)Math.Round((float)damage * spell.Cast());
                            target.SetSpell(spell);

                        }
                        else
                        {
                            SetMana(spell.GetManaCost());
                        }

                    }
                    if (diceRoll <= attackTable["MELEE"]["HIT"])
                    {
                        hitType = CombatText.TextType.HIT;
                    }
                    else if (diceRoll <= attackTable["MELEE"]["CRITICAL"])
                    {
                        hitType = CombatText.TextType.CRITICAL;
                        damage *= 2;
                    }
                    else if (diceRoll <= attackTable["MELEE"]["DODGE"])
                    {
                        hitType = CombatText.TextType.DODGE;
                        damage = 0;
                        snd = nameof(swingType) + GD.Randi() % Globals.WEAPON_TYPE[nameof(swingType)];
                    }
                    else if (diceRoll <= attackTable["MELEE"]["PARRY"]
                            && target.weaponType != WorldTypes.BOW
                            && target.GetState() == States.ATTACKING)
                    {
                        hitType = CombatText.TextType.PARRY;
                        damage = 0;
                        WorldTypes[] metal = { WorldTypes.SWORD, WorldTypes.AXE, WorldTypes.DAGGER };
                        WorldTypes[] wood = { WorldTypes.CLUB, WorldTypes.STAFF, WorldTypes.CLAW };
                        if (metal.Contains(weaponType) && metal.Contains(target.weaponType))
                        {
                            snd = "block_metal_metal" + GD.Randi() % Globals.WEAPON_TYPE["BLOCK_METAL_METAL"];
                        }
                        else if (wood.Contains(weaponType) && wood.Contains(target.weaponType))
                        {
                            snd = "block_wood_wood" + GD.Randi() % Globals.WEAPON_TYPE["BLOCK_WOOD_WOOD"];
                        }
                        else
                        {
                            snd = "block_metal_wood" + GD.Randi() % Globals.WEAPON_TYPE["BLOCK_METAL_WOOD"];
                        }
                    }
                    else
                    {
                        hitType = CombatText.TextType.MISS;
                        damage = 0;
                        snd = nameof(swingType) + GD.Randi() % Globals.WEAPON_TYPE[nameof(swingType)];
                    }
                    target.TakeDamage(damage, ignoreArmor, this, hitType);
                }
                GetNode<Timer>("timer").Start();
            }
            GetNode<Sprite>("img").SetFrame(0);
        }
        public virtual void TakeDamage(short damage, bool ignoreArmor, WorldObject worldObject, CombatText.TextType textType)
        {
            if (!ignoreArmor)
            {
                damage -= armor;
            }
            if (GetState() != States.ATTACKING)
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
                        combatText.SetType(String.Format("-{0}", damage),
                           CombatText.TextType.HIT, GetNode<Node2D>("img").GetPosition());
                        if (!IsDead() && GetState() == States.ATTACKING || GetState() == States.IDLE)
                        {
                            Bump(GetDirection(attacker.GetGlobalPosition()).Rotated((float)Math.PI) / 4.0f);
                        }
                        SetHp((short)-damage);
                    }
                    else
                    {
                        switch (textType)
                        {
                            case CombatText.TextType.DODGE:
                            case CombatText.TextType.PARRY:
                            case CombatText.TextType.MISS:
                                combatText.SetType(nameof(textType),
                                    CombatText.TextType.HIT, GetNode<Node2D>("img").GetPosition());
                                break;
                        }
                    }
                }
            }
        }
        public async void Cast()
        {
            if (spell == null)
            {
                return;
            }
            path.Clear();
            SetProcess(false);
            spell.Cast();

            await ToSignal(GetNode<AnimationPlayer>("anim"), "animation_finished");

            GetNode<Sprite>("img").SetFrame(0);
            SetProcess(true);
        }
        public Vector2 GetCenterPos()
        {
            return GetNode<Node2D>("img").GetGlobalPosition();
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
            tween.InterpolateProperty(this, "@:global_position", GetGlobalPosition(), GetGlobalPosition() + direction,
                GetGlobalPosition().DistanceTo(GetGlobalPosition() + direction) / 10.0f, Tween.TransitionType.Elastic, Tween.EaseType.Out);
            tween.Start();

            await ToSignal(tween, "tween_completed");

            Vector2 gridPos = Globals.GetMap().GetGridPosition(GetGlobalPosition());
            tween.InterpolateProperty(this, "@:global_position", GetGlobalPosition(), gridPos,
                gridPos.DistanceTo(GetGlobalPosition()) / 10.0f, Tween.TransitionType.Elastic, Tween.EaseType.In);
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
                if (!node2Obj.GetScale().Equals(new Vector2(1.0f, 1.0f)))
                {
                    // Reverts to original scale when unit is clicked on
                    tween.InterpolateProperty(node2Obj, nodePath, node2Obj.GetScale(),
                        new Vector2(1.0f, 1.0f), 0.5f, Tween.TransitionType.Cubic, Tween.EaseType.Out);
                    tween.Start();
                }
            }
            if (tween.GetPauseMode() == PauseModeEnum.Process)
            {
                // Condition when merchant/trainer are selected to let their
                // animation run through it's course when game is paused
                tween.SetPauseMode(PauseModeEnum.Inherit);
            }
            else if (nodePath == ":global_position")
            {
                // When unit is done making a move, this allows him to move again
                SetProcess(true);
            }
        }
        public void _OnTimerTimeout()
        {
            if (IsDead())
            {
                if (this is Npc)
                {
                    SetState(States.ALIVE);
                }
            }
            else if (GetState() == States.ATTACKING)
            {
                if (target != null && !IsDead() && !target.IsDead())
                {
                    Node2D missile = GetNode<Node2D>("img/missile");
                    Vector2 missilePos = missile.GetGlobalPosition();
                    if (target.GetGlobalPosition().x - GetGlobalPosition().x > 0)
                    {
                        GetNode<Sprite>("img").SetFlipH(false);
                        missilePos.x = Math.Abs(missilePos.x);
                    }
                    else
                    {
                        GetNode<Sprite>("img").SetFlipH(true);
                        missilePos.x = Math.Abs(missilePos.x) * -1.0f;
                    }
                    missile.SetPosition(missilePos);
                    GetNode<AnimationPlayer>("anim").Play("attacking", -1.0f, animSpeed);
                }
            }
            else if (!engaging && (hp < hpMax || mana < manaMax))
            {
                string img = GetNode<Sprite>("img").GetTexture().GetPath();
                short regenAmount = Stats.HpManaRegenAmount(GetLevel(), Stats.GetMultiplier(this is Npc, img));
                SetHp(regenAmount);
                SetMana(regenAmount);
            }
        }
        public Vector2 GetDirection(Vector2 targetPos)
        {
            Vector2 myPos = Globals.GetMap().GetGridPosition(GetGlobalPosition());
            targetPos = Globals.GetMap().GetGridPosition(targetPos);
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
            tween.InterpolateProperty(this, "@:global_position", GetGlobalPosition(), targetPosition,
                GetGlobalPosition().DistanceTo(targetPosition) / 16.0f * speedModifier,
                Tween.TransitionType.Linear, Tween.EaseType.InOut);
            tween.Start();
        }
        public abstract void MoveTo(Vector2 WorldPosition);
        public abstract void _OnSelectPressed();
        public States GetState()
        {
            return state;
        }
        public virtual void SetState(States state)
        {
            this.state = state;
        }
        public bool IsDead()
        {
            return dead;
        }
        public virtual async void SetDead(bool dead)
        {
            this.dead = dead;
            Area2D area2D = GetNode<Area2D>("area");
            area2D.SetCollisionLayerBit(Globals.Collision["CHARACTERS"], !dead);
            area2D.SetCollisionLayerBit(Globals.Collision["DEAD_CHARACTERS"], !dead);
            if (dead)
            {
                EmitSignal("died");
                SetProcess(false);
                SetTarget(null);
                GetNode<Tween>("tween").RemoveAll();
                GetNode<Timer>("timer").Stop();
                GetNode<AnimationPlayer>("anim").Play("dying");
                SetProcessUnhandledInput(false);
                path.Clear();

                await ToSignal(GetNode<AnimationPlayer>("anim"), "animation_finished");

                Sprite img = GetNode<Sprite>("img");
                img.SetFrame(0);
                img.SetFlipH(false);
                img.SetModulate(new Color("#ffffff"));
                SetModulate(new Color(1.0f, 1.0f, 1.0f, 0.8f));

                if (hp > 0)
                {
                    SetHp((short)(-hpMax));
                }
                SetMana((short)(-manaMax));

                foreach (Spell.Spell spell in spellQueue)
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
                SetModulate(new Color("#ffffff"));
                SetTime(regenTime);
                foreach (Area2D area2d in GetNode<Area2D>("sight").GetOverlappingAreas())
                {
                    Character unit = (Character)area2d.GetOwner();
                    if (unit is Npc)
                    {
                        ((Npc)unit).CheckSight(area2D);
                    }
                }
            }
        }
        public void SetHp(short hp)
        {
            this.hp += hp;
            if (this.hp >= hpMax)
            {
                this.hp = hpMax;
                if (this is Npc && !spellQueue.Any() && IsInGroup(Globals.SAVE_GROUP))
                {
                    RemoveFromGroup(Globals.SAVE_GROUP);
                }
            }
            else if (this.hp <= 0)
            {
                this.hp = 0;
                SetState(States.DEAD);
            }
            else if (this is Npc && !IsInGroup(Globals.SAVE_GROUP))
            {
                AddToGroup(Globals.SAVE_GROUP);
            }
            if (this.hp > 0 && IsDead())
            {
                SetState((short)States.ALIVE);
            }
            EmitSignal(nameof(UpdateHud), nameof(hp), this, hp, hpMax);
        }
        public void SetMana(short mana)
        {
            this.mana += mana;
            if (this.mana >= manaMax)
            {
                this.mana = manaMax;
                if (this is Npc && !spellQueue.Any() && IsInGroup(Globals.SAVE_GROUP))
                {
                    RemoveFromGroup(Globals.SAVE_GROUP);
                }
            }
            else if (this is Npc && !IsInGroup(Globals.SAVE_GROUP))
            {
                AddToGroup(Globals.SAVE_GROUP);
            }
            if (this.mana < 0)
            {
                this.mana = 0;
            }
            EmitSignal(nameof(UpdateHud), nameof(mana), this, mana, manaMax);
        }
        public Item GetWeapon()
        {
            return weapon;
        }
        public void SetWeapon(Item item)
        {
            weapon = item;
        }
        public Item GetArmor()
        {
            return vest;
        }
        public void SetArmor(Item item)
        {
            vest = item;
        }
        public short GetLevel()
        {
            return level;
        }
        public void SetLevel(short level)
        {
            Dictionary<string, double> stats = Stats.UnitMake((double)level,
                Stats.GetMultiplier(this is Npc, GetNode<Sprite>("img").GetTexture().GetPath()));
            foreach (string attribute in stats.Keys)
            {
                Set(attribute, (short)stats[attribute]);
            }
            this.level = level;
            if (this.level > Stats.MAX_LEVEL)
            {
                this.level = Stats.MAX_LEVEL;
            }
        }
        public virtual void SetTarget(Character target)
        {
            this.target = target;
        }
        public Character GetTarget()
        {
            return target;
        }
        public void SetSpell(Spell.Spell spell, float seek = 0.0f)
        {
            if (spell == null)
            {
                this.spell = null;
            }
            else
            {
                foreach (Spell.Spell spll in spellQueue)
                {
                    if (spll.GetWorldName().Equals(spell.GetWorldName()))
                    {
                        spll.UnMake();
                    }
                }
                if (spell.GetDuration() > 0.0f)
                {
                    spellQueue.Add(spell);
                    spell.Connect(nameof(Spell.Spell.Unmake), this, nameof(RemoveFromSpellQueue), new Godot.Collections.Array() { spell });
                }
                EmitSignal(nameof(UpdateHud), "icon", this, spell, seek);
            }
        }
        public void RemoveFromSpellQueue(Spell.Spell spell)
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
                    BuffAnim buffAnim = (BuffAnim)Globals.buffAnim.Instance();
                    GetNode("img").AddChild(buffAnim);
                    buffAnim.SetItem(buff);
                    buff.Connect(nameof(Item.UnMake), buffAnim, nameof(QueueFree));
                }
                buff.GetNode<Timer>("timer").Start();
                foreach (Item currentBuff in buffs["active"])
                {
                    if (currentBuff.GetPickableSubType() == buff.GetPickableSubType() && !buff.GetWorldName().ToLower().Contains("potion"))
                    {
                        currentBuff.ConfigureBuff(this, true);
                    }
                }
                if (buff.GetPickableSubType() != Item.WorldTypes.HEALING && buff.GetPickableSubType() != Item.WorldTypes.MANA)
                {
                    EmitSignal(nameof(UpdateHud), "icon", this, buff, seek);
                }
                buffs["active"].Remove(buff);
                buffPool.Remove(buff);
            }
        }
        public ushort GetWeaponRange()
        {
            return weaponRange;
        }
        public Spell.Spell GetSpell()
        {
            return spell;
        }
        public void SetTime(float time, bool oneShot = false)
        {
            Timer timer = GetNode<Timer>("timer");
            timer.Stop();
            timer.SetWaitTime(time);
            timer.SetOneShot(oneShot);
            timer.Start();
        }
        public void PlaceFootStep(bool step)
        {
            if (!IsDead())
            {
                FootStep footStep = (FootStep)Globals.footStep.Instance();
                Vector2 stepPos = GetGlobalPosition();
                stepPos.y -= 3;
                stepPos.x += (step) ? 1.0f : -4.0f;
                Globals.GetMap().GetNode("ground").AddChild(footStep);
                footStep.SetGlobalPosition(stepPos);
            }
        }
        public bool IsEnemy(Character character)
        {
            return enemy;
        }
        public void UpdateHUD()
        {
            EmitSignal(nameof(UpdateHud), new Godot.Collections.Array() { "hp", GetWorldName(), hp, hpMax });
            EmitSignal(nameof(UpdateHud), new Godot.Collections.Array() { "mana", GetWorldName(), mana, manaMax });
            GD.Print("Partially implemented");
        }
        public void SetImg(string imgPath, bool loaded = false)
        {
            string[] splittedImgPath = imgPath.BaseName().Split('/');
            string[] parsedImg = splittedImgPath[splittedImgPath.Length - 1].Split('-');
            string raceName = splittedImgPath[splittedImgPath.Length - 2];
            Sprite img = GetNode<Sprite>("img");
            if (img.GetTexture() == null || !imgPath.Equals(img.GetTexture().GetPath()))
            {
                img.SetTexture((Texture)GD.Load(imgPath));
            }
            if (parsedImg[0].Equals("comm"))
            {
                img.SetHframes(Int32.Parse(parsedImg[2]));
                SetWorldType(WorldTypes.COMMONER);
            }
            else
            {
                byte idx = 0;
                if (raceName.Equals("critter"))
                {
                    idx = 1;
                }
                if (!parsedImg[idx].ToUpper().Equals("null"))
                {
                    switch (parsedImg[idx].ToUpper())
                    {
                        case "SICKLE":
                        case "MAGIC":
                            weaponType = WorldTypes.DAGGER;
                            break;
                        case "HATCHET":
                        case "SPEAR":
                            weaponType = WorldTypes.SWORD;
                            break;
                        case "STAFF":
                        case "MACE":
                        case "ROCK":
                            weaponType = WorldTypes.CLUB;
                            break;
                        case "BOW":
                            weaponRange = Stats.WEAPON_RANGE_RANGE;
                            break;
                        default:
                            GD.PrintErr(string.Format("{0} doesn't have a valid weaponType", imgPath));
                            break;
                    }
                }
                swingType = parsedImg[idx + 1];
                img.SetHframes(Int32.Parse(parsedImg[idx + 2]));
                if (img.GetHframes() != 17)
                {
                    AnimationPlayer anim = GetNode<AnimationPlayer>("anim");
                    anim.RemoveAnimation("attacking");
                    anim.AddAnimation("attacking",
                        (Animation)GD.Load(string.Format("res://asset/img/character/resource/attacking_{0}f.res", parsedImg[idx + 2])));
                }
                string bodyName = raceName;
                if (raceName.Equals("human"))
                {
                    switch (img.GetHframes())
                    {
                        case 20:
                        case 16:
                        case 10:
                            bodyName += "_unarmored";
                            break;
                        default:
                            bodyName += "_armored";
                            break;
                    }
                }
                else if (raceName.Equals("critter"))
                {
                    bodyName = imgPath.GetFile().BaseName().Split('-')[0];
                }
                if (this is Player)
                {
                    SetWorldType(WorldTypes.PLAYER);
                }
                AtlasTexture texture = (AtlasTexture)GD.Load(string.Format("res://asset/img/character/resource/{0}_body.res", bodyName));
                TouchScreenButton select = GetNode<TouchScreenButton>("select");
                Vector2 textureSize = texture.GetRegion().Size;
                Node2D areaBody = GetNode<Node2D>("area/body");
                select.SetTexture(texture);
                img.SetPosition(new Vector2(0.0f, -img.GetTexture().GetHeight() / 2.0f));
                GetNode<Node2D>("head").SetPosition(new Vector2(0.0f, -texture.GetHeight()));
                select.SetPosition(new Vector2(-textureSize.x / 2.0f, -textureSize.y));
                areaBody.SetPosition(new Vector2(-0.5f, -textureSize.y / 2.0f));
                GetNode<Node2D>("sight/distance").SetPosition(areaBody.GetGlobalPosition());
                Dictionary<string, double> stats = Stats.UnitMake((double)level, Stats.GetMultiplier(this is Npc, imgPath));
                foreach (string attribute in stats.Keys)
                {
                    Set(attribute, (short)stats[attribute]);
                }
                SetTime(regenTime);
                if (!loaded)
                {
                    SetHp(hpMax);
                    SetMana(manaMax);
                }
            }
        }
        public virtual void SetSaveData(Godot.Collections.Dictionary saveData)
        {
            GD.Print("Save Not Implemented");
        }
        public virtual Godot.Collections.Dictionary GetSaveData()
        {
            GD.Print("Save Not Implemented");
            return new Godot.Collections.Dictionary();
        }
    }
}