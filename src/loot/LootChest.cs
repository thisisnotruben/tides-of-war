using Godot;
using Game.Utils;
namespace Game.Loot
{
    public class LootChest : Node2D
    {
        public string pickableWorldName;
        private Tween tween;
        private AnimationPlayer animationPlayer;
        private Node2D select;
        private Speaker2D speaker2D;
        private const byte collectDistance = 2;

        public override void _Ready()
        {
            tween = GetNode<Tween>("tween");
            animationPlayer = GetNode<AnimationPlayer>("anim");
            select = GetNode<Node2D>("select");
            speaker2D = GetNode<Speaker2D>("speaker2d");
        }
        public void Collect()
        {
            Area2D area2D = GetNode<Area2D>("sight");
            area2D.Disconnect("area_entered", this, nameof(_OnSightAreaEntered));
            area2D.Disconnect("area_exited", this, nameof(_OnSightAreaExited));
            select.Disconnect("pressed", this, nameof(_OnSelectPressed));
            Globals.PlaySound("chest_collect", this, speaker2D);
            animationPlayer.Connect("animation_started", this, nameof(_OnAnimationStarted));
            animationPlayer.Queue("collect");
            GetNode<Timer>("timer").Start();
        }
        public void _OnAnimationStarted(string animName)
        {
            if (animName.Equals("collect"))
            {
                Sprite sprite = GetNode<Sprite>("img");
                tween.InterpolateProperty(sprite, ":frame", sprite.Frame, 0,
                animationPlayer.GetAnimation(animName).Length, Tween.TransitionType.Linear, Tween.EaseType.InOut);
                tween.Start();
            }
        }
        public void _OnTimerTimeout()
        {
            QueueFree();
        }
        public virtual void _OnSightAreaEntered(Area2D area2D)
        {
            // TODO: what if already inside? it wouldn't activate
            // maybe a signal to check if player moved, hmmm...
            // if (collectDistance >= Globals.map.getAPath(Globals.player.GlobalPosition, GlobalPosition).Count)
            // {
            tween.InterpolateProperty(this, ":scale", Scale, new Vector2(1.05f, 1.05f),
                0.5f, Tween.TransitionType.Bounce, Tween.EaseType.In);
            tween.Start();
            select.Show();
            Globals.PlaySound("chest_open", this, speaker2D);
            animationPlayer.Queue("open_chest");
            // }
        }
        public virtual void _OnSightAreaExited(Area2D area2D)
        {
            select.Hide();
            Globals.PlaySound("chest_open", this, speaker2D);
            animationPlayer.Queue("close_chest");
        }
        public void _OnSelectPressed()
        {
            Globals.player.GetMenu().LootInteract(this);
        }
        public void _OnTweenCompleted(Godot.Object obj, NodePath nodePath)
        {
            Node2D node2dObj = (Node2D)obj;
            Vector2 normalScale = new Vector2(1.0f, 1.0f);
            if (!node2dObj.Scale.Equals(normalScale))
            {
                tween.InterpolateProperty(node2dObj, nodePath, node2dObj.Scale, normalScale,
                    0.5f, Tween.TransitionType.Bounce, Tween.EaseType.Out);
                tween.Start();
            }
        }
    }
}