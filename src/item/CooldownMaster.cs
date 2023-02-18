using Godot;
using GC = Godot.Collections;
namespace Game
{
	public class CooldownMaster : Node, ISerializable
	{
		// structure: rootNode (character) with list of items with their respective cooldowns
		private readonly GC.Dictionary<string, GC.Dictionary<string, Timer>> cooldowns =
				new GC.Dictionary<string, GC.Dictionary<string, Timer>>();

		public override void _Ready()
		{
			Name = nameof(CooldownMaster);
			AddToGroup(Globals.SAVE_GROUP);
		}
		public bool AddCooldown(string rootNodePath, string worldName, float cooldownSec)
		{
			if (IsCoolingDown(rootNodePath, worldName))
			{
				return false;
			}
			SetCooldown(rootNodePath, worldName, cooldownSec);
			return true;
		}
		private void SetCooldown(string rootNodePath, string worldName, float cooldownSec)
		{
			if ((int)cooldownSec <= 0)
			{
				return;
			}

			Timer cooldownTimer = new Timer();
			AddChild(cooldownTimer, true);
			cooldownTimer.OneShot = true;
			cooldownTimer.Connect("timeout", this, nameof(OnCooldownTimeout),
				new GC.Array() { rootNodePath, worldName, cooldownTimer });
			cooldownTimer.Start(cooldownSec);

			if (cooldowns.ContainsKey(rootNodePath))
			{
				cooldowns[rootNodePath].Add(worldName, cooldownTimer);
			}
			else
			{
				cooldowns[rootNodePath] = new GC.Dictionary<string, Timer>() { { worldName, cooldownTimer } };
			}
		}
		public float GetCoolDown(string rootNodePath, string worldName)
		{
			return IsCoolingDown(rootNodePath, worldName)
				? cooldowns[rootNodePath][worldName].TimeLeft
				: 0.0f;
		}
		public bool IsCoolingDown(string rootNodePath, string worldName)
		{
			return cooldowns.ContainsKey(rootNodePath) && cooldowns[rootNodePath].ContainsKey(worldName);
		}
		public void ClearCooldowns()
		{
			cooldowns.Clear();
			for (int i = 0; i < GetChildCount(); i++)
			{
				GetChild(i).QueueFree();
			}
		}
		public void OnCooldownTimeout(string rootNodePath, string worldName, Timer timer)
		{
			if (cooldowns.ContainsKey(rootNodePath) && cooldowns[rootNodePath].ContainsKey(worldName))
			{
				cooldowns[rootNodePath].Remove(worldName);
				if (cooldowns[rootNodePath].Count == 0)
				{
					cooldowns.Remove(rootNodePath);
				}
			}
			timer?.QueueFree();
		}
		public GC.Dictionary Serialize()
		{
			GC.Dictionary<string, GC.Dictionary> payload = new GC.Dictionary<string, GC.Dictionary>();
			foreach (string nodePath in cooldowns.Keys)
			{
				payload[nodePath.ToString()] = new GC.Dictionary();
				foreach (string commodityName in cooldowns[nodePath].Keys)
				{
					payload[nodePath.ToString()][commodityName] = cooldowns[nodePath][commodityName].TimeLeft;
				}
			}
			return (GC.Dictionary)payload;
		}
		public void Deserialize(GC.Dictionary payload)
		{
			GC.Dictionary specificCooldowns;
			foreach (string nodePath in payload.Keys)
			{
				specificCooldowns = (GC.Dictionary)payload[nodePath];
				foreach (string commodityName in specificCooldowns.Keys)
				{
					SetCooldown(nodePath, commodityName, (float)specificCooldowns[commodityName]);
				}
			}
		}
	}
}