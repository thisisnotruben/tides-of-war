using Game.Actor.Doodads;
namespace Game.Ability
{
	public class SiphonMana : Spell
	{
		public override void Start()
		{
			base.Start();

			int siphonedAmount = (int)(character.target.mana * 0.25f);

			character.target.mana -= siphonedAmount;
			character.mana += siphonedAmount;

			character.SpawnCombatText(siphonedAmount.ToString(), CombatText.TextType.MANA);
		}
	}
}