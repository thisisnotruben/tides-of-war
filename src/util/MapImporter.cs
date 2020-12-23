using Godot;
using GC = Godot.Collections;
using Game.Light;
using System;
namespace Game.Util
{
	[Tool]
	public class MapImporter : Node
	{
		private const int LIGHT_MASK_PASS = 0b_00000_00000_00000_00001,
			LIGHT_MASK_SHADOW = 0b_00000_00000_00000_00010;

		private static readonly Vector2 CELL_SIZE = new Vector2(16.0f, 16.0f),
			HALF_CELL_SIZE = CELL_SIZE / 2.0f,
			OFFSET = new Vector2(0.0f, -CELL_SIZE.y);

		private const string targetDummyPath = "res://src/character/npc/targetDummy/{0}.tscn",
			lightPath = "res://src/light/{0}.tscn",
			tilesetPath = "res://asset/img/map/tilesets/{0}.png",
			worldTilesetPath = "res://asset/img/map/worldTileset.tres",
			occluderDataPath = "res://data/importer/tilesetLightOccluders.json",
			animDataPath = "res://data/importer/tilesetAnimations.json",
			shaderDataPath = "res://data/importer/tilesetShaders.json",
			lightPosPath = "res://data/importer/tilesetLightPos.json",
			animatedTilesetPath = "res://asset/img/map/tilesets_animated/{0}.tres",
			assetMapDir = "res://asset/img/map/",
			srcMapDir = "res://src/map/",
			lightGradientDir = "res://asset/img/light/gradient/";

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
			veilScene = GD.Load<PackedScene>("res://src/map/doodads/VeilFog.tscn"),
			lightScene = GD.Load<PackedScene>("res://src/light/light.tscn"),
			lightSource = GD.Load<PackedScene>("res://src/light/lightSource.tscn");

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
		public void ExportAllMaps()
		{
			Directory directory = new Directory();
			directory.Open(srcMapDir);
			directory.ListDirBegin(true, true);

			string filename = directory.GetNext();
			while (!filename.Empty())
			{
				if (filename.Contains("zone"))
				{
					GD.Print("Importing: " + srcMapDir.PlusFile(filename));
					ImportMap(srcMapDir.PlusFile(filename));
				}
				filename = directory.GetNext();
			}
			directory.ListDirEnd();
		}
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

			Node2D meta = map.GetNode<Node2D>("meta"),
				zedGroup = map.GetNode<Node2D>("zed"),
				targetDummys = zedGroup.GetNode<Node2D>("target_dummys"),
				characters = zedGroup.GetNode<Node2D>("characters"),
				transitionsZones = meta.GetNode<Node2D>("transitionZones"),
				lights = meta.GetNode<Node2D>("lights"),
				graveSites = meta.GetNode<Node2D>("gravesites"),
				transitions = meta.GetNode<Node2D>("transitions"),
				paths = meta.GetNode<Node2D>("paths"),
				collNav = meta.GetNode<Node2D>("coll_nav"),
				lightSpace = meta.GetNode<Node2D>("lightSpace");

			zed = zedGroup.GetNode<TileMap>("z1");

			// add worldEnvironment
			WorldEnvironment worldEnvironment = new WorldEnvironment();
			map.AddChild(worldEnvironment, true);
			map.MoveChild(worldEnvironment, 0);
			worldEnvironment.Owner = map;

			// add worldClock scene
			Node worldClock = worldClockScene.Instance();
			map.AddChild(worldClock);
			map.MoveChild(worldClock, 1);
			worldClock.Owner = map;

			// add veil scene
			Node2D veil = (Node2D)veilScene.Instance();
			map.AddChild(veil);
			map.MoveChild(veil, 2);
			veil.Owner = map;
			veil.Hide();

			zedGroup.MoveChild(characters, 0);
			zedGroup.MoveChild(targetDummys, 1);

			SetUnits(characters, transitions);
			SetTargetDummys(targetDummys);
			SetLights(zedGroup, lights, lightSpace);
			SetShaderData();
			SetTransitions(transitionsZones, transitions);
			SetWorldTileset();

			// center gravesites on map to cell
			foreach (Node2D node2D in graveSites.GetChildren())
			{
				node2D.GlobalPosition = GetCenterPos(node2D.GlobalPosition);
				node2D.AddToGroup(Globals.GRAVE_GROUP, true);
			}

