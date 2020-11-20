using Godot;
namespace Game.Database
{
	public class SpellEffectDB : AbstractDB<PackedScene>
	{

		public static readonly SpellEffectDB Instance = new SpellEffectDB();
		public static void Init() { }

		public SpellEffectDB() : base(PathManager.spellEffectDir) { }
		public override void LoadData(string path)
		{
			Directory directory = new Directory();
			if (!directory.DirExists(path))
			{
				return;
			}

			directory.Open(path);
			directory.ListDirBegin(true, true);

			string masterDirName = "master",
				resourceName = directory.GetNext();

			while (!resourceName.Empty())
			{
				if (directory.CurrentIsDir() && !resourceName.Equals(masterDirName))
				{
					LoadData(path.PlusFile(resourceName));
				}
				else if (resourceName.Extension().Equals(PathManager.sceneExt))
				{
					data[resourceName.BaseName()] = GD.Load<PackedScene>(path.PlusFile(resourceName));
				}
				resourceName = directory.GetNext();
			}
			directory.ListDirEnd();
		}
		public override PackedScene GetData(string dataName)
		{
			return base.GetData(dataName);
		}
		public override bool HasData(string dataName)
		{
			return SpellDB.Instance.HasData(dataName)
				? data.ContainsKey(SpellDB.Instance.GetData(dataName).spellEffect)
				: false;
		}
	}
}