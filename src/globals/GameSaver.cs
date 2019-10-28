using Godot;
using System.Collections.Generic;

namespace Game
{
    public class GameSaver : Node
    {
        public void SaveGame(string savePath)
        {
            File file = new File();
            file.Open(savePath, (int)File.ModeFlags.Write);
            Dictionary<string, string> saveDict = new Dictionary<string, string>();
            saveDict.Add("scene", Globals.GetMap().GetFilename());
            foreach (ISaveable node in GetTree().GetNodesInGroup(Globals.SAVE_GROUP))
            {
                saveDict[((Node)node).GetPath()] = JSON.Print(node.GetSaveData());
            }
            file.StoreLine(JSON.Print(saveDict));
            file.Close();
        }
    }
}