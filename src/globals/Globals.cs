using System.Collections.Generic;
using Godot;
namespace Game
{
	public class Globals : Node
	{
		public static readonly Dictionary<string, int> WEAPON_TYPE = new Dictionary<string, int>
		{ { "AXE", 5 },
			{ "CLUB", 3 },
			{ "DAGGER", 3 },
			{ "SWORD", 8 },
			{ "BOW", 5 },
			{ "ARROW_HIT_ARMOR", 5 },
			{ "ARROW_PASS", 6 },
			{ "SWING_SMALL", 3 },
			{ "SWING_MEDIUM", 3 },
			{ "SWING_LARGE", 3 },
			{ "SWING_VERY_LARGE", 3 },
			{ "BLOCK_METAL_METAL", 5 },
			{ "BLOCK_METAL_WOOD", 3 },
			{ "BLOCK_WOOD_WOOD", 3 },
		};
		public static readonly Dictionary<string, int> Collision = new Dictionary<string, int>
		{ { "WORLD", 1 },
			{ "CHARACTERS", 2 },
			{ "DEAD_CHARACTERS", 3 },
			{ "COMBUSTIBLE", 8 }
		};
		public const string HUD_SHORTCUT_GROUP = "HUD-shortcut";
		public const string SAVE_GROUP = "save";
	}
}