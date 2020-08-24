using Godot;
namespace Game.Actor
{
    public class PlayerCamera : Camera2D
    {
        private Vector2 cameraPos;
        public override void _Ready()
        {
            cameraPos = GetCameraPosition();
        }
        public void OnPlayerMove(bool moving)
        {
            if (!moving)
            {
                Align();
            }
        }
    }
}
