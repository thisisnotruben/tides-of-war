using Godot;
namespace Game.Actor.State
{
	public class Alive : StateBehavior
	{
		private Tween tween = new Tween();

		public override void _Ready()
		{
			base._Ready();
			AddChild(tween);
			GD.Randomize();
		}
		public override void Start()
		{
			// changing character detection
			Area2D area2D = character.GetNode<Area2D>("area");
			area2D.SetCollisionLayerBit(Globals.Collision["CHARACTERS"], true);
			area2D.SetCollisionLayerBit(Globals.Collision["DEAD_CHARACTERS"], true);

			// animate transparency
			tween.InterpolateProperty(character, ":modulate", character.Modulate,
				new Color("#ffffff"), 0.5f, Tween.TransitionType.Quart, Tween.EaseType.Out);

			// reset clock for health/mana regen
			character.regenTimer.Start();

			if (character is Player)
			{
				// TODO: delete tomb
				character.hp = (int)(character.hpMax * GD.RandRange(Stats.HP_MANA_RESPAWN_MIN_LIMIT, 1.0));
				character.mana = (int)(character.manaMax * GD.RandRange(Stats.HP_MANA_RESPAWN_MIN_LIMIT, 1.0));
			}
			else
			{
				character.hp = character.hpMax;
				character.mana = character.manaMax;
				if (IsInGroup(Globals.SAVE_GROUP))
				{
					RemoveFromGroup(Globals.SAVE_GROUP);
				}
				// little pop effect upon entry
				tween.InterpolateProperty(sprite, ":scale", sprite.Scale, new Vector2(1.03f, 1.03f),
					0.5f, Tween.TransitionType.Elastic, Tween.EaseType.Out);
			}
			tween.Start();

			fsm.ChangeState(FSM.State.IDLE);
		}
		public override void Exit() { }
	}
}