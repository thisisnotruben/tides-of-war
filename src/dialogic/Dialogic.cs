using Godot;
using GC = Godot.Collections;
namespace Game.DialogicAdapter
{
	public class Dialogic : Node
	{
		private const string dialogScenePath = "res://addons/dialogic/Dialog.tscn";
		private Script dialogicSingleton = GD.Load<Script>("res://addons/dialogic/Other/DialogicClass.gd");

		public Control Start(string timeline, bool resetSaves = true, string dialogScenePath = dialogScenePath, bool debugMode = false)
		{
			return (Control)dialogicSingleton.Call("start", timeline, resetSaves, dialogScenePath, debugMode);
		}
		public Control StartFromSave(string initialTimeline, string dialogScenePath = dialogScenePath, bool debugMode = false)
		{
			return (Control)dialogicSingleton.Call("start_from_save", initialTimeline, dialogScenePath, debugMode);
		}
		public GC.Dictionary GetDefaultDefinitions() { return (GC.Dictionary)dialogicSingleton.Call("get_default_definitions"); }
		public GC.Dictionary GetDefinitions() { return (GC.Dictionary)dialogicSingleton.Call("get_definitions"); }
		public void SaveDefinitions() { dialogicSingleton.Call("save_definitions"); }
		public void ResetSaves() { dialogicSingleton.Call("reset_saves"); }
		public string GetVariable(string name) { return dialogicSingleton.Call("get_variable", name).ToString(); }
		public void SetVariable(string name, string value) { dialogicSingleton.Call("set_variable", name, value); }
		public GC.Dictionary GetGlossary(string name) { return (GC.Dictionary)dialogicSingleton.Call("get_glossary", name); }
		public void SetGlossary(string name, string title, string text, string extra) { dialogicSingleton.Call("set_glossary", name, title, text, extra); }
		public string GetCurrentTimeline() { return dialogicSingleton.Call("get_current_timeline").ToString(); }
	}
}