/*
Spells only need the string worldName || WorldType to init
Item needs ???

 */
using Game.Misc.Loot;
using Game.Spell;
using Godot;
namespace Game.Database
{
    public static class PickableFactory
    {
        public static Spell.Spell GetMakeSpell(string worldName)
        {
            PackedScene packedScene = (PackedScene)GD.Load("res://src/spell/spell.tscn");
            Node spellNode = packedScene.Instance();
            spellNode.SetScript(GD.Load($"res://src/spell/spells/{worldName.Replace(" ", "")}.cs"));
            Spell.Spell spell = (Spell.Spell)spellNode;
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