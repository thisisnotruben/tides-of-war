using System;
using Godot;
namespace Game
{
	public abstract class WorldObject : Node2D
	{
		public enum WorldTypes : byte
		{
			FRENZY, EXPLOSIVE_TRAP, CASTING,
			DAMAGE_MODIFIER, CHOOSE_AREA_EFFECT,
		}
		public WorldTypes worldType { get; private protected set; }
		public String worldName { get; private protected set; }
		public bool loaded;
		[Signal] public delegate void Unmake();
	}
}