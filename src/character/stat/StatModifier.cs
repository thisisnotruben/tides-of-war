namespace Game.Actor.Stat
{
	public class StatModifier
	{
		public enum StatModType { FLAT = 100, PERCENT_ADD = 200, PERCENT_MUL = 300 }
		internal readonly float value;
		internal readonly StatModType statModType;
		internal readonly int order;
		internal readonly object source;

		public StatModifier(float value, StatModType statModType, int order, object source)
		{
			this.value = value;
			this.statModType = statModType;
			this.order = order;
			this.source = source;
		}
		public StatModifier(float value, StatModType statModType) : this(value, statModType, (int)statModType, null) { }
		public StatModifier(float value, StatModType statModType, int order) : this(value, statModType, order, null) { }
		public StatModifier(float value, StatModType statModType, object source) : this(value, statModType, (int)statModType, source) { }
	}
}