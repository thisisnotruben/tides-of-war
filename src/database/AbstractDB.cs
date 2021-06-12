using Godot;
using System.Collections.Generic;
namespace Game.Database
{
	public abstract class AbstractDB<T>
	{
		public Dictionary<string, T> data = new Dictionary<string, T>();

		public AbstractDB(string path) { LoadData(path); }
		public AbstractDB() { }

		public abstract void LoadData(string path);
		public virtual T GetData(string dataName) { return data[dataName]; }
		public virtual bool HasData(string dataName) { return data.ContainsKey(dataName); }
		public Godot.Collections.Dictionary LoadJson(string path)
		{
			File file = new File();
			file.Open(path, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			return (Godot.Collections.Dictionary)jSONParseResult.Result;
		}
	}
}