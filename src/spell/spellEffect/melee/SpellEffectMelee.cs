using Game.Actor;
using Game.Database;
using Godot;
namespace Game.Ability
{
	public class SpellEffectMelee : SpellEffect
	{
		public override void Init(Character character, string spellWorldName)
		{
			base.Init(character, spellWorldName);

			switch (spellWorldName)
			{
				case WorldNameDB.BASH:
					onHitEffect = () =>
					{
						// base.OnHit(spell);
						Position = character.head.Position;
						tween.Start();
						timer.Start();
					};
					onTimeOut = () =>
					{
						// base._OnTimerTimeout();
						FadeLight(true);
						tween.InterpolateProperty(this, ":modulate", Modulate,
							new Color("00ffffff"), lightFadeDelay,
							Tween.TransitionType.Linear, Tween.EaseType.InOut);
						tween.Start();
						timer.Start();
						foreach (Particles2D particles2D in idleParticles.GetChildren())
						{
							particles2D.Emitting = false;
						}
					};
					break;
				case WorldNameDB.CLEAVE:
				case WorldNameDB.DEVASTATE:
				case WorldNameDB.HEMORRHAGE:
				case WorldNameDB.OVERPOWER:
					onHitEffect = () =>
						{
							// base.OnHit(spell);
							Position = character.img.Position;
							tween.Start();
							timer.Start();
						};
					break;
			}
		}
	}
}