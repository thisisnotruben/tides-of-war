using Godot;
using Game.Misc.Loot;
using Game.Actor;
using Game.Database;
using System;
using System.Collections.Generic;

namespace Game.Spell
{
    public abstract class Spell : Pickable
    {
        private short manaCost;
        private bool ignoreArmor;
        private protected short spellRange;
        private short count;
        private protected Character caster;
        private protected bool casted;
        private protected bool effectOnTarget;
        private protected bool requiresTarget;
        private protected Dictionary<string, ushort> attackTable;

        public override void GetPickable(Character character, bool addToBag)
        {
            base.GetPickable(character, addToBag);
            caster = character;
        }
        public virtual Dictionary<string, Dictionary<string, ushort>> GetAttackTable()
        {
            return Stats.attackTable;
        }
        public short GetManaCost()
        {
            return manaCost;
        }
        public short GetSpellRange()
        {
            return spellRange;
        }
        public bool RequiresTarget()
        {
            return requiresTarget;
        }
        public virtual bool IsIgnoreArmor()
        {
            return ignoreArmor;
        }
        public float GetCoolDownTime()
        {
            return cooldown;
        }
        public override void UnMake()
        {
            _OnTimerTimeout();
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
                    player.SetSpell(null);
                }
            }
            base.UnMake();
        }
        public void SetTime(float time, bool setDuration = true)
        {
            if (!loaded)
            {
                GetNode<Timer>("timer").SetWaitTime(time);
            }
            if (setDuration)
            {
                SetDuration((count > 0) ? time * count : time);
            }
            GetNode<Timer>("timer").Start();
        }
        public override float GetTimeLeft()
        {
            Timer timer = GetNode<Timer>("timer");
            return (count > 0) ? GetDuration() - (count * timer.GetWaitTime() - base.GetTimeLeft()) : base.GetTimeLeft();
        }
        public void SetCount(short count)
        {
            if (!loaded)
            {
                this.count = count;
            }
        }
        public abstract float Cast();
        public abstract bool Casted();
        public override void _OnTimerTimeout()
        {
        }
        public override void Make()
        {
            SetWorldName(Enum.GetName(typeof(WorldTypes), GetWorldType()).ToLower());
            level = ItemDB.GetItemLevel(GetWorldName());
            spellRange = SpellDB.GetSpellRange(GetWorldName());
            cooldown = SpellDB.GetSpellCooldown(GetWorldName());
            goldWorth = Stats.GetSpellWorthCost(GetLevel());
            manaCost = Stats.GetSpellManaCost(GetLevel());
            icon = (AtlasTexture)GD.Load(string.Format("res://asset/img/icon/spell/{0}_icon.res", SpellDB.GetSpellIconID(GetWorldName())));
            attackTable = (spellRange > Stats.WEAPON_RANGE_MELEE) ? Stats.attackTable["RANGED"] : Stats.attackTable["MELEE"];
            pickableDescription = string.Format("-Mana Cost: {0}\n-Range: {1}\n-Cooldown: {2}\n-Level: {3}\n\n-{4}",
                manaCost, spellRange, string.Format("{0} sec.", cooldown), GetLevel(), SpellDB.GetSpellDescription(GetWorldName()));
            
            GD.Print("Not Implemented");
        }
        public void ConfigureSpell()
        {
            GD.Print("Not Implemented");
        }
    }
}