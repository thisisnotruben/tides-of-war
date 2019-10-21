using Godot;

namespace Game.Database
{
    public class SpellDB : Node
    {
        public static short GetSpellRange(string worldName)
        {
            short spellRange = -1;
            GD.Print("Not implemented.");
            return spellRange;
        }
        public static short GetSpellCooldown(string worldName)
        {
            short cooldown = -1;
            GD.Print("Not implemented.");
            return cooldown;
        }
        public static string GetSpellDescription(string worldName)
        {
            string des = "";
            GD.Print("Not implemented.");
            return des;
        }
        public static int GetSpellIconID(string worldName)
        {
            int iconID = -1;
            GD.Print("Not implemented.");
            return iconID;
        }
    }
}