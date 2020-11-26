using Godot;
using Game.Sound;
namespace Game
{
	public class Globals : Node
	{
		public const string HUD_SHORTCUT_GROUP = "HUD-shortcut",
			SAVE_GROUP = "save",
			LIGHT_GROUP = "light",
			GRAVE_GROUP = "graveSite";
		public static readonly SoundPlayer soundPlayer = new SoundPlayer();

		public override void _Ready() { AddChild(soundPlayer); }

		// util function
		public static void TryLinkSignal(Godot.Object source, string sourceSignal, Godot.Object target, string targetMethod, bool link)
		{
			if (source == null || target == null)
			{
				return;
			}

			if (link && !source.IsConnected(sourceSignal, target, targetMethod))
			{
				source.Connect(sourceSignal, target, targetMethod);
			}
			else if (!link && source.IsConnected(sourceSignal, target, targetMethod))
			{
				source.Disconnect(sourceSignal, target, targetMethod);
			}
		}
	}
}