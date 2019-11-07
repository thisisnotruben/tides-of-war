/*
Spells only need the string worldName || WorldType to init
Item needs ???

 */
using Godot;
using Game.Spell;
using Game.Misc.Loot;

namespace Game.Database
{
    public static class PickableFactory
    {
        public static Spell.Spell GetMakeSpell(string worldName)
        {
            PackedScene packedScene = (PackedScene)GD.Load($"res://src/spell/spells/{worldName}");
            Spell.Spell spell = (Spell.Spell)packedScene.Instance();
            spell.Init(worldName);
            return spell;
        }
        public static Item GetMakeItem(string worldName)
        {
            PackedScene packedScene = (PackedScene)GD.Load("res://src/misc/loot/item.tscn");
            Item item = (Item)packedScene.Instance();
            item.Init(worldName);
            return item;
        }
    }
}