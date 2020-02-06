using Game.Ability;
using Game.Loot;
using Godot;
namespace Game.Database
{
    public static class PickableFactory
    {
        public static Spell GetMakeSpell(string worldName)
        {
            PackedScene spellScene = (PackedScene)GD.Load($"res://src/spell/spells/{GetFileFormat(worldName)}.tscn");
            Spell spell = (Spell)spellScene.Instance();
            spell.Init(worldName);
            return spell;
        }
        public static Item GetMakeItem(string worldName)
        {
            PackedScene itemScene = (PackedScene)GD.Load("res://src/loot/item.tscn");
            Item item = (Item)itemScene.Instance();
            item.Init(worldName);
            return item;
        }
        public static SpellEffect GetMakeSpellEffect(string worldName)
        {
            PackedScene spellEffectScene = (PackedScene)GD.Load($"res://src/spell/spell_effects/{GetFileFormat(worldName)}.tscn");
            SpellEffect spellEffect = (SpellEffect)spellEffectScene.Instance();
            return spellEffect;
        }
        private static string GetFileFormat(string worldName)
        {
            return worldName.Replace(" ", "_").ToLower();
        }
    }
}