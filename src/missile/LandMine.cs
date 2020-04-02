using Game.Actor;
using Game.Actor.Doodads;
using Godot;
namespace Game.Missile
{
    public class LandMine : WorldObject, ICombustible
    {
        public Area2D exludedUnitArea;
        private bool exploded;
        public int minDamage = 0;
        public int maxDamage = 10;
        public void _OnTimerTimeout()
        {
            Explode();
        }
        public void _OnAffectedAreaEntered(Area2D area2d)
        {
            if (area2d != exludedUnitArea && area2d.CollisionLayer == Globals.Collision["CHARACTERS"])
            {
                Explode();
            }
        }
        public void _OnTweenCompleted(Godot.Object obj, NodePath nodePath)
        {
            QueueFree();
        }
        public void Explode()
        {
            if (!exploded)
            {
                exploded = true;
                GetNode<AudioStreamPlayer2D>("img/snd").Play();
                foreach (Node node in GetNode("img/explode").GetChildren())
                {
                    Particles2D particles2D = node as Particles2D;
                    if (particles2D != null)
                    {
                        particles2D.Emitting = true;
                    }
                }
                foreach (Area2D area2D in GetNode("img/affected_area").GetChildren())
                {
                    uint layer = area2D.CollisionLayer;
                    if (layer == Globals.Collision["CHARACTERS"] && area2D != exludedUnitArea)
                    {
                        Character character = area2D.Owner as Character;
                        if (character != null)
                        {
                            GD.Randomize();
                            character.TakeDamage((int)GD.RandRange((double)minDamage, (double)maxDamage),
                                false, this, CombatText.TextType.HIT);
                        }
                        else
                        {
                            GD.Print("LandMine found an anomaly when exploding.");
                        }
                    }
                    else if (layer == Globals.Collision["COMBUSTIBLE"])
                    {
                        ICombustible obj = area2D.Owner as ICombustible;
                        if (obj != null)
                        {
                            obj.Explode();
                        }
                    }
                }
            }
        }
    }
}