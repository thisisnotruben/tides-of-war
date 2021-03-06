namespace Game.Actor.Stat
{
	public class StatManager : Godot.Object
	{
		protected Character character;
		public int level { get { return character.level; } }
		public float multiplier;

		public Stamina stamina;
		public Intellect intellect;
		public Agility agility;
		public HpMax hpMax;
		public ManaMax manaMax;
		public MaxDamage maxDamage;
		public MinDamage minDamage;
		public RegenTime regenTime;
		public RegenAmount regenAmount;
		public Armor armor;

		public CharacterStat weaponRange, animSpeed, weaponSpeed;

		public StatManager(Character character)
		{
			this.character = character;
			multiplier = Stats.GetMultiplier(
				Globals.unitDB.HasData(character.Name)
				? Globals.unitDB.GetData(character.Name).img.Split('-')[0] // character race
				: character.Name); // is player

			stamina = new Stamina(this);
			intellect = new Intellect(this);
			agility = new Agility(this);
			hpMax = new HpMax(this);
			manaMax = new ManaMax(this);
			maxDamage = new MaxDamage(this);
			minDamage = new MinDamage(this);
			regenTime = new RegenTime(this);
			regenAmount = new RegenAmount(this);
			armor = new Armor(this);

			weaponRange = new CharacterStat(this); // baseValue set by 'Character.SetImg'
			animSpeed = new CharacterStat(this, 1.0f);
			weaponSpeed = new CharacterStat(this, 1.3f);

			regenTime.Connect(nameof(RegenTime.OnValueChanged), this, nameof(OnRegenSet));
			OnRegenSet(regenTime.value);
		}
		public void Recalculate()
		{
			foreach (CharacterStat stat in new CharacterStat[] { stamina, intellect, agility, hpMax, manaMax,
			maxDamage, minDamage, regenTime, regenAmount, armor })
			{
				stat.baseValue = stat.CalculateBaseValue();
			}
		}
		private void OnRegenSet(float value) { character.regenTimer.WaitTime = value; }
	}
}