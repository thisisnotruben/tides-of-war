using Godot;
using Game.Actor;
using Game.Ui;
using Game.Database;
using System;
using System.Collections.Generic;

namespace Game.Misc.Loot
{
    public class Item : Pickable
    {
        public const float MAX_DURABILITY = 1.0f;
        private float durability = MAX_DURABILITY;
        private short minValue;
        private short maxValue;
        private short value;
        [Signal]
        public delegate void EquipItem(Item item, bool equip);

        public void Init(string worldName)
        {
            Dictionary<string, string> itemData = ItemDB.GetItemData(worldName);
            SetWorldName(worldName);
            GD.Print(GetWorldName());
            SetWorldType((WorldTypes)Enum.Parse(typeof(WorldTypes), itemData["type"].ToUpper()));
            icon = (AtlasTexture)GD.Load($"res://asset/img/icon/{itemData["type"].ToLower()}/{itemData[nameof(icon)]}_icon.res");
            level = short.Parse(itemData[nameof(level)]);
            if (GetWorldType() == WorldTypes.WEAPON || GetWorldType() == WorldTypes.POTION)
            {
                SetPickableSubType((WorldTypes)Enum.Parse(typeof(WorldTypes), itemData["subType"].ToUpper()));
            }
            Tuple<short, short> itemStats = Stats.GetItemStats(GetLevel(), GetWorldType(), GetPickableSubType());
            goldWorth = Stats.GetItemGoldWorth(GetLevel(), GetWorldType(), durability);
            if (GetWorldType() == WorldTypes.POTION)
            {
                SetDuration(120.0f);
                stackSize = 5;
                switch (GetPickableSubType())
                {
                    case WorldTypes.HEALING:
                    case WorldTypes.MANA:
                        minValue = itemStats.Item1;
                        maxValue = itemStats.Item2;
                        break;
                    default:
                        value = itemStats.Item1;
                        break;
                }
            }
            else
            {
                switch (GetWorldType())
                {
                    case WorldTypes.WEAPON:
                    case WorldTypes.FOOD:
                        minValue = itemStats.Item1;
                        maxValue = itemStats.Item2;
                        break;
                    case WorldTypes.ARMOR:
                        value = itemStats.Item1;
                        break;
                }
            }
            switch (GetWorldType())
            {
                case WorldTypes.WEAPON:
                    pickableDescription = $"-Damage: {minValue} - {maxValue}";
                    break;
                case WorldTypes.ARMOR:
                    pickableDescription = $"-Armor: {value}";
                    break;
                case WorldTypes.FOOD:
                    pickableDescription = $"-Restores {minValue} - {maxValue} health";
                    break;
                default:
                    pickableDescription = $"-Grants {value} {GetPickableSubType().ToString().ToLower()}\nfor {GetDuration().ToString("0.00")} seconds.";
                    switch (GetPickableSubType())
                    {
                        case WorldTypes.HEALING:
                            pickableDescription = $"-Restores {minValue} - {maxValue} health";
                            break;
                        case WorldTypes.MANA:
                            pickableDescription = $"-Restores {minValue} - {maxValue} mana";
                            break;
                    }
                    break;
            }
            pickableDescription += $"\n-Gold: {goldWorth}";
            switch (GetWorldType())
            {
                case WorldTypes.WEAPON:
                case WorldTypes.ARMOR:
                    pickableDescription += $"\n-Durability: {durability.ToString("P")}";
                    break;
            }
        }
        public override void _OnTimerTimeout()
        {
            if (GetOwner() is Character)
            {
                ConfigureBuff((Character)GetOwner(), true);
            }
        }
        public void Equip()
        {
            EmitSignal(nameof(EquipItem), this, true);
        }
        public void Unequip()
        {
            EmitSignal(nameof(EquipItem), this, false);
        }
        public short GetValue()
        {
            return value;
        }
        public float GetDurability()
        {
            return durability;
        }
        public Tuple<short, short> GetValues()
        {
            return new Tuple<short, short>(minValue, maxValue);
        }
        public void Consume(Character character, float seek)
        {
            EmitSignal(nameof(PickableExchanged), this, false);
            GD.Randomize();
            short amount = (short)Math.Round(GD.RandRange((double)minValue, (double)maxValue));
            switch (GetPickableSubType())
            {
                case WorldTypes.HEALING:
                    character.SetHp(amount);
                    break;
                case WorldTypes.MANA:
                    character.SetMana(amount);
                    break;
                default:
                    if (GetWorldType() == WorldTypes.FOOD)
                    {
                        UnMake();
                    }
                    break;
            }
            ConfigureBuff(character, false);
            if (GetTree().IsPaused())
            {
                character.buffs["pending"].Add(this);
            }
            else
            {
                character.SetBuff(new List<Item> { this }, seek);
            }
        }
        public void ConfigureBuff(Character character, bool expire = false)
        {
            if (character is Player)
            {
                Player player = (Player)character;
                if (IsConnected(nameof(SetInMenu), player.GetMenu(), nameof(InGameMenu._OnSetPickableInMenu)))
                {
                    Disconnect(nameof(SetInMenu), player.GetMenu(), nameof(InGameMenu._OnSetPickableInMenu));
                }
            }
            if (expire)
            {
                value *= -1;
            }
            double percent;
            switch (GetPickableSubType())
            {
                case WorldTypes.STAMINA:
                    percent = (double)character.hp / (double)character.hpMax;
                    character.hpMax += value;
                    character.SetHp((short)Math.Round(percent * (double)character.hpMax));
                    break;
                case WorldTypes.INTELLECT:
                    percent = (double)character.mana / (double)character.manaMax;
                    character.manaMax += value;
                    character.SetMana((short)Math.Round(percent * (double)character.manaMax));
                    break;
                case WorldTypes.AGILITY:
                    short regenAmount = Stats.GetModifiedRegen(character.GetLevel(),
                        Stats.GetMultiplier(character is Npc, character.GetNode<Sprite>("img").GetTexture().GetPath()));
                    character.regenTime = regenAmount;
                    if (character.GetState() != Character.States.ATTACKING)
                    {
                        character.SetTime(character.regenTime, false);
                    }
                    break;
                case WorldTypes.STRENGTH:
                    character.minDamage += value;
                    character.maxDamage += value;
                    break;
                case WorldTypes.DEFENSE:
                    character.armor += value;
                    break;
            }
            if (expire)
            {
                character.buffs["active"].Remove(this);
                UnMake();
            }
        }
        public void RepairItem(float amount)
        {
            durability += Math.Abs(amount);
            if (durability > 1.0f)
            {
                durability = 1.0f;
            }
            GD.Print("Not Implemented");
        }
        public void TakeDamage(bool byPass = false, float damageAmount = 0.1f)
        {
            GD.Randomize();
            if (byPass || GD.Randi() % 100 + 1 <= 10)
            {
                float oldDurability = durability;
                durability -= Math.Abs(damageAmount);
                if (durability > 1.0f)
                {
                    durability = 1.0f;
                }
                else if (durability < 0.0f)
                {
                    durability = 0.0f;
                }
                // use regex to find old durability string and replace with new durability string
                // if anything, this whole function needs rewriting
                if (durability >= 0.5f)
                {
                    goldWorth = Stats.GetItemGoldWorth(GetLevel(), GetWorldType(), durability);
                    switch (GetWorldType())
                    {
                        case WorldTypes.WEAPON:
                            break;
                        case WorldTypes.ARMOR:
                            break;
                    }
                }
            }
            GD.Print("Not Implemented");
        }
    }
}