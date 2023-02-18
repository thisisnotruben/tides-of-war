namespace Game.Ui
{
	public class ItemInfoSlotController : SlotController
	{
		public override void OnButtonChanged(bool down)
		{
			if (allowDrag)
			{
				base.OnButtonChanged(down);
			}
		}
	}
}