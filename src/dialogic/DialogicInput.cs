using Godot;
namespace Game.Ui
{
	public class DialogicInput : Button
	{
		private void OnPressed()
		{
			InputEventAction dialogueNext = new InputEventAction();
			dialogueNext.Action = "dialogue_next";
			dialogueNext.Pressed = true;
			Input.ParseInputEvent(dialogueNext);
		}
	}
}