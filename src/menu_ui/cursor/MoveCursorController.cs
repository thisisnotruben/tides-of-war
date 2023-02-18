using Godot;
using GC = Godot.Collections;
using Game.Database;
namespace Game.Ui
{
	public class MoveCursorController : Node2D, ISerializable
	{
		public void AddToMap(Vector2 globalTilePosition)
		{
			Map.Map.map.AddGChild(this);
			GlobalPosition = globalTilePosition;
			AddToGroup(Globals.SAVE_GROUP);
		}
		public void Delete() { QueueFree(); }
		public GC.Dictionary Serialize()
		{
			AnimationPlayer anim = GetChild<AnimationPlayer>(0);
			return new GC.Dictionary()
			{
				{NameDB.SaveTag.POSITION, new GC.Array() { GlobalPosition.x, GlobalPosition.y }},
				{NameDB.SaveTag.ANIM_POSITION,
					!anim.CurrentAnimation.Empty()
						? anim.CurrentAnimationPosition
						: 0.0f
				}
			};
		}
		public void Deserialize(GC.Dictionary payload)
		{
			float animPos = (float)payload[NameDB.SaveTag.ANIM_POSITION];
			if (animPos > 0.0f)
			{
				GetChild<AnimationPlayer>(0).Seek(animPos, true);
			}
			else
			{
				Delete();
			}
		}
	}
}