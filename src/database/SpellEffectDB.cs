using Godot;
namespace Game.Database
{
	public class SpellEffectDB : AbstractDB<PackedScene>
	{
		public SpellEffectDB(string path) : base(path) { }
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
			return Globals.spellDB.HasData(dataName)
				? data.ContainsKey(Globals.spellDB.GetData(dataName).spellEffect)
				: false;
		}
	}
}