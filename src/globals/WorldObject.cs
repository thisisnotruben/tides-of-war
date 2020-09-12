using System;
using Godot;
namespace Game
{
	public abstract class WorldObject : Node2D
	{
		public enum WorldTypes : byte
		{
			FRENZY,
			CONCUSSIVE_SHOT,
			EXPLOSIVE_ARROW,
			PIERCING_SHOT,
			VOLLEY,
			EXPLOSIVE_TRAP,
			FIREBALL,
			SHADOW_BOLT,
			FROST_BOLT,
			SIPHON_MANA,
			SLOW,
			MIND_BLAST,
			CASTING,
			DAMAGE_MODIFIER,
			CHOOSE_AREA_EFFECT,
			ARROW_HIT_ARMOR,
			ARROW_PASS
		}
		public WorldTypes worldType { get; private protected set; }
		public String worldName { get; private protected set; }
		public bool loaded;
		[Signal] public delegate void Unmake();
	}
}