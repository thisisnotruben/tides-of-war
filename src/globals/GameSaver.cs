using System.Collections.Generic;
using Godot;
namespace Game
{
    public class GameSaver : Node
    {
        public void SaveGame(string savePath)
        {
            File file = new File();
            file.Open(savePath, File.ModeFlags.Write);
            Dictionary<string, string> saveDict = new Dictionary<string, string>();
            saveDict.Add("scene", Globals.map.Filename);
            foreach (ISaveable node in GetTree().GetNodesInGroup(Globals.SAVE_GROUP))
            {
                saveDict[((Node)node).GetPath()] = JSON.Print(node.GetSaveData());
            }
            file.StoreLine(JSON.Print(saveDict));
            file.Close();
        }
    }
}