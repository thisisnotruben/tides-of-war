using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
namespace Game.Actor.Stat
{
	public class CharacterStat
	{
		public float baseValue;
		public int valueI { get { return (int)(Math.Round(value)); } }
		public float value
		{
			get
			{
				if (isDirty || baseValue != lastBaseValue)
				{
					lastBaseValue = baseValue;
					_value = CalculateFinalValue();
					isDirty = false;
				}
				return _value;
			}
		}
		public readonly ReadOnlyCollection<StatModifier> StatModifiers;
		private readonly List<StatModifier> statModifiers;
		private bool isDirty = true;
		private float _value;
		private float lastBaseValue = float.MinValue;
		protected StatManager manager;

		public CharacterStat(StatManager statManager)
		{
			manager = statManager;
			statModifiers = new List<StatModifier>();
			StatModifiers = statModifiers.AsReadOnly();
			baseValue = CalculateBaseValue();
		}
		public CharacterStat(StatManager statManager, float baseValue) : this(statManager)
		{
			this.baseValue = baseValue;
		}
		public void AddModifier(StatModifier statModifier)
		{
			isDirty = true;
			statModifiers.Add(statModifier);
			statModifiers.Sort(CompareModifierOrder);
		}
		public bool RemoveModifier(StatModifier statModifier)
		{
			if (statModifiers.Remove(statModifier))
			{
				isDirty = true;
				return true;
			}
			return false;
		}
		public bool RemoveAllModifiersFromSource(object source)
		{
			bool didRemove = false;
			for (int i = statModifiers.Count - 1; i >= 0; i--)
			{
				if (statModifiers[i].source == source)
				{
					isDirty = true;
					didRemove = true;
					statModifiers.RemoveAt(i);
				}
			}
			return didRemove;
		}
		public virtual float CalculateBaseValue()
		{
			return baseValue;
		}
		private float CalculateFinalValue()
		{
			float finalValue = CalculateBaseValue();
			float sumPercentAdd = 0.0f;
			StatModifier mod;

			for (int i = 0; i < StatModifiers.Count; i++)
			{
				mod = statModifiers[i];

				if (mod.statModType == StatModifier.StatModType.FLAT)
				{
					finalValue += mod.value;
				}
				else if (mod.statModType == StatModifier.StatModType.PERCENT_ADD)
				{
					if (i + 1 >= statModifiers.Count || statModifiers[i + 1].statModType != StatModifier.StatModType.PERCENT_ADD)
					{
						finalValue *= 1 + sumPercentAdd;
						sumPercentAdd = 0;
					}
				}
				else if (mod.statModType == StatModifier.StatModType.PERCENT_MUL)
				{
					finalValue *= 1.0f * mod.value;
				}
			}
			return (float)Math.Round(finalValue, 4);
		}
		private int CompareModifierOrder(StatModifier a, StatModifier b)
		{
			if (a.order < b.order)
			{
				return -1;
			}
			else if (a.order > b.order)
			{
				return 1;
			}
			return 0;
		}
	}
}