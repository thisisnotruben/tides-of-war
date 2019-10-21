namespace Game
{
    public interface ISaveable
    {
        void SetSaveData(Godot.Collections.Dictionary data);
        Godot.Collections.Dictionary GetSaveData();
    }
}