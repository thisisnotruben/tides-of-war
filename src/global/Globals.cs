using Godot;
using Game.Sound;
namespace Game
{
	public class Globals : Node
	{
		public const string HUD_SHORTCUT_GROUP = "HUD-shortcut",
							SAVE_GROUP = "save";
		public static readonly SoundPlayer soundPlayer = new SoundPlayer();

		public override void _Ready() { AddChild(soundPlayer); }
	}
}