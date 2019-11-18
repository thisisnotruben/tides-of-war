using Godot;
namespace Game.Utils
{
    public class MapImporter : Node
    {
        private string scenePath = "";
        public override void _Ready()
        {
            PackedScene packedScene = (PackedScene)GD.Load(scenePath);
            GD.Print("Error code: " + CheckMap(packedScene).ToString());
        }
        private Godot.Error CheckMap(PackedScene mapToImport)
        {
            Node map = mapToImport.Instance();
            string zedz1 = "zed/z1";
            map.GetNode<Node2D>("meta").Hide();
            map.GetNode<TileMap>(zedz1).SetYSortMode(true);
            map.GetNode<TileMap>("meta/coll_nav").SetCollisionFriction(0.0f);
            map.GetNode<TileMap>("meta/coll_nav").SetModulate(new Color(1.0f, 1.0f, 1.0f, 0.5f));
            foreach (Node2D node2D in map.GetChildren())
            {
                node2D.SetUseParentMaterial(true);
                foreach (Node2D subNode2d in node2D.GetChildren())
                {
                    subNode2d.SetUseParentMaterial(true);
                }
            }
            Node2D paths = new Node2D();
            paths.SetName(nameof(paths));
            map.GetNode<Node2D>("meta").AddChild(paths);
            map.GetNode<Node2D>("meta").SetZIndex(1);
            paths.SetOwner(map);
            PackedScene dayTimeScene = (PackedScene)GD.Load("res://src/map/doodads/day_time.tscn");
            Node dayTime = dayTimeScene.Instance();
            map.AddChild(dayTime);
            dayTime.SetOwner(map);
            PackedScene veilScene = (PackedScene)GD.Load("res://src/map/doodads/veil_fog.tscn");
            Node2D veil = (Node2D)veilScene.Instance();
            map.AddChild(veil);
            veil.SetOwner(map);
            veil.Hide();
            foreach (Node node in map.GetNode("meta/gravesites").GetChildren())
            {
                node.AddToGroup("gravesite", true);
            }
            int count = 0;
            foreach (Node2D node in map.GetNode("zed/characters").GetChildren())
            {
                if (node is Position2D)
                {
                    PackedScene playerScene = (PackedScene)GD.Load("res://src/character/player/player.tscn");
                    Node2D player = (Node2D)playerScene.Instance();
                    player.SetName("player");
                    map.GetNode(zedz1).AddChild(player);
                    player.SetOwner(map);
                    player.SetGlobalPosition(node.GetGlobalPosition());
                }
                else if (node is Sprite)
                {
                    string texturePath = ((Sprite)node).GetTexture().GetPath();
                    string parsedName = "";
                    Node2D character;
                    foreach (char letter in node.GetName())
                    {
                        if (!System.Char.IsDigit(letter))
                        {
                            parsedName += letter;
                        }
                    }
                    parsedName = parsedName.StripEdges().Capitalize();
                    if (node.GetName().Contains("<*>"))
                    {
                        parsedName = parsedName.Split("<*>")[0] + $"-{count}<*>";
                        count++;
                    }
                    else if (parsedName.Empty() && !texturePath.ToLower().Contains("target_dummy"))
                    {
                        parsedName = texturePath.GetBaseDir().GetFile();
                        if (parsedName.Equals("critter"))
                        {
                            parsedName += '-' + texturePath.BaseName().GetFile().Split('-')[0];
                        }
                        parsedName += $"-{count}";
                        if (texturePath.GetFile().BaseName().ToLower().Contains("comm"))
                        {
                            parsedName += "<#>";
                        }
                        count++;
                    }
                    if (texturePath.GetBaseDir().GetFile().Equals("misc"))
                    {
                        PackedScene targetDummyScene = (PackedScene)GD.Load("res://src/misc/other/target_dummy.tscn");
                        character = targetDummyScene.Instance()as Node2D;
                        parsedName = $"Target Dummy-{count}";
                        count++;
                    }
                    else
                    {
                        PackedScene npcScene = (PackedScene)GD.Load("res://src/character/npc/npc.tscn");
                        character = npcScene.Instance()as Node2D;
                    }
                    character.SetName(parsedName);
                    map.GetNode(zedz1).AddChild(character);
                    character.SetOwner(map);
                    character.SetGlobalPosition(node.GetGlobalPosition());
                }
            }
            foreach (Vector2 cell in map.GetNode<TileMap>(zedz1).GetUsedCells())
            {
                int tileId = map.GetNode<TileMap>(zedz1).GetCellv(cell);
                string resourcePath = "res://src/misc/light/torch_post";
                switch (tileId)
                {
                    case 10_226:
                        resourcePath += "_base.tscn";
                        break;
                    case 10_267:
                        resourcePath += ".tscn";
                        break;
                    case 10_367:
                        resourcePath += "_black_base.tscn";
                        break;
                    case 10_377:
                        resourcePath += "_black.tscn";
                        break;
                }
                if (resourcePath.Extension().Equals("tscn"))
                {
                    PackedScene lightScene = (PackedScene)GD.Load(resourcePath);
                    Node2D light = (Node2D)lightScene.Instance();
                    light.SetGlobalPosition(cell * new Vector2(16.0f, 16.0f) + new Vector2(8.0f, 16.0f));
                    map.GetNode(zedz1).AddChild(light);
                    light.SetOwner(map);
                    light.SetName($"light-{count}");
                    count++;
                }
            }
            foreach (TileMap tileMap in map.GetNode("map").GetChildren())
            {
                foreach (Vector2 cell in tileMap.GetUsedCells())
                {
                    int tileId = tileMap.GetCellv(cell);
                    string resourcePath = "res://src/misc/light/";
                    switch (tileId)
                    {
                        case 2_739:
                            resourcePath += resourcePath.PlusFile("pit.tscn");
                            break;
                        case 2_633:
                            resourcePath += resourcePath.PlusFile("torch.tscn");
                            break;
                        case 2_742:
                            resourcePath += resourcePath.PlusFile("torch_handles.tscn");
                            break;
                        case 6_857:
                            resourcePath += resourcePath.PlusFile("torch_handles_black.tscn");
                            break;
                    }
                    if (resourcePath.Extension().Equals("tscn"))
                    {
                        PackedScene lightScene = (PackedScene)GD.Load(resourcePath);
                        Sprite light = (Sprite)lightScene.Instance();
                        light.SetGlobalPosition(cell * new Vector2(16.0f, 16.0f) + new Vector2(8.0f, 8.0f) - light.GetOffset());
                        map.GetNode(zedz1).AddChild(light);
                        light.SetOwner(map);
                        light.SetName($"light-{count}");
                        count++;
                    }
                }
            }
            map.GetNode("zed/characters").QueueFree();
            map.GetNode(zedz1).MoveChild(map.GetNode("zed/z1/player"), 0);
            map.GetNode("meta").MoveChild(map.GetNode("coll_nav"), 0);
            map.MoveChild(map.GetNode("day_time"), 0);
            map.MoveChild(map.GetNode("veil_fog"), 1);
            map.MoveChild(map.GetNode("meta"), 2);
            map.SetScript(GD.Load("res://src/map/Map.cs"));
            PackedScene packedScene = new PackedScene();
            packedScene.Pack(map);
            return ResourceSaver.Save(map.GetFilename(), packedScene);
        }
    }
}