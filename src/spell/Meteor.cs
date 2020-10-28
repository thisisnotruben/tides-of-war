using Game.Actor;
using Godot;
using System;
namespace Game.Ability
{
	public class Meteor : SpellAreaEffect
	{
		public Meteor(Character character, string worldName) : base(character, worldName) { }
	}
}