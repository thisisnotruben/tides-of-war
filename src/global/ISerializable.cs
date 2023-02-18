namespace Game
{
	public interface ISerializable
	{
		Godot.Collections.Dictionary Serialize();
		void Deserialize(Godot.Collections.Dictionary payload);
	}
}