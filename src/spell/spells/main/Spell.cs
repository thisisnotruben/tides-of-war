using System;
using System.Collections.Generic;
using Game.Actor;
using Game.Database;
using Game.Misc.Loot;
using Game.Utils;
using Godot;
namespace Game.Ability
{
    public abstract class Spell : Pickable
    {
        private protected float percentDamage;
        private protected short manaCost;
        private protected ushort spellRange;
        private protected byte count;
        private protected bool casted;
        private protected bool ignoreArmor;
        private protected bool effectOnTarget;
        private protected bool requiresTarget;
        private protected Dictionary<string, ushort> attackTable;
        private protected Character caster = null;
        private protected Character target = null;
        private protected Speaker2D snd;
        public override void _Ready()
        {
            base._Ready();
            snd = GetNode<Speaker2D>("snd");
        }
        public override void Init(string worldName)
        {
            SetWorldType((WorldTypes)Enum.Parse(typeof(WorldTypes), worldName.ToUpper().Replace(" ", "_")));
            SetWorldName(worldName);
            SetName(GetWorldName());
            Dictionary<string, string> spellData = SpellDB.GetSpellData(worldName);
            SetPickableSubType((WorldTypes)Enum.Parse(typeof(WorldTypes), spellData["subType"].ToUpper()));
            icon = (AtlasTexture)GD.Load($"res://asset/img/icon/spell/{spellData[nameof(icon)]}_icon.res");
            level = short.Parse(spellData[nameof(level)]);
            spellRange = ushort.Parse(spellData[nameof(spellRange)]);
            cooldown = short.Parse(spellData[nameof(cooldown)]);
            percentDamage = float.Parse(spellData[nameof(percentDamage)]);
            ignoreArmor = bool.Parse(spellData[nameof(ignoreArmor)]);
            effectOnTarget = bool.Parse(spellData[nameof(effectOnTarget)]);
            requiresTarget = bool.Parse(spellData[nameof(requiresTarget)]);
            attackTable = Stats.attackTable[(spellRange > Stats.WEAPON_RANGE_MELEE) ? "RANGED" : "MELEE"];
            goldWorth = Stats.GetSpellWorthCost(level);
            manaCost = Stats.GetSpellManaCost(level);
            pickableDescription = $"-Mana Cost: {manaCost}\n{((spellRange == 0) ? "" : $"-Range: {spellRange}\n")}" +
                $"-Cooldown: {cooldown} sec.\n-Level: {level}" +
                $"\n\n-{spellData["description"]}";
        }
        public override void GetPickable(Character character, bool addToBag)
        {
            base.GetPickable(character, addToBag);
            caster = character;
        }
        public override void UnMake()
        {
            SetProcess(false);
            Player player = caster as Player;
            if (player != null)
            {
                player.RemoveFromSpellQueue(this);
                Control osb = player.GetMenu().GetNode<Control>("c/osb");
                if (IsInGroup(osb.GetInstanceId().ToString()))
                {
                    RemoveFromGroup(osb.GetInstanceId().ToString());
                }
                if (player.GetSpell() == this)
                {
                    player.SetCurrentSpell(null);
                }
            }
            base.UnMake();
        }
        public override float GetTimeLeft()
        {
            Timer timer = GetNode<Timer>("timer");
            return (count > 0) ? GetDuration() - (count * timer.GetWaitTime() - base.GetTimeLeft()) : base.GetTimeLeft();
        }
        public override void _OnTimerTimeout()
        {
            UnMake();
        }
        public virtual bool Casted()
        {
            return casted;
        }
        public virtual float Cast()
        {
            if (casted)
            {
                return 1.0f;
            }
            casted = true;
            if (!loaded)
            {
                caster.SetMana((short) - manaCost);
            }
            if ((GetPickableSubType() == WorldTypes.CASTING ||
                    GetPickableSubType() == WorldTypes.DAMAGE_MODIFIER) &&
                GetWorldType() != WorldTypes.EXPLOSIVE_TRAP)
            {
                SpellEffect spellEffect = SetEffect();
                if (GetWorldType() == WorldTypes.FRENZY)
                {
                    Connect(nameof(Unmake), spellEffect, nameof(SpellEffect._OnTimerTimeout));
                }
            }
            if (GetDuration() == 0.0f && GetPickableSubType() != WorldTypes.CHOOSE_AREA_EFFECT)
            {
                SetName(GetInstanceId().ToString());
                SetTime(2.5f, false);
            }
            return percentDamage;
        }
        public virtual async void ConfigureSpell()
        {
            caster.SetCurrentSpell(this);
            switch (GetPickableSubType())
            {
                case WorldTypes.DAMAGE_MODIFIER:
                    caster.weaponRange = spellRange;
                    break;
                case WorldTypes.CASTING:
                    PrepSight();
                    SetGlobalPosition(caster.GetGlobalPosition());
                    AnimationPlayer casterAnim = caster.GetNode<AnimationPlayer>("anim");
                    if (casterAnim.GetCurrentAnimation().Equals("cast"))
                    {
                        await ToSignal(casterAnim, "animation_finished");
                    }
                    casterAnim.Play("cast", -1, caster.animSpeed);
                    break;
            }
        }
        public virtual void ConfigureSnd() { }
        public Dictionary<string, ushort> GetAttackTable()
        {
            return attackTable;
        }
        public short GetManaCost()
        {
            return manaCost;
        }
        public ushort GetSpellRange()
        {
            return spellRange;
        }
        public bool RequiresTarget()
        {
            return requiresTarget;
        }
        public bool IsIgnoreArmor()
        {
            return ignoreArmor;
        }
        public float GetCoolDownTime()
        {
            return cooldown;
        }
        public void SetTime(float time, bool setDuration = true)
        {
            Timer timer = GetNode<Timer>("timer");
            if (!loaded)
            {
                timer.SetWaitTime(time);
            }
            if (setDuration)
            {
                SetDuration((count > 0) ? time * count : time);
            }
            timer.Start();
        }
        public void SetCount(byte count)
        {
            if (!loaded)
            {
                this.count = count;
            }
        }
        private protected void StunUnit(Character character, bool stun)
        {
            character.SetProcess(false);
            if (stun)
            {
                character.GetNode<Timer>("timer").Stop();
                character.GetNode<AnimationPlayer>("anim").Stop();
                character.GetNode<Sprite>("img").SetFrame(0);
            }
            else
            {
                character.SetState(character.GetState(), true);
            }
        }
        private protected void PrepSight()
        {
            Node sight = GetNode("sight");
            sight.Disconnect("area_entered", this, nameof(_OnSightAreaEntered));
            sight.Disconnect("area_exited", this, nameof(_OnSightAreaExited));
            sight.SetBlockSignals(false);
            sight.GetNode<CollisionShape2D>("distance").SetDisabled(false);
        }
        private protected SpellEffect SetEffect()
        {
            SpellEffect spellEffect = PickableFactory.GetMakeSpellEffect(GetWorldName());
            ((effectOnTarget) ? target : caster).AddChild(spellEffect);
            spellEffect.SetOwner((effectOnTarget) ? target : caster);
            spellEffect.Init((effectOnTarget) ? target : caster);
            spellEffect.OnHit(this);
            return spellEffect;
        }
    }
}