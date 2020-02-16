using Godot;
namespace Game.Light
{
    public class TorchLight : GameLight
    {
        private Light2D light;
        private VisibilityNotifier2D visibility;

        public override void _Ready()
        {
            light = GetNode<Light2D>("light");
            visibility = GetNode<VisibilityNotifier2D>("visibility");
        }

        public override void SetIntensity(bool full, float min=0.8f, float max=1.0f)
        {
            float choice = (full) ? max : min;
            if (visibility.IsOnScreen())
            {
                Tween tween = GetNode<Tween>("tween");
                tween.InterpolateProperty(light, "energy", light.Energy,
                    choice, 1.0f, Tween.TransitionType.Linear, Tween.EaseType.In);
                tween.Start();
            }
            else
            {
                light.Energy = choice;
            }
        }
        public override void Start()
        {
            if (visibility.IsOnScreen())
            {
                GetNode<AnimationPlayer>("anim").Play("torch");
                light.Enabled = true;
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
            if (!visibility.IsOnScreen())
            {
                GetNode<AnimationPlayer>("anim").Stop();
                light.Enabled = false;
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
            Particles2D particles2D = node as Particles2D;
            if (particles2D != null)
            {
                particles2D.Emitting = set;
            }
        }
    }
}