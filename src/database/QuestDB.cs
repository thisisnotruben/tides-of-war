using Godot;

namespace Game.Database
{
    public static class QuestDB
    {
        private static string DB_PATH = "res://src/Database/QuestDB.xml";
        private static readonly XMLParser xMLParser = new XMLParser();
    }
}