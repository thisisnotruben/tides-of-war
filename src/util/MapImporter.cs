using System;
using Godot;
namespace Game.Util
{
	public class MapImporter : Node
	{
		private string targetDummyPath = "res://src/character/npc/target_dummy/{0}.tscn",
			lightPath = "res://src/light/{0}.tscn";
		private PackedScene npcScene = GD.Load<PackedScene>("res://src/character/npc/npc.tscn"),
			playerScene = GD.Load<PackedScene>("res://src/character/player/player.tscn"),
			transitionScene = GD.Load<PackedScene>("res://src/map/doodads/TransitionZone.tscn"),
			dayTimeScene = GD.Load<PackedScene>("res://src/map/doodads/DayTime.tscn"),
			veilScene = GD.Load<PackedScene>("res://src/map/doodads/VeilFog.tscn");

		private static readonly Vector2 CELL_SIZE = new Vector2(16.0f, 16.0f),
			HALF_CELL_SIZE = CELL_SIZE / 2.0f,
			OFFSET = new Vector2(0.0f, -CELL_SIZE.y);

		private Node2D map;
		private TileMap zed;
		private Label label;
		[Signal] public delegate void SendScenePath(string scenePath);

		public override void _Ready()
		{
			label = GetChild<Label>(2);
			Connect(nameof(SendScenePath), GetChild(0), "set_map_script");
		}
		public void _OnQuit() { GetTree().Quit(); }
		public void ImportMap(string scenePath)
		{
			EmitSignal(nameof(SendScenePath), scenePath);
			PackedScene packedScene = GD.Load<PackedScene>(scenePath);
			Godot.Error code = CheckMap(packedScene);
			label.Text = $"Error code for ({scenePath}): {code}";
		}
		private Godot.Error CheckMap(PackedScene mapToImport)
		{
			map = (Node2D)mapToImport.Instance();
			map.GlobalPosition = new Vector2(0.0f, CELL_SIZE.y);

			zed = map.GetNode<TileMap>("zed/z1");
			Node2D characters = map.GetNode<Node2D>("zed/characters"),
				transitions = map.GetNode<Node2D>("meta/transitions"),
				transitionsZones = map.GetNode<Node2D>("meta/transitionZones"),
				targetDummys = map.GetNode<Node2D>("zed/target_dummys"),
				lights = map.GetNode<Node2D>("meta/lights"),
				graveSites = map.GetNode<Node2D>("meta/gravesites"),
				paths = map.GetNode<Node2D>("meta/paths"),
				meta = map.GetNode<Node2D>("meta"),
				collNav = map.GetNode<Node2D>("meta/coll_nav");

			// add dayTime scene
			Node dayTime = dayTimeScene.Instance();
			map.AddChild(dayTime);
			dayTime.Owner = map;
			map.MoveChild(dayTime, 0);

			// add veil scene
			Node2D veil = (Node2D)veilScene.Instance();
			map.AddChild(veil);
			veil.Owner = map;
			map.MoveChild(veil, 1);
			veil.Hide();

			SetUnits(characters, transitions);
			SetTargetDummys(targetDummys);
			SetLights(lights);
			SetTransitions(transitionsZones, transitions);
			TreeUseMaterial(map);

			// center gravesites on map to cell
			foreach (Node2D node2D in graveSites.GetChildren())
			{
				node2D.GlobalPosition = GetCenterPos(node2D.GlobalPosition);
				node2D.AddToGroup(Globals.GRAVE_GROUP, true);
			}

			// delete now useless nodes 
			foreach (Node node in new Node[] { characters, targetDummys, paths, lights })
			{
				node.Owner = null;
				node.QueueFree();
			}

			// set node options
			collNav.Modulate = new Color("80ffffff");
			zed.CellYSort = true;
			zed.CellTileOrigin = TileMap.TileOrigin.TopLeft;

			// shuffle scene tree
			meta.MoveChild(collNav, 0);
			meta.Hide();

			// save map
			PackedScene packedScene = new PackedScene();
			packedScene.Pack(map);
			return ResourceSaver.Save(map.Filename, packedScene);
		}
		private void SetUnits(Node characters, Node transitions)
		{
			// spawn npc's
			Node2D character;
			foreach (Node2D node in characters.GetChildren())
			{
				character = (Node2D)npcScene.Instance();
				zed.AddChild(character);
				character.Owner = map;
				character.Name = node.Name;
				character.GlobalPosition = GetCenterPos(node.GlobalPosition);
			}

			// spawn player
			string playerSpawnName = map.Name switch
			{
				"zone_1" => "zone_2",
				"zone_2" => "zone_3",
				"zone_3" => "zone_5",
				"zone_5" => "zone_4",
				_ => "playerSpawn"
			};

			character = (Node2D)playerScene.Instance();
			zed.AddChild(character);
			character.Owner = map;
			character.Name = "player";
			character.GlobalPosition = GetCenterPos(transitions.GetNode<Node2D>(playerSpawnName).GlobalPosition);

			zed.MoveChild(character, 0);
		}
		private void SetTargetDummys(Node targetDummys)
		{
			PackedScene scene;
			Node2D character;
			foreach (Node2D node in targetDummys.GetChildren())
			{
				scene = GD.Load<PackedScene>(string.Format(targetDummyPath, GetParsedName(node.Name)));

				character = (Node2D)scene.Instance();
				zed.AddChild(character);
				character.Owner = map;
				character.Name = node.Name;
				character.GlobalPosition = GetCenterPos(node.GlobalPosition);
			}
		}
		private void SetLights(Node lights)
		{
			string parsedName;
			PackedScene lightScene;
			Node2D light;
			Vector2 pos;
			foreach (Node2D node in lights.GetChildren())
			{
				parsedName = GetParsedName(node.Name);
				lightScene = GD.Load<PackedScene>(String.Format(lightPath, parsedName));

				light = (Node2D)lightScene.Instance();
				zed.AddChild(light);
				light.Owner = map;
				light.Name = node.Name;

				pos = node.GlobalPosition;
				pos.x += HALF_CELL_SIZE.x;
				pos.y -= CELL_SIZE.y;
				if (parsedName.Contains("pit"))
				{
					pos.y -= HALF_CELL_SIZE.y;
				}
				light.GlobalPosition = pos;
			}
		}
		private void SetTransitions(Node transitionZones, Node transitions)
		{
			Node2D transition;
			string transitionName;
			Node collisionLayer;
			foreach (Node2D node2D in transitionZones.GetChildren())
			{
				transition = (Node2D)transitionScene.Instance();
				transitionZones.AddChild(transition);
				transition.Owner = map;

				transitionName = node2D.Name;
				node2D.Name = transitionName + "-DELETE";
				transition.Name = transitionName;

				transition.GlobalPosition = new Vector2(node2D.GlobalPosition.x, node2D.GlobalPosition.y - CELL_SIZE.y);

				// remove collision layer from tiled node and set it to node with script for detection purposes
				collisionLayer = node2D.GetChild(0);
				node2D.RemoveChild(collisionLayer);
				node2D.Owner = null;
				transition.AddChild(collisionLayer);
				collisionLayer.Owner = map;
			}

			foreach (Node2D node2D in transitions.GetChildren())
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
		private string GetParsedName(string name) { return name.Split("-")[1]; }
		private Vector2 GetCenterPos(Vector2 worldPosition)
		{
			return zed.MapToWorld(zed.WorldToMap(worldPosition + OFFSET)) + HALF_CELL_SIZE;
		}
	}
}