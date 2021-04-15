using Godot;
using Game.Sound;
using Game.Database;
using Game.DialogicAdapter;
namespace Game
{
	public class Globals : Node
	{
		public const string HUD_SHORTCUT_GROUP = "HUD-shortcut",
			SAVE_GROUP = "save",
			LIGHT_GROUP = "light",
			GRAVE_GROUP = "graveSite";

		public static readonly SoundPlayer soundPlayer = new SoundPlayer();
		public static readonly CooldownMaster cooldownMaster = new CooldownMaster();

		public static readonly AreaEffectDB areaEffectDB = new AreaEffectDB(PathManager.areaEffect);
		public static readonly ContentDB contentDB = new ContentDB();
		public static readonly ImageDB imageDB = new ImageDB(PathManager.image);
		public static readonly ItemDB itemDB = new ItemDB(PathManager.item);
		public static readonly LandMineDB landMineDB = new LandMineDB(PathManager.landMine);
		public static readonly MissileSpellDB missileSpellDB = new MissileSpellDB(PathManager.missileSpell);
		public static readonly ModDB modDB = new ModDB(PathManager.modifier);
		public static readonly QuestDB questDB = new QuestDB();
		public static readonly SpellDB spellDB = new SpellDB(PathManager.spell);
		public static readonly SpellEffectDB spellEffectDB = new SpellEffectDB(PathManager.spellEffectDir);
		public static readonly UnitDB unitDB = new UnitDB();
		public static readonly UseDB useDB = new UseDB(PathManager.use);

		public static readonly Dialogic dialogic = new Dialogic();

		public override void _Ready()
		{
			AddChild(soundPlayer);
			AddChild(cooldownMaster);
		}

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