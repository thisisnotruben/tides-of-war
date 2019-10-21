using Godot;

namespace Game.Misc.Light
{
    public class TorchLight : GameLight
    {
        public override void Start()
        {
            if (GetNode<VisibilityNotifier2D>("visibility").IsOnScreen())
            {
                GetNode<AnimationPlayer>("anim").Play("torch");
                foreach (Node node in GetChildren())
                {
                    Emit(node, true);
                    foreach (Node childNode in node.GetChildren())
                    {
                        Emit(childNode, true);
                    }
                }
            }
        }
        public override void Stop()
        {
            GetNode<AnimationPlayer>("anim").Stop();
            GetNode<Node2D>("light").Hide();
            if (!GetNode<VisibilityNotifier2D>("visibility").IsOnScreen())
            {
                foreach (Node node in GetChildren())
                {
                    Emit(node, false);
                    foreach (Node childNode in node.GetChildren())
                    {
                        Emit(childNode, false);
                    }
                }
            }
        }
        public void Emit(Node node, bool set)
        {
            if (node is Particles2D)
            {
                Particles2D particles2D = (Particles2D)node;
                particles2D.SetEmitting(set);
            }
        }
    }
}