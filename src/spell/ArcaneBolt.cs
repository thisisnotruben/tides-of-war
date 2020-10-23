using Game.Actor;
using Game.Database;
using Godot;
namespace Game.Ability
{
	public class ArcaneBolt : SpellAreaEffect
	{
		public ArcaneBolt(Character character, string worldName) : base(character, worldName)
		{
			GD.Randomize();
		}
		protected override void StartAreaEffect(Character character)
		{
			if (50 >= GD.Randi() % 100 + 1)
			{
				base.StartAreaEffect(character);

				SpellEffect spellEffect = SpellDB.GetSpellEffect(SpellDB.GetSpellData(worldName).spellEffect);
				spellEffect.Init(character, worldName, character);
				spellEffect.OnHit();
			}
		}
	}
}