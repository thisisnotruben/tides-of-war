using Game.Misc.Other;
using Godot;
namespace Game.Spell
{
    public class ExplosiveTrap : Spell
    {
        public override float Cast()
        {
            PackedScene landMineScene = (PackedScene)GD.Load("res://src/misc/missile/land_mine.tscn");
            LandMine landMine = (LandMine)landMineScene.Instance();
            landMine.exludedUnitArea = caster.GetNode<Area2D>("area");
            landMine.SetGlobalPosition(Globals.GetMap().GetGridPosition(caster.GetGlobalPosition()));
            landMine.AddToGroup(caster.GetInstanceId().ToString() + "lm");
            caster.GetParent().AddChild(landMine);
            return base.Cast();
        }
    }
}