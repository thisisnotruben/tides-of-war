using Godot;
using Game.Actor;

namespace Game.Ui
{
    public class MoveCursor : Node2D
    {
        public void _OnAnimFinished(string AnimName)
        {
            Delete();
        }
        public void AddToMap(Player originator, Vector2 globalTilePosition)
        {
            originator.Connect(nameof(Player.PosChanged), this, nameof(Delete));
            Globals.GetMap().GetNode("ground").AddChild(this);
            SetGlobalPosition(globalTilePosition);
        }
        public void Delete()
        {
            QueueFree();
        }
    }
}