using System.Collections.Generic;
namespace Game.Ability
{
    public class overpower : Spell
    {
        public override void Init(string worldName)
        {
            base.Init(worldName);
            attackTable = new Dictionary<string, ushort>()
            { { "hit", 90 }, { "critical", 100 }, { "dodge", 100 }, { "parry", 100 },
            };
        }
    }
}