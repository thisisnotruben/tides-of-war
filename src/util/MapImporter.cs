using Godot;
using GC = Godot.Collections;
using Game.Light;
using Game.Database;
using Game.Actor;
using System;
namespace Game.Util
{
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
			lightSpaceGradientPath = "res://data/importer/lightSpaceGradient.json",
			usedGidDataPath = "res://data/importer/usedGid.json",
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

		private PackedScene
			playerScene = GD.Load<PackedScene>("res://src/character/player/player.tscn"),
			worldClockScene = GD.Load<PackedScene>("res://src/map/doodads/WorldClock.tscn"),
			veilScene = GD.Load<PackedScene>("res://src/map/doodads/VeilFog.tscn"),
			lightScene = GD.Load<PackedScene>("res://src/light/light.tscn"),
			lightSource = GD.Load<PackedScene>("res://src/light/lightSource.tscn"),
			transitionSign = GD.Load<PackedScene>("res://src/map/doodads/transitionSign.tscn"),
			interactItem = GD.Load<PackedScene>("res://src/map/doodads/interactItem.tscn");

		private Resource environment = GD.Load<Godot.Environment>("res://2d_env.tres");

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
			map = mapToImport.Instance<Node2D>();
			map.GlobalPosition = new Vector2(0.0f, CELL_SIZE.y);

			Node2D meta = map.GetNode<Node2D>("meta"),
				zedGroup = map.GetNode<Node2D>("zed"),
				targetDummys = zedGroup.GetNode<Node2D>("target_dummys"),
				characters = zedGroup.GetNode<Node2D>("characters"),
				transitionsZones = meta.GetNode<Node2D>("transitionZones"),
				transitionsSigns = zedGroup.GetNode<Node2D>("transitionSigns"),
				lights = meta.GetNode<Node2D>("lights"),
				graveSites = meta.GetNode<Node2D>("gravesites"),
				transitions = meta.GetNode<Node2D>("transitions"),
				paths = meta.GetNode<Node2D>("paths"),
				collNav = meta.GetNode<Node2D>("coll_nav"),
				lightSpace = meta.GetNode<Node2D>("lightSpace"),
				audio = meta.GetNode<Node2D>("audio");

			GC.Array<Node2D> quests = new GC.Array<Node2D>();
			foreach (Node group in new Node[] { zedGroup, map.GetNode("ground") })
			{
				if (group.HasNode("quest"))
				{
					quests.Add(group.GetNode<Node2D>("quest"));
				}
			}

			zed = zedGroup.GetNode<TileMap>("z1");

			// add worldEnvironment
			WorldEnvironment worldEnvironment = new WorldEnvironment();
			worldEnvironment.Environment = (Godot.Environment)environment;
			map.AddChild(worldEnvironment, true);
			map.MoveChild(worldEnvironment, 0);
			worldEnvironment.Owner = map;

			// add worldClock scene
			Node worldClock = worldClockScene.Instance();
			map.AddChildBelowNode(worldEnvironment, worldClock);
			worldClock.Owner = map;

			// add veil scene
			Node2D veil = veilScene.Instance<Node2D>();
			map.AddChildBelowNode(worldClock, veil);
			veil.Owner = map;
			veil.Hide();

			zedGroup.MoveChild(characters, 0);
			zedGroup.MoveChild(targetDummys, 1);

			SetTransitions(transitionsZones, transitionsSigns, transitions);
			SetUnits(characters, transitions);
			SetTargetDummys(targetDummys);
			SetQuestItems(quests);
			SetLights(zedGroup, lights, lightSpace);
			SetAudioCollisions(audio);
			SetShaderData();
			SetWorldTileset();

			// center gravesites on map to cell
			foreach (Node2D node2D in graveSites.GetChildren())
			{
				node2D.GlobalPosition = GetCenterPos(node2D.GlobalPosition);
				node2D.AddToGroup(Globals.GRAVE_GROUP, true);
			}

			// delete now useless nodes
			foreach (Node node in new Node[] { characters, targetDummys, paths, lights, lightSpace, transitionsZones })
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
			string dataPath = string.Format(PathManager.unitDataTemplate, map.Name);
			File file = new File();
			if (file.FileExists(dataPath))
			{
				Globals.unitDB.LoadData(dataPath);
			}

