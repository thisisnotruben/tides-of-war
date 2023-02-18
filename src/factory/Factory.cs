using Game.Actor;
namespace Game.Factory
{
	public abstract class Factory<T>
	{
		public T Make(Character character, string worldName) { return Create(character, worldName); }
		public T Make(Character character, Character target, string worldName) { return Create(character, target, worldName); }
		protected abstract T Create(Character character, string worldName);
		protected virtual T Create(Character character, Character target, string worldName) { return Create(character, worldName); }
	}
}