using Game.Actor;
namespace Game.Factory
{
	public abstract class Factory<T>
	{
		public T Make(Character character, string worldName) { return Create(character, worldName); }
		protected abstract T Create(Character character, string worldName);
	}
}