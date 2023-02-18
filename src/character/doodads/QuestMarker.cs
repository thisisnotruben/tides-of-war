using Godot;
using System.Collections.Generic;
namespace Game.Actor.Doodads
{
	public class QuestMarker : Sprite
	{
		public enum MarkerType : byte { AVAILABLE, ACTIVE, COMPLETED, OBJECTIVE }

		private readonly Queue<MarkerType> history = new Queue<MarkerType>(2);

		public void ShowMarker(MarkerType markerType)
		{
			Color color = Color.ColorN("white");
			int frame = 1;

			switch (markerType)
			{
				case MarkerType.AVAILABLE:
					color = Color.ColorN("yellow");
					frame = 0;
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
			history.Enqueue(markerType);
		}
		public void Revert()
		{
			if (history.Count > 1)
			{
				ShowMarker(history.Dequeue());
			}
			else
			{
				Hide();
			}
		}
	}
}
