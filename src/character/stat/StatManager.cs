using System.Collections.Generic;
using Game.GameItem;
using Game.Database;
using Game.Factory;
using GC = Godot.Collections;
namespace Game.Actor.Stat
{
	public class StatManager : Godot.Object, ISerializable
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
		private CharacterStat[] GetStats()
		{
			return new CharacterStat[] { stamina, intellect, agility, hpMax, manaMax,
				maxDamage, minDamage, regenTime, regenAmount, armor, weaponRange, animSpeed, weaponSpeed };
		}
		public void Recalculate()
		{
			foreach (CharacterStat stat in GetStats())
			{
				stat.baseValue = stat.CalculateBaseValue();
			}
		}
		private void OnRegenSet(float value) { character.regenTimer.WaitTime = value; }
		public bool ShouldSerialize() { return Serialize().Count > 0; }
		public GC.Dictionary Serialize()
		{
			HashSet<Commodity> uniqueBaseItems = new HashSet<Commodity>();
			foreach (CharacterStat stat in GetStats())
			{
				stat.statModifiers.ForEach(sM => uniqueBaseItems.Add(sM.source));
			}

			// these will already be saved somewhere else
			Player player = character as Player;
			if (player != null)
			{
				uniqueBaseItems.Remove(player.weapon);
				uniqueBaseItems.Remove(player.vest);
			}

			GC.Array uniqueBaseItemNames = new GC.Array();
			foreach (Commodity baseItem in uniqueBaseItems)
			{
				uniqueBaseItemNames.Add(new GC.Array() { baseItem.worldName, baseItem.Serialize() });
			}

			GC.Dictionary payload = new GC.Dictionary();
			if (uniqueBaseItemNames.Count > 0)
			{
				payload[NameDB.SaveTag.BASE_ITEMS] = uniqueBaseItemNames;
			}

			return payload;
		}
		public void Deserialize(GC.Dictionary payload)
		{
			if (payload.Count == 0)
			{
				return;
			}

			ItemFactory itemFactory = new ItemFactory();
			SpellFactory spellFactory = new SpellFactory();

			Commodity baseItem;
			string baseItemName;
			foreach (GC.Array baseItemPacket in (GC.Array)payload[NameDB.SaveTag.BASE_ITEMS])
			{
				baseItemName = baseItemPacket[0].ToString();
				if (Globals.spellDB.HasData(baseItemName))
				{
					baseItem = spellFactory.Make(character, baseItemName);
				}
				else
				{
					baseItem = itemFactory.Make(character, baseItemName);
				}

				character.AddChild(baseItem);
				baseItem.Deserialize((GC.Dictionary)baseItemPacket[1]);
			}
		}
	}
}