using System;
using Godot;
namespace Game.Actor.State
{
	public class Stun : StateBehavior
	{
		public override void _Ready()
		{

		}
		public override void Start()
		{
			Map.Map.map.OccupyCell(character.GlobalPosition, true);
		}
		public override void Exit()
		{
			Map.Map.map.OccupyCell(character.GlobalPosition, false);
		}
	}
}