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
            spellNode.SetScript(GD.Load($"res://src/spell/spell_logics/{GetFileFormat(worldName)}.cs"));
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
        public static SpellEffect GetMakeSpellEffect(string worldName)
        {
            worldName = GetFileFormat(worldName);
            PackedScene spellEffectScene = (PackedScene)GD.Load($"res://src/spell/spell_effects/{worldName}.tscn");
            Node spellEffectNode = spellEffectScene.Instance();
            spellEffectNode.SetScript(GD.Load($"res://src/spell/spell_effect_controllers/{worldName}_effect.cs"));
            SpellEffect spellEffect = (SpellEffect)spellEffectNode;
            return spellEffect;
        }
        private static string GetFileFormat(string worldName)
        {
            return worldName.Replace(" ", "_").ToLower();
        }
    }
}