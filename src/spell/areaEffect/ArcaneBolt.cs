using System;
using Game.Actor;
using Game.Database;
namespace Game.Ability
{
	public class ArcaneBolt : SpellAreaEffect
	{
		private readonly Random rand = new Random();

		protected override void StartAreaEffect(Character character)
		{
			if (50 >= rand.Next(1, 101))
			{
				base.StartAreaEffect(character);

				SpellEffect spellEffect = (SpellEffect)Globals.spellEffectDB.GetData(
					Globals.spellDB.GetData(worldName).spellEffect).Instance();
				spellEffect.Init(character, worldName, character);
				spellEffect.OnHit();
			}
		}
	}
}