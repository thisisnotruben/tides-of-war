namespace Game
{
	public interface ISerializable
	{
		void Deserialize(Godot.Collections.Dictionary payload);
		Godot.Collections.Dictionary Serialize();
	}
}