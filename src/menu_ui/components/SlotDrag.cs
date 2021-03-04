using Godot;
namespace Game.Ui
{
	public class SlotDrag : Button
	{
		public SlotController slot;

		public override object GetDragData(Vector2 position) { return slot.GetDragData(position); }
		public override bool CanDropData(Vector2 position, object data) { return slot.CanDropData(position, data); }
		public override void DropData(Vector2 position, object data) { slot.DropData(position, data); }
	}
}