using Godot;
using Game.Missile;
namespace Game.Ability
{
    public class explosive_trap : Spell
    {
        public override float Cast()
        {
            PackedScene landMineScene = (PackedScene)GD.Load("res://src/missile/land_mine.tscn");
            LandMine landMine = (LandMine)landMineScene.Instance();
            landMine.exludedUnitArea = caster.GetNode<Area2D>("area");
            landMine.GlobalPosition = Map.Map.map.GetGridPosition(caster.GlobalPosition);
            landMine.AddToGroup(caster.GetInstanceId().ToString() + "lm");
            caster.GetParent().AddChild(landMine);
            return base.Cast();
        }
    }
}