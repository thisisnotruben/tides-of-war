using Game.Actor;
using Godot;
namespace Game.Misc.Other
{
    public class LandMine : WorldObject, ICombustible
    {
        public Area2D exludedUnitArea;
        private bool exploded;
        public short minDamage = 0;
        public short maxDamage = 10;
        public void _OnTimerTimeout()
        {
            Explode();
        }
        public void _OnAffectedAreaEntered(Area2D area2d)
        {
            if (area2d != exludedUnitArea && area2d.GetCollisionLayer() == Globals.Collision["CHARACTERS"])
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
                        particles2D.SetEmitting(true);
                    }
                }
                foreach (Area2D area2D in GetNode("img/affected_area").GetChildren())
                {
                    int layer = area2D.GetCollisionLayer();
                    if (layer == Globals.Collision["CHARACTERS"] && area2D != exludedUnitArea)
                    {
                        Character character = area2D.GetOwner()as Character;
                        if (character != null)
                        {
                            GD.Randomize();
                            character.TakeDamage((short)GD.RandRange((double)minDamage, (double)maxDamage),
                                false, this, CombatText.TextType.HIT);
                        }
                        else
                        {
                            GD.Print("LandMine found an anomaly when exploding.");
                        }
                    }
                    else if (layer == Globals.Collision["COMBUSTIBLE"])
                    {
                        ICombustible obj = area2D.GetOwner()as ICombustible;
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