using System.Collections.Generic;
using Game.Ability;
using Game.Loot;
using Game.ItemPoto;
using Godot;
namespace Game.Database
{
	public static class PickableFactory
	{
		private static readonly Dictionary<string, Spell> cachedSpells;
		private static readonly Dictionary<string, SpellEffect> cachedSpellEffects;

		static PickableFactory()
		{
			cachedSpells = new Dictionary<string, Spell>();
			cachedSpellEffects = new Dictionary<string, SpellEffect>();
		}
		public static void LoadSpells()
		{
			// clear out cache
			cachedSpells.Clear();
			cachedSpellEffects.Clear();
			// load all spells
			foreach (string spellName in SpellDB.GetSpellNames())
			{
				cachedSpells.Add(spellName, GetMakeSpell(spellName));
				cachedSpellEffects.Add(spellName, GetMakeSpellEffect(spellName));
			}
		}
		public static Spell GetMakeSpell(string worldName)
		{
			Spell spell;
			if (cachedSpells.ContainsKey(worldName))
			{
				spell = cachedSpells[worldName];
			}
			else
			{
				PackedScene spellScene = (PackedScene)GD.Load($"res://src/spell/spells/{GetFileFormat(worldName)}.tscn");
				spell = (Spell)spellScene.Instance();
				spell.Init(worldName);
			}
			return spell;
		}
		public static SpellEffect GetMakeSpellEffect(string worldName)
		{
			SpellEffect spellEffect;
			if (cachedSpellEffects.ContainsKey(worldName))
			{
				spellEffect = cachedSpellEffects[worldName];
			}
			else
			{
				PackedScene spellEffectScene = (PackedScene)GD.Load($"res://src/spell/spell_effects/{GetFileFormat(worldName)}.tscn");
				spellEffect = (SpellEffect)spellEffectScene.Instance();
			}
			return spellEffect;
		}
		private static string GetFileFormat(string worldName) { return worldName.Replace(" ", "_").ToLower(); }
	}
}