using System;
using Godot;
namespace Game
{
	public abstract class WorldObject : Node2D
	{
		public enum WorldTypes : byte
		{
			// Spell WorldTypes
			CLEAVE,
			BASH,
			HEMORRHAGE,
			INTIMIDATING_SHOUT,
			FRENZY,
			STOMP,
			FORTIFY,
			OVERPOWER,
			DEVASTATE,
			SEARING_ARROW,
			CONCUSSIVE_SHOT,
			SNIPER_SHOT,
			EXPLOSIVE_ARROW,
			PRECISE_SHOT,
			PIERCING_SHOT,
			VOLLEY,
			STINGING_SHOT,
			EXPLOSIVE_TRAP,
			FIREBALL,
			SHADOW_BOLT,
			FROST_BOLT,
			DIVINE_HEAL,
			SIPHON_MANA,
			HASTE,
			SLOW,
			ARCANE_BOLT,
			MIND_BLAST,
			METEOR,
			// Spell Catagory WorldTypes
			CASTING,
			DAMAGE_MODIFIER,
			CHOOSE_AREA_EFFECT,
			// Item WorldTypes
			WEAPON,
			ARMOR,
			POTION,
			FOOD,
			MISC,
			// (Pickable SubTypes) Armor, Food, and Misc doesn't have a subType
			HEALING,
			MANA,
			STAMINA,
			INTELLECT,
			AGILITY,
			STRENGTH,
			DEFENSE,
			// Swing WorldTypes
			ARROW_HIT_ARMOR,
			ARROW_PASS
		}
		public WorldTypes worldType { get; private protected set; }
		public String worldName { get; private protected set; }
		public bool loaded = false;
		[Signal]
		public delegate void Unmake();
	}
}