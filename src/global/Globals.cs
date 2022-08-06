using Godot;
using GC = Godot.Collections;
using Game.Ui;
using Game.Quest;
using Game.Audio;
using Game.Database;
namespace Game
{
	public class Globals : Node
	{
		public const string HUD_SHORTCUT_GROUP = "HUD-shortcut",
			SAVE_GROUP = "save",
			LIGHT_GROUP = "light",
			GRAVE_GROUP = "graveSite";

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
		public static readonly MapQuestItemDB mapQuestItemLootDB = new MapQuestItemDB();
		public static readonly MapQuestItemDropDB mapQuestItemDropDB = new MapQuestItemDropDB();

		public static readonly AudioPlayer audioPlayer = new AudioPlayer();
		public static readonly CooldownMaster cooldownMaster = new CooldownMaster();
		public static readonly QuestMaster questMaster = new QuestMaster();
		public static readonly SceneLoader sceneLoader = new SceneLoader();

		public override void _Ready()
		{
			AddChild(audioPlayer);
			AddChild(cooldownMaster);
			AddChild(sceneLoader);
			AddChild(questMaster);
		}
		// util function
		public static void TryLinkSignal(Godot.Object source, string sourceSignal, Godot.Object target, string targetMethod, bool link, GC.Array args = null)
		{
			if (source == null || target == null)
			{
				return;
			}

			if (link && !source.IsConnected(sourceSignal, target, targetMethod))
			{
				source.Connect(sourceSignal, target, targetMethod, args);
			}
			else if (!link && source.IsConnected(sourceSignal, target, targetMethod))
			{
				source.Disconnect(sourceSignal, target, targetMethod);
			}
		}
	}
}