using System;
using Godot;
namespace Game.Util
{
	public class MapImporter : Node
	{
		private static readonly Vector2 CELL_SIZE = new Vector2(16.0f, 16.0f),
			HALF_CELL_SIZE = CELL_SIZE / 2.0f,
			OFFSET = new Vector2(0.0f, -CELL_SIZE.y);

		public string scenePath;
		private TileMap tileMap;
		private ColorRect colorRect;
		private Label label;
		[Signal] public delegate void SendScenePath(string scenePath);

		public override void _Ready()
		{
			colorRect = GetNode<ColorRect>(nameof(ColorRect));
			label = GetNode<Label>("label");
			Connect(nameof(SendScenePath), GetNode(nameof(ColorRect)), "set_map_script");
		}
		public void _OnQuit()
		{
			GetTree().Quit();
		}
		public void ImportMap(string scenePath)
		{
			this.scenePath = scenePath;
			EmitSignal(nameof(SendScenePath), scenePath);
			PackedScene packedScene = (PackedScene)GD.Load(scenePath);
			Godot.Error code = CheckMap(packedScene);
			label.Text = $"Error code for ({scenePath}): {code}";
		}
		private Godot.Error CheckMap(PackedScene mapToImport)
		{
			Node2D map = (Node2D)mapToImport.Instance();
			tileMap = map.GetNode<TileMap>("zed/z1");
			map.GlobalPosition = new Vector2(0.0f, CELL_SIZE.y);

			// add dayTime scene
			PackedScene dayTimeScene = (PackedScene)GD.Load("res://src/map/doodads/DayTime.tscn");
			Node dayTime = dayTimeScene.Instance();
			map.AddChild(dayTime);
			dayTime.Owner = map;

			// add death scene
			PackedScene veilScene = (PackedScene)GD.Load("res://src/map/doodads/VeilFog.tscn");
			Node2D veil = (Node2D)veilScene.Instance();
			map.AddChild(veil);
			veil.Owner = map;
			veil.Hide();

			// center gravesites on map to cell
			foreach (Node2D node2D in map.GetNode("meta/gravesites").GetChildren())
			{
				node2D.GlobalPosition = GetCenterPos(node2D.GlobalPosition);
				node2D.AddToGroup("gravesite", true);
			}

			SetUnits(map);
			SetTargetDummys(map);
			SetLights(map);
			SetTransitions(map);
			TreeUseMaterial(map);

			// delete now useless nodes 
			foreach (String nodePath in new String[] { "zed/characters", "zed/target_dummys", "meta/paths", "meta/lights" })
			{
				map.GetNode(nodePath).Owner = null;
				map.GetNode(nodePath).QueueFree();
			}

			// set node options
			map.GetNode<TileMap>("meta/coll_nav").Modulate = new Color(1.0f, 1.0f, 1.0f, 0.5f);
			tileMap.CellYSort = true;
			tileMap.CellTileOrigin = TileMap.TileOrigin.TopLeft;

			// shuffle scene tree
			map.GetNode<Node2D>("meta").Hide();
			map.GetNode("meta").MoveChild(map.GetNode("meta/coll_nav"), 0);
			map.MoveChild(map.GetNode("day_time"), 0);
			map.MoveChild(map.GetNode("veil_fog"), 1);
			foreach (Node node in tileMap.GetChildren())
			{
				if (node.Name.Contains("player"))
				{
					tileMap.MoveChild(node, 0);
					break;
				}
			}

			// save map
			PackedScene packedScene = new PackedScene();
			packedScene.Pack(map);
			return ResourceSaver.Save(map.Filename, packedScene);
		}
		private void SetTargetDummys(Node map)
		{
			foreach (Node2D node in map.GetNode("zed/target_dummys").GetChildren())
			{
				string targetDummyBaseName = node.Name.Split("-")[1];
				PackedScene scene = (PackedScene)GD.Load($"res://src/character/npc/target_dummy/{targetDummyBaseName}.tscn");
				Node2D character = (Node2D)scene.Instance();
				character.Name = node.Name;
				map.GetNode("zed/z1").AddChild(character);
				character.Owner = map;
				character.GlobalPosition = GetCenterPos(node.GlobalPosition);
			}
		}
		private void SetUnits(Node map)
		{
			// spawn npc's
			string scenePath = "res://src/character/npc/npc.tscn";
			foreach (Node2D node in map.GetNode("zed/characters").GetChildren())
			{
				PackedScene scene = (PackedScene)GD.Load(scenePath);
				Node2D character = (Node2D)scene.Instance();
				character.Name = node.Name;
				map.GetNode("zed/z1").AddChild(character);
				character.Owner = map;
				character.GlobalPosition = GetCenterPos(node.GlobalPosition);
			}
			// spawn player
			PackedScene playerScene = (PackedScene)GD.Load("res://src/character/player/player.tscn");
			Node2D player = (Node2D)playerScene.Instance();
			player.Name = "player";
			map.GetNode("zed/z1").AddChild(player);
			player.Owner = map;
			string playerSpawnName;
			switch (map.Name)
			{
				case "zone_1":
					playerSpawnName = "zone_2";
					break;
				case "zone_2":
					playerSpawnName = "zone_3";
					break;
				case "zone_3":
					playerSpawnName = "zone_5";
					break;
				case "zone_4":
					playerSpawnName = "game_start";
					break;
				case "zone_5":
					playerSpawnName = "zone_4";
					break;
				default:
					playerSpawnName = "opps";
					break;
			}
			player.GlobalPosition = GetCenterPos(
				map.GetNode<Node2D>("meta/transitions/" + playerSpawnName).GlobalPosition);
		}
		private void SetLights(Node map)
		{
			string resourcePath = "res://src/light/{0}.tscn";
			foreach (Node2D node in map.GetNode("meta/lights").GetChildren())
			{
				string parsedName = node.Name.Split("-")[1];
				PackedScene lightScene = (PackedScene)GD.Load(String.Format(resourcePath, parsedName));
				Node2D light = (Node2D)lightScene.Instance();
				Vector2 pos = node.GlobalPosition;
				pos.x += HALF_CELL_SIZE.x;
				pos.y -= CELL_SIZE.y;
				if (parsedName.Contains("pit"))
				{
					pos.y -= HALF_CELL_SIZE.y;
				}
				light.GlobalPosition = pos;
				map.GetNode("zed/z1").AddChild(light);
				light.Owner = map;
				light.Name = node.Name;
			}
		}
		private void SetTransitions(Node map)
		{
			string resourcePath = "res://src/map/doodads/TransitionZone.tscn";
			Node2D transitions = (Node2D)map.GetNode("meta/transitionZones");
			foreach (Node2D node2D in transitions.GetChildren())
			{
				// load and set transition scene
				PackedScene transitionScene = (PackedScene)GD.Load(resourcePath);
				Node2D transitionNode = (Node2D)transitionScene.Instance();
				transitions.AddChild(transitionNode);
				transitionNode.Owner = map;
				string transitionName = node2D.Name;
				node2D.Name = transitionName + "-DELETE";
				transitionNode.Name = transitionName;
				transitionNode.GlobalPosition = new Vector2(node2D.GlobalPosition.x, node2D.GlobalPosition.y - CELL_SIZE.y);
				// remove collision layer from tiled node and set
				// it to node with script for detection purposes
				Node collisionLayer = node2D.GetChild(0);
				node2D.RemoveChild(collisionLayer);
				transitionNode.AddChild(collisionLayer);
				collisionLayer.Owner = map;
				node2D.Owner = null;
			}
			foreach (Node2D node2D in map.GetNode("meta/transitions").GetChildren())
			{
				node2D.GlobalPosition = GetCenterPos(node2D.GlobalPosition);

			}
		}
		private void TreeUseMaterial(Node root)
		{
			// used for when 'veil' has taken effect
			foreach (Node node in root.GetChildren())
			{
				Node2D node2D = node as Node2D;
				if (node2D != null)
				{
					node2D.UseParentMaterial = true;
					TreeUseMaterial(node);
				}
			}
		}
		private Vector2 GetCenterPos(Vector2 worldPosition)
		{
			return tileMap.MapToWorld(tileMap.WorldToMap(worldPosition + OFFSET)) + HALF_CELL_SIZE;
		}
	}
}