			// spawn npc's
			Node2D character;
			foreach (Node2D node in characters.GetChildren())
			{
				if (Globals.unitDB.HasData(node.Name)
				&& !Globals.unitDB.GetData(node.Name).eventTrigger.Empty())
				{
					continue;
				}

				character = SceneDB.npcScene.Instance<Node2D>();
				zed.AddChild(character);
				character.Owner = map;
				character.Name = node.Name;
				character.GlobalPosition = GetCenterPos(node.GlobalPosition);
			}

			// spawn player
			string playerSpawnName = map.Name switch
			{
				"zone_2" => "zone_1",
				"zone_3" => "zone_2",
				"zone_4" => "zone_3",
				"zone_5" => "zone_4",
				_ => "playerSpawn"
			};

			character = playerScene.Instance<Node2D>();
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

				character = scene.Instance<Node2D>();
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

				light = lightScene.Instance<Node2D>();
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

			file.Open(shaderDataPath, File.ModeFlags.Read);
			GC.Dictionary shaderData = (GC.Dictionary)JSON.Parse(file.GetAsText()).Result;
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

					light = lightScene.Instance<Node2D>();

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

					if (map.Name.Equals("zone_2") && shaderData.Contains(tileID))
					{
						// using the '*WorldObject.tres' for this, has
						// the shader already set in vs. tileset resource.
						light.Modulate = (Color)GD.Load<ShaderMaterial>(
							assetMapDir.PlusFile(shaderData[tileID] + "WorldObject.tres")
						).GetShaderParam("color");
					}
				}
			}
		}
		private void SetLightSpace(Node zedGroup, Node lightSpace)
		{
			string lightID;
			bool isConnectedLight = false;
			LightSource light2D;
			Vector2 offset;

			File file = new File();
			file.Open(lightSpaceGradientPath, File.ModeFlags.Read);
			GC.Dictionary lightGradientData = (GC.Dictionary)((GC.Dictionary)JSON.Parse(file.GetAsText()).Result)[map.Name];
			file.Close();

			Action<Node, Node2D> addLightSpace = (Node parent, Node2D source) =>
			{
				light2D = lightSource.Instance<LightSource>();
				parent.AddChild(light2D);
				light2D.Owner = map;

				light2D.TextureScale = GetParsedName(source.Name).ToFloat() / light2D.Texture.GetWidth();
				// in tiled, objects's origin is in bottom left, this transforms to center origin
				offset = light2D.Texture.GetSize() * light2D.TextureScale / 2.0f;
				offset.y *= -1.0f;
				light2D.GlobalPosition = GetCenterPos(source.GlobalPosition + offset);

				string lightName = source.Name.Split("-")[0];
				if (lightGradientData.Contains(lightName))
				{
					light2D.gradient = GD.Load<Gradient>(lightGradientDir.PlusFile(lightGradientData[lightName] + ".tres"));
				}
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
		private void SetAudioCollisions(Node audio)
		{
			if (audio == null)
			{
				return;
			}

			CollisionPolygon2D collisionPolygon2D;
			Area2D area2D;
			foreach (Node2D node2D in audio.GetChildren())
			{
				// Re-parent to area-2d
				collisionPolygon2D = node2D.GetChild<CollisionPolygon2D>(0);
				node2D.RemoveChild(collisionPolygon2D);
				node2D.Name = node2D.Name + "-delete";
				node2D.Owner = null;

				area2D = new Area2D();
				audio.AddChild(area2D);
				area2D.Owner = map;

				area2D.AddChild(collisionPolygon2D);
				collisionPolygon2D.Owner = map;
				collisionPolygon2D.BuildMode = CollisionPolygon2D.BuildModeEnum.Solids;
				area2D.Name = node2D.Name.Split("-delete")[0];
				area2D.GlobalPosition = new Vector2(node2D.GlobalPosition.x, node2D.GlobalPosition.y - CELL_SIZE.y);
				area2D.CollisionMask = Character.COLL_MASK_PLAYER;
				area2D.CollisionLayer = 0;
				area2D.Monitorable = false;
			}
		}
		private void SetTransitions(Node transitionZones, Node transitionSigns, Node transitions)
		{
			foreach (Sprite sign in transitionSigns.GetChildren())
			{
				Sprite sprite = transitionSign.Instance<Sprite>();

				zed.AddChild(sprite);
				sprite.Owner = map;
				sprite.Name = map.Name + "-" + sign.Name;
				sprite.GlobalPosition = new Vector2(sign.GlobalPosition.x + HALF_CELL_SIZE.x, sign.GlobalPosition.y - CELL_SIZE.y);
				sprite.Texture = sign.Texture;
				sprite.RegionRect = sign.RegionRect;

				Area2D area2D = new Area2D();
				sprite.AddChild(area2D);
				area2D.Owner = map;
				area2D.Name = "area2D";
				area2D.Monitorable = false;
				area2D.CollisionLayer = 0;
				area2D.CollisionMask = Character.COLL_MASK_PLAYER;

				Node2D transitionArea = transitionZones.GetNode<Node2D>(sign.Name);
				area2D.GlobalPosition = new Vector2(transitionArea.GlobalPosition.x, transitionArea.GlobalPosition.y - CELL_SIZE.y);

				// remove collision layer from tiled node and set it to node with script for detection purposes
				Node2D collisionShape = transitionArea.GetChild<Node2D>(0);
				transitionArea.RemoveChild(collisionShape);
				area2D.AddChild(collisionShape);
				collisionShape.Owner = map;
			}

			transitionSigns.Owner = null;
			transitionSigns.QueueFree();

			foreach (Node2D node2D in transitions.GetChildren())
			{
				node2D.GlobalPosition = GetCenterPos(node2D.GlobalPosition);
			}
		}
		private void SetQuestItems(GC.Array<Node2D> quests)
		{
			foreach (Node questLayer in quests)
			{
				GC.Array children = questLayer.GetChildren();
				foreach (Node2D questLoot in children)
				{
					if (questLoot is Sprite)
					{
						Sprite sprite = interactItem.Instance<Sprite>(),
							questLootSprite = (Sprite)questLoot;

						zed.AddChild(sprite);
						sprite.Owner = map;
						sprite.Name = questLoot.Name;
						sprite.GlobalPosition = new Vector2(questLoot.GlobalPosition.x + HALF_CELL_SIZE.x,
							questLoot.GlobalPosition.y - CELL_SIZE.y);
						sprite.Texture = questLootSprite.Texture;
						sprite.RegionRect = questLootSprite.RegionRect;
						if (questLootSprite.RegionRect.Size.y == 32.0f)
						{
							sprite.Offset = new Vector2(0.0f, -16.0f);
						}

						Area2D area2D = new Area2D();
						sprite.AddChild(area2D, true);
						area2D.Owner = map;
						area2D.Monitorable = false;
						area2D.CollisionLayer = 0;
						area2D.CollisionMask = Character.COLL_MASK_PLAYER;
						area2D.Position = sprite.Offset / 2.0f;

						CollisionShape2D collisionShape2D = new CollisionShape2D();
						area2D.AddChild(collisionShape2D, true);
						collisionShape2D.Owner = map;

						CircleShape2D circleShape2D = new CircleShape2D();
						circleShape2D.Radius = 16.0f;
						collisionShape2D.Shape = circleShape2D;

						sprite.Hide();
					}
					else if (questLoot is StaticBody2D)
					{
						Area2D area2D = new Area2D();
						zed.AddChild(area2D);
						area2D.Owner = map;
						area2D.Name = questLoot.Name;
						area2D.GlobalPosition = new Vector2(questLoot.GlobalPosition.x, questLoot.GlobalPosition.y - CELL_SIZE.y);

						Node collisionShape = questLoot.GetChild(0);
						questLoot.RemoveChild(collisionShape);
						area2D.AddChild(collisionShape, true);
						collisionShape.Owner = map;

						area2D.Monitoring = area2D.Monitorable = false;
						area2D.CollisionLayer = 0;
						area2D.CollisionMask = Character.COLL_MASK_PLAYER;
					}
				}
				questLayer.Owner = null;
				questLayer.QueueFree();
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
					worldObjectShaderData.Add(GetParsedName(key), shaderData[key].ToString());
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
			int i, j, k = 0, cellHeight, z, l;
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

			// used gid data
			file.Open(usedGidDataPath, File.ModeFlags.Read);
			GC.Array usedGidData = (GC.Array)JSON.Parse(file.GetAsText()).Result;
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
						if (!usedGidData.Contains((float)++k))
						{
							continue;
						}

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

							templatePoints = (GC.Array)occluderData[((GC.Dictionary)occluderData[gid])["templateName"]];

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
								assetMapDir.PlusFile(shaderData[gid].ToString() + ".tres"))
							);
						}
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