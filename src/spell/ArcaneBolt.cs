using System;
using Game.Actor;
using Game.Database;
using Godot;
namespace Game.Ability
{
	public class ArcaneBolt : SpellAreaEffect
	{
		private readonly Random rand = new Random();

		public ArcaneBolt(Character character, string worldName) : base(character, worldName) { }
		protected override void StartAreaEffect(Character character)
		{
			if (50 >= rand.Next(1, 101))
			{
				base.StartAreaEffect(character);

				SpellEffect spellEffect = SpellDB.GetSpellEffect(SpellDB.GetSpellData(worldName).spellEffect);
				spellEffect.Init(character, worldName, character);
				spellEffect.OnHit();
			}
		}
	}
}