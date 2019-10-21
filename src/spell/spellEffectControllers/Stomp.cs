using Godot;

namespace Game.Spell
{
    public class StompEffect : SearingarrowEffect
    {
        private protected override void OnReady()
        {
            SetWorldType(WorldTypes.STOMP);
        }
    }
}