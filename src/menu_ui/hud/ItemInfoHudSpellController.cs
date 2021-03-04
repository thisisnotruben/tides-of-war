using Godot;
namespace Game.Ui
{
	public class ItemInfoHudSpellController : ItemInfoSpellController
	{
		public CanvasItem tabContainer, parent;

		public override void _Ready()
		{
			base._Ready();
			parent = GetParent<CanvasItem>();
			foreach (Node node in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
			{
				(node as SlotController)?.Connect(nameof(SlotController.OnSlotPressed), this, nameof(OnHudSlotPressed));
			}
		}
		private void OnHudSlotPressed(string itemName)
		{
			if (!Globals.spellDB.HasData(itemName))
			{
				return;
			}

			commodityWorldName = itemName;

			OnCastPressed();
			tabContainer.SelfModulate = Color.ColorN("white", 0.0f);
			tabContainer.Visible = parent.Visible = Visible = popup.IsVisibleInTree();
		}
		protected override void OnHide()
		{
			base.OnHide();
			tabContainer.Visible = parent.Visible = false;
		}
	}
}