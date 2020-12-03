using System;
using Godot;
namespace Game.Util
{
	public class MapImporter : Node
	{
		private const int LIGHT_MASK_PASS = 0b_00000_00000_00000_00001,
			LIGHT_MASK_SHADOW = 0b_00000_00000_00000_00010;

		private static readonly Vector2 CELL_SIZE = new Vector2(16.0f, 16.0f),
			HALF_CELL_SIZE = CELL_SIZE / 2.0f,
			OFFSET = new Vector2(0.0f, -CELL_SIZE.y);

		private const string targetDummyPath = "res://src/character/npc/target_dummy/{0}.tscn",
			lightPath = "res://src/light/{0}.tscn",
			lightOccluderPath = "res://asset/img/light/occluder/{0}.tres",
			tilesetPath = "res://asset/img/map/tilesets/{0}.png",
			worldTilesetPath = "res://asset/img/map/worldTileset.tres",
			occluderDataPath = "res://data/importer/lightOccluders.json",
			animDataPath = "res://data/importer/terrainAnim.json",
			animatedTilesetPath = "res://asset/img/map/tilesets_animated/{0}.tres";

		private readonly string[] tileAtlases = new string[]
		{
			string.Format(tilesetPath, "terrain"),
			string.Format(tilesetPath, "trees"),
			string.Format(tilesetPath, "walls"),
			string.Format(tilesetPath, "tiles"),
			string.Format(tilesetPath, "tiles_32"),
			string.Format(tilesetPath, "misc"),
			string.Format(tilesetPath, "misc_32"),
			string.Format(tilesetPath, "buildings")
		};

		private PackedScene npcScene = GD.Load<PackedScene>("res://src/character/npc/npc.tscn"),
			playerScene = GD.Load<PackedScene>("res://src/character/player/player.tscn"),
			transitionScene = GD.Load<PackedScene>("res://src/map/doodads/TransitionZone.tscn"),
			worldClockScene = GD.Load<PackedScene>("res://src/map/doodads/WorldClock.tscn"),
			veilScene = GD.Load<PackedScene>("res://src/map/doodads/VeilFog.tscn");

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

			// add worldClock scene
			Node worldClock = worldClockScene.Instance();
			map.AddChild(worldClock);
			worldClock.Owner = map;
			map.MoveChild(worldClock, 0);

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

			SetWorldTileset();

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
		private void SetWorldTileset()
		{
			TileMap tileMap;
			TileSet worldTileset = GD.Load<TileSet>(worldTilesetPath);
			bool isGround;

			foreach (Node group in map.GetChildren())
			{
				isGround = group.Name.Equals("ground");

				foreach (Node layer in group.GetChildren())
				{
					tileMap = layer as TileMap;
					if (tileMap != null)
					{
						tileMap.TileSet = worldTileset;

						tileMap.LightMask = isGround
							? LIGHT_MASK_SHADOW
							: LIGHT_MASK_PASS;
						tileMap.OccluderLightMask = isGround
							? LIGHT_MASK_PASS
							: LIGHT_MASK_SHADOW;
					}
				}
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
		private void MakeTileset()
		{
			TileSet tileSet = new TileSet();

			Texture img, tex;
			OccluderPolygon2D occluderPolygon2D;
			Rect2 rect2;
			int i, j, k = 1, cellHeight;
			string tileID;

			File file = new File();

			// occluder data
			file.Open(occluderDataPath, File.ModeFlags.Read);
			Godot.Collections.Dictionary occluderData = (Godot.Collections.Dictionary)JSON.Parse(file.GetAsText()).Result;
			file.Close();

			// anim data
			file.Open(animDataPath, File.ModeFlags.Read);
			Godot.Collections.Dictionary terrrainAnimData = (Godot.Collections.Dictionary)JSON.Parse(file.GetAsText()).Result;
			file.Close();

			foreach (string imgPath in tileAtlases)
			{
				cellHeight = Is32Tileset(imgPath) ? 32 : (int)CELL_SIZE.y;
				img = GD.Load<Texture>(imgPath);

				for (i = 0; i < img.GetHeight(); i += cellHeight)
				{
					for (j = 0; j < img.GetWidth(); j += (int)CELL_SIZE.x)
					{
						tileID = k.ToString();
						if (terrrainAnimData.Contains(tileID))
						{
							tex = GD.Load<Texture>(string.Format(animatedTilesetPath, (string)terrrainAnimData[tileID]));
							rect2 = new Rect2(Vector2.Zero, CELL_SIZE);
						}
						else
						{
							tex = img;
							rect2 = new Rect2(j, i, CELL_SIZE.x, cellHeight);
						}

						tileSet.CreateTile(k);
						tileSet.TileSetTexture(k, tex);
						tileSet.TileSetRegion(k, rect2);
						tileSet.TileSetTextureOffset(k, new Vector2(0.0f, -cellHeight));

						if (occluderData.Contains(tileID))
						{
							occluderPolygon2D = GD.Load<OccluderPolygon2D>(string.Format(lightOccluderPath, (string)occluderData[tileID]));
							occluderPolygon2D.ResourceLocalToScene = true;

							tileSet.TileSetLightOccluder(k, occluderPolygon2D);
						}
						k++;
					}
				}
			}
			ResourceSaver.Save(worldTilesetPath, tileSet);

			label.Text = $"Tileset made at: {worldTilesetPath}";
		}
		private bool Is32Tileset(string imgPath)
		{
			switch (imgPath.BaseName().GetFile())
			{
				case "trees":
				case "walls":
				case "tiles_32":
				case "misc_32":
				case "buildings":
					return true;
				default:
					return false;
			}
		}
	}
}