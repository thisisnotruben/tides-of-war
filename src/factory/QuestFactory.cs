using Game.Quest;
using Game.Quest.Custom;
namespace Game.Factory
{
	public class QuestFactory
	{
		public WorldQuest Create(string questId)
		{
			return questId switch
			{
				"q1.1407.3" => new q1_1407_3(),
				_ => new WorldQuest()
			};
		}
	}
}