			// delete now useless nodes
			foreach (Node node in new Node[] { characters, targetDummys, paths, lights, lightSpace })
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
		private void SetLights(Node2D zedGroup, Node lights, Node lightSpace)
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
			SetTileLights(zedGroup);
			SetLightSpace(zedGroup, lightSpace);
		}
		private void SetTileLights(Node2D zedGroup)
		{
			File file = new File();
			file.Open(lightPosPath, File.ModeFlags.Read);
			GC.Dictionary lightPos = (GC.Dictionary)JSON.Parse(file.GetAsText()).Result;
			file.Close();

			TileMap tileMap;
			Node2D light;
			GC.Array pos;
			string tileID;
			int drawIndex;
			foreach (Node2D node2D in zedGroup.GetChildren())
			{
				tileMap = node2D as TileMap;
				if (tileMap == null)
				{
					continue;
				}

				foreach (Vector2 cellPos in tileMap.GetUsedCells())
				{
					tileID = tileMap.GetCellv(cellPos).ToString();
					if (!lightPos.Contains(tileID))
					{
						continue;
					}

					light = (Node2D)lightScene.Instance();

					drawIndex = tileMap.GetIndex() + 1;
					(drawIndex > zedGroup.GetChildCount() - 1
						? zedGroup
						: zedGroup.GetChild(drawIndex)
					).AddChild(light, true);
					light.Owner = map;

					pos = (GC.Array)lightPos[tileID];
					light.GlobalPosition = tileMap.MapToWorld(cellPos)
						+ tileMap.TileSet.TileGetTextureOffset(tileID.ToInt())
						+ new Vector2((float)pos[0], (float)pos[1]);
				}
			}
		}
		private void SetLightSpace(Node zedGroup, Node lightSpace)
		{
			string lightID;
			bool isConnectedLight = false;
			Light2D light2D;
			Vector2 offset;
			Action<Node, Node2D> addLightSpace = (Node parent, Node2D source) =>
			{
				light2D = (Light2D)lightSource.Instance();
				parent.AddChild(light2D);
				light2D.Owner = map;

				light2D.TextureScale = GetParsedName(source.Name).ToFloat() / light2D.Texture.GetWidth();
				// in tiled, objects's origin is in bottom left, this transforms to center origin
				offset = light2D.Texture.GetSize() * light2D.TextureScale / 2.0f;
				offset.y *= -1.0f;
				light2D.GlobalPosition = GetCenterPos(source.GlobalPosition + offset);
			};

			foreach (Node2D light in lightSpace.GetChildren())
			{
				lightID = light.Name.Split("-")[0];
				foreach (Node2D zedLight in zed.GetChildren())
				{
					if (lightID.Equals(zedLight.Name.Split("-")[0]))
					{
						addLightSpace(zedLight, light);
						isConnectedLight = true;
						break;
					}
				}
				if (!isConnectedLight)
				{
					addLightSpace(zedGroup, light);
				}
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
		private void SetShaderData()
		{
			File file = new File();
			file.Open(shaderDataPath, File.ModeFlags.Read);
			GC.Dictionary shaderData = (GC.Dictionary)JSON.Parse(file.GetAsText()).Result;
			file.Close();

			GC.Dictionary<string, string> worldObjectShaderData = new GC.Dictionary<string, string>();
			foreach (string key in shaderData.Keys)
			{
				if (key.Contains(map.Name))
				{
					worldObjectShaderData.Add(GetParsedName(key), (string)shaderData[key]);
				}
			}

			string node2DID;
			ShaderMaterial shaderMaterial;
			LightSource lightSource;
			foreach (Node2D node2D in zed.GetChildren())
			{
				node2DID = node2D.Name.Split("-")[0];
				if (worldObjectShaderData.ContainsKey(node2DID))
				{
					shaderMaterial = GD.Load<ShaderMaterial>(assetMapDir.PlusFile(worldObjectShaderData[node2DID] + "WorldObject.tres"));
					node2D.Material = shaderMaterial;
					node2D.Modulate = (Color)shaderMaterial.GetShaderParam("color");

					// set light's color to same modulate if any
					foreach (Node node in node2D.GetChildren())
					{
						lightSource = node as LightSource;
						if (lightSource != null)
						{
							lightSource.Color = node2D.Modulate;
							lightSource.gradient = GD.Load<Gradient>(lightGradientDir.PlusFile(worldObjectShaderData[node2DID] + "Light.tres"));
						}
					}
				}
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
			Vector2 offset, pos;
			GC.Array posArray, templatePoints;
			Vector2[] polygon;
			int i, j, k = 1, cellHeight, z, l;
			string gid;

			File file = new File();

			// occluder data
			file.Open(occluderDataPath, File.ModeFlags.Read);
			GC.Dictionary occluderData = (GC.Dictionary)JSON.Parse(file.GetAsText()).Result;
			file.Close();

			// anim data
			file.Open(animDataPath, File.ModeFlags.Read);
			GC.Dictionary terrrainAnimData = (GC.Dictionary)JSON.Parse(file.GetAsText()).Result;
			file.Close();

			// shader data
			file.Open(shaderDataPath, File.ModeFlags.Read);
			GC.Dictionary shaderData = (GC.Dictionary)JSON.Parse(file.GetAsText()).Result;
			file.Close();

			foreach (string imgPath in tileAtlases)
			{
				cellHeight = Is32Tileset(imgPath) ? 32 : (int)CELL_SIZE.y;
				offset = new Vector2(0.0f, -cellHeight);
				img = GD.Load<Texture>(imgPath);

				for (i = 0; i < img.GetHeight(); i += cellHeight)
				{
					for (j = 0; j < img.GetWidth(); j += (int)CELL_SIZE.x)
					{
						gid = k.ToString();
						if (terrrainAnimData.Contains(gid))
						{
							tex = GD.Load<Texture>(string.Format(animatedTilesetPath, terrrainAnimData[gid]));
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
						tileSet.TileSetTextureOffset(k, offset);

						if (occluderData.Contains(gid))
						{
							// make occluder
							posArray = (GC.Array)((GC.Dictionary)occluderData[gid])["pos"];
							pos = new Vector2((float)posArray[0], (float)posArray[1] - cellHeight);

							templatePoints = (GC.Array)occluderData[
								(string)((GC.Dictionary)occluderData[gid])["templateName"]];

							polygon = new Vector2[templatePoints.Count / 2];
							for (z = 0, l = 0; l < templatePoints.Count; l += 2, z++)
							{
								polygon[z] = pos + new Vector2((float)templatePoints[l], (float)templatePoints[l + 1]);
							}

							// set occluder
							occluderPolygon2D = new OccluderPolygon2D();
							occluderPolygon2D.Polygon = polygon;
							tileSet.TileSetLightOccluder(k, occluderPolygon2D);
						}

						if (shaderData.Contains(gid))
						{
							tileSet.TileSetMaterial(k, GD.Load<ShaderMaterial>(
								assetMapDir.PlusFile((string)shaderData[gid] + ".tres"))
							);
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