using System;
using System.Collections.Generic;
using Game.Actor;
using Game.Database;
using Game.Loot;
using Game.Utils;
using Godot;
namespace Game.Ability
{
    public abstract class Spell : Pickable
    {
        public float percentDamage { get; private protected set; }
        public int manaCost { get; private protected set; }
        public int spellRange { get; private protected set; }
        private byte _count;
        public byte count
        {
            get
            {
                return _count;
            }
            private protected set
            {
                if (!loaded)
                {
                    _count = value;
                }
            }
        }
        public bool casted { get; private protected set; }
        public bool ignoreArmor { get; private protected set; }
        public bool effectOnTarget { get; private protected set; }
        public bool requiresTarget { get; private protected set; }
        private protected Dictionary<string, int> attackTable;
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
            worldType = (WorldTypes)Enum.Parse(typeof(WorldTypes), worldName.ToUpper().Replace(" ", "_"));
            this.worldName = worldName;
            Name = worldName;
            SpellDB.SpellNode spellData = SpellDB.GetSpellData(worldName);
            spellRange = spellData.spellRange;
            percentDamage = spellData.percentDamage;
            ignoreArmor = spellData.ignoreArmor;
            effectOnTarget = spellData.effectOnTarget;
            requiresTarget = spellData.requiresTarget;
            attackTable = Stats.attackTable[(spellRange > Stats.WEAPON_RANGE_MELEE) ? "RANGED" : "MELEE"];
            manaCost = Stats.GetSpellManaCost(spellData.level);
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
                if (player.spell == this)
                {
                    player.SetCurrentSpell(null);
                }
            }
            base.UnMake();
        }
        public override float GetTimeLeft()
        {
            Timer timer = GetNode<Timer>("timer");
            return (count > 0) ? duration - (count * timer.WaitTime - base.GetTimeLeft()) : base.GetTimeLeft();
        }
        public override void _OnTimerTimeout()
        {
            UnMake();
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
                caster.mana = -manaCost;
            }
            if ((subType == WorldTypes.CASTING ||
                    subType == WorldTypes.DAMAGE_MODIFIER) &&
                worldType != WorldTypes.EXPLOSIVE_TRAP)
            {
                SpellEffect spellEffect = SetEffect();
                if (worldType == WorldTypes.FRENZY)
                {
                    Connect(nameof(Unmake), spellEffect, nameof(SpellEffect._OnTimerTimeout));
                }
            }
            if (duration == 0.0f && subType != WorldTypes.CHOOSE_AREA_EFFECT)
            {
                Name = GetInstanceId().ToString();
                SetTime(2.5f, false);
            }
            return percentDamage;
        }
        public virtual async void ConfigureSpell()
        {
            caster.SetCurrentSpell(this);
            switch (subType)
            {
                case WorldTypes.DAMAGE_MODIFIER:
                    caster.weaponRange = spellRange;
                    break;
                case WorldTypes.CASTING:
                    PrepSight();
                    GlobalPosition = caster.GlobalPosition;
                    AnimationPlayer casterAnim = caster.GetNode<AnimationPlayer>("anim");
                    if (casterAnim.CurrentAnimation.Equals("casting"))
                    {
                        await ToSignal(casterAnim, "animation_finished");
                    }
                    casterAnim.Play("casting", -1, caster.animSpeed);
                    break;
            }
        }
        public virtual void ConfigureSnd() { }
        public Dictionary<string, int> GetAttackTable()
        {
            return attackTable;
        }
        public void SetTime(float time, bool setDuration = true)
        {
            Timer timer = GetNode<Timer>("timer");
            if (!loaded)
            {
                timer.WaitTime = time;
            }
            if (setDuration)
            {
                duration = (count > 0) ? time * count : time;
            }
            timer.Start();
        }
        private protected void StunUnit(Character character, bool stun)
        {
            character.SetProcess(false);
            if (stun)
            {
                character.GetNode<Timer>("timer").Stop();
                character.GetNode<AnimationPlayer>("anim").Stop();
                character.GetNode<Sprite>("img").Frame = 0;
            }
            else
            {
                character.SetState(character.state, true);
            }
        }
        private protected void PrepSight()
        {
            Node sight = GetNode("sight");
            sight.SetBlockSignals(false);
            sight.GetNode<CollisionShape2D>("distance").Disabled = false;
        }
        private protected SpellEffect SetEffect()
        {
            SpellEffect spellEffect = PickableFactory.GetMakeSpellEffect(worldName);
            ((effectOnTarget) ? target : caster).AddChild(spellEffect);
            spellEffect.Owner = (effectOnTarget) ? target : caster;
            spellEffect.Init((effectOnTarget) ? target : caster);
            spellEffect.OnHit(this);
            return spellEffect;
        }
    }
}