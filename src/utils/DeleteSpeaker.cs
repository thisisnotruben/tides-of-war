using Godot;
namespace Game.Utils
{
    public class DeleteSpeaker : Node
    {
        public void Delete(Node speaker)
        {
            speaker.QueueFree();
        }
    }
}