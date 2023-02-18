using System;
using Game.Actor;
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

				Globals.spellEffectDB.GetData(Globals.spellDB.GetData(
					worldName).spellEffect).Instance<SpellEffect>().Init(character, worldName).OnHit();
			}
		}
	}
}