using Godot;
namespace Game.Actor.Doodads
{
	public class QuestMarker : Sprite
	{
		public enum MarkerType : byte { AVAILABLE, ACTIVE, COMPLETED, OBJECTIVE }

		public void ShowMarker(MarkerType markerType)
		{
			Color color = Color.ColorN("white");
			int frame = 1;

			switch (markerType)
			{
				case MarkerType.AVAILABLE:
					color = Color.ColorN("yellow");
					Frame = 0;
					break;

				case MarkerType.COMPLETED:
					color = Color.ColorN("yellow");
					break;

				case MarkerType.OBJECTIVE:
					color = Color.ColorN("dodgerblue");
					break;
			}

			Modulate = color;
			Frame = frame;
			Show();
		}
	}
}
