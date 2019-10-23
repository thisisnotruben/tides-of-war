using Godot;
using System;
using System.Collections.Generic;
using Game.Actor;
using Game.Misc.Loot;
using Game.Database;

namespace Game.Map
{
    public class Map : Node2D
    {
        private TileMap mapGrid;
        private readonly AStar aStar = new AStar();
        private readonly Vector2 HALF_CELL_SIZE = new Vector2(8.0f, 8.0f);
        private const float ASTAR_OCCUPIED_WEIGHT = 25.0f;
        private const float ASTAR_NORMAL_WEIGHT = 1.0f;
        private const int OBSTACLE_TILE = 4577;
        private List<Vector2> mapObstacles = new List<Vector2>();
        private Vector2 mapSize;
        private Vector2 pathStartPosition;
        private Vector2 pathEndPosition;

        public override void _Ready()
        {
            Globals.SetMap(this); // This is used for debugging, until Godot fixies the issue
            mapGrid = GetNode<TileMap>("meta/coll_nav");
            mapSize = mapGrid.GetUsedRect().Size;
            SetObstacles();
            List<Vector2> walkableCells = AstarAddWalkableCells(mapObstacles);
            AstarConnectWalkableCellsDiagonal(walkableCells);
            SetVeilSize();
            SetPlayerCameraLimits();
            SetUnits();
        }
        public void SetVeil(bool on)
        {
            Particles2D veil = GetNode<Particles2D>("veil_fog");
            veil.SetEmitting(on);
            if (on)
            {
                SetMaterial((ParticlesMaterial)GD.Load("res://src/map/doodads/veil.tres"));
                veil.Show();
            }
            else
            {
                SetMaterial(null);
                veil.Hide();
            }
        }
        private void SetPlayerCameraLimits()
        {
            Rect2 mapBorders = mapGrid.GetUsedRect();
            Vector2 mapCellSize = mapGrid.GetCellSize();
            Camera2D playerCamera = Globals.player.GetNode<Camera2D>("img/camera");
            playerCamera.LimitLeft = (int)(mapBorders.Position.x * mapCellSize.x);
            playerCamera.LimitRight = (int)(mapBorders.End.x * mapCellSize.x);
            playerCamera.LimitTop = (int)(mapBorders.Position.y * mapCellSize.y);
            playerCamera.LimitBottom = (int)(mapBorders.End.y * mapCellSize.y);
        }
        private void SetVeilSize()
        {
            Vector2 veilSize = mapSize * HALF_CELL_SIZE;
            Particles2D veil = GetNode<Particles2D>("veil_fog");
            ParticlesMaterial veilMaterial = (ParticlesMaterial)veil.GetProcessMaterial();
            veilMaterial.SetEmissionBoxExtents(new Vector3(veilSize.x, veilSize.y, 0.0f));
            veil.SetAmount((int)((veilSize.x + veilSize.y) / 2.0f));
            veil.SetVisibilityRect(new Rect2(-veilSize, veilSize * 2.0f));
            veil.SetGlobalPosition(veilSize);
        }
        private void SetUnits()
        {
            foreach (Node2D node2D in GetNode<Node>("zed/z1").GetChildren())
            {
                Character character = node2D as Character;
                if (character != null)
                {
                    Vector2 originPosition = GetGridPosition(character.GetGlobalPosition());
                    character.origin = originPosition;
                    character.SetGlobalPosition(originPosition);
                    character.path.Add(originPosition);
                    OccupyOriginCell(originPosition);
                    Npc npc = character as Npc;
                    if (npc != null)
                    {
                        if (npc.GetName().Contains("<*>") || UnitDB.IsUnitUnique(npc.GetName(), GetName()))
                        {
                            SetDeterminedUnit(npc);
                        }
                        else
                        {
                            SetRandomizedUnit(npc);
                        }
                    }
                }
            }
        }
        private void SetDeterminedUnit(Npc npc)
        {
            string unitNodeName = npc.GetName();
            if (unitNodeName.Contains("<*>"))
            {
                unitNodeName = unitNodeName.Split("-")[0];
            }
            npc.SetUniqueData(UnitDB.GetUniqueUnitData(npc.GetName(), GetName()));
            // Godot.Collections.Dictionary unitMeta = (Godot.Collections.Dictionary)((Godot.Collections.Dictionary)Globals.unitMeta[GetName()])[unitNodeName];
            // foreach (string attribute in unitMeta.Keys)
            // {
            //     switch (attribute)
            //     {
            //         case "img":
            //             npc.SetImg("res://asset/img/character".PlusFile((string)unitMeta[attribute]));
            //             break;
            //         case "item":
            //         case "spell":
            //             if (unitMeta["type"].Equals("trainer") || unitMeta["type"].Equals("merchant"))
            //             {
            //                 foreach (Godot.Collections.Dictionary item in (Godot.Collections.Array)unitMeta[attribute])
            //                 {
            //                     Pickable itm = (Pickable)((((string)attribute).Equals("item") ? Globals.item : Globals.spell)).Instance();
            //                     foreach (string itemAttribute in item.Keys)
            //                     {
            //                         itm.Set(itemAttribute, item[itemAttribute]);
            //                     }
            //                     itm.GetPickable(npc, false);
            //                 }
            //             }
            //             break;
            //         default:
            //             npc.Set(attribute, unitMeta[attribute]);
            //             break;
            //     }
            // }
        }
        private void SetRandomizedUnit(Npc npc)
        {
            string raceName = npc.GetName().ToLower().Split('-')[0];
            string directoryPath = "res://asset/img/character".PlusFile(raceName);
            string ext = "png";
            List<string> spriteList = new List<string>();
            Directory dir = new Directory();
            dir.Open(directoryPath);
            dir.ListDirBegin(true, true);
            string resource = dir.GetNext();
            while (!resource.Empty())
            {
                resource = directoryPath.PlusFile(resource);
                if (resource.Extension().Equals(ext) && !resource.Contains("flail")
                && !((npc.GetName().Contains("critter") && !resource.Contains(npc.GetName().Split('-')[1]))
                || ((npc.GetName().Contains("<#>") && !resource.Contains("comm"))
                || (!npc.GetName().Contains("<#>") && resource.Contains("comm")))))
                {
                    spriteList.Add(resource);
                }
                resource = dir.GetNext();
            }
            dir.ListDirEnd();
            resource = spriteList[(int)(GD.Randi() % spriteList.Count)];
            npc.SetWorldName(UnitDB.GetGenericUnitName(resource));
            npc.SetEnemy(UnitDB.GetGenericUnitEnemy(resource));
        }
        private void SetObstacles()
        {
            mapObstacles.Clear();
            foreach (Vector2 cell in mapGrid.GetUsedCells())
            {
                if (mapGrid.GetCellv(cell) == OBSTACLE_TILE)
                {
                    mapObstacles.Add(cell);
                }
            }
        }
        private List<Vector2> AstarAddWalkableCells(List<Vector2> obstacles)
        {
            aStar.Clear();
            List<Vector2> pointsArray = new List<Vector2>();
            for (int y = 0; y < (int)mapSize.y; y++)
            {
                for (int x = 0; x < (int)mapSize.x; x++)
                {
                    Vector2 point = new Vector2(x, y);
                    if (!obstacles.Contains(point))
                    {
                        pointsArray.Add(point);
                        int pointIndex = CalculatePointIndex(point);
                        aStar.AddPoint(pointIndex, new Vector3(point.x, point.y, 0.0f));
                    }
                }
            }
            return pointsArray;
        }
        private void AstarConnectWalkableCellsDiagonal(List<Vector2> pointsArray)
        {
            foreach (Vector2 point in pointsArray)
            {
                int pointIndex = CalculatePointIndex(point);
                for (byte localY = 0; localY < 3; localY++)
                {
                    for (byte localX = 0; localX < 3; localX++)
                    {
                        Vector2 pointLocal = new Vector2(point.x + localX - 1, point.y + localY - 1);
                        int pointLocalIndex = CalculatePointIndex(pointLocal);
                        if (pointLocal.Equals(point) || IsOutsideMapBounds(pointLocal) || !aStar.HasPoint(pointLocalIndex))
                        {
                            continue;
                        }
                        aStar.ConnectPoints(pointIndex, pointLocalIndex, false);
                    }
                }
            }
        }
        private bool IsOutsideMapBounds(Vector2 point)
        {
            return point.x < 0.0f || point.y < 0.0f || point.x >= mapSize.x || point.y >= mapSize.y;
        }
        private int CalculatePointIndex(Vector2 point)
        {
            return (int)(point.x + mapSize.x * point.y);
        }
        private Vector3[] GetRecalulatedPath()
        {
            int startPointIndex = CalculatePointIndex(pathStartPosition);
            int endPointIndex = CalculatePointIndex(pathEndPosition);
            return aStar.GetPointPath(startPointIndex, endPointIndex);
        }
        private bool SetStartPosition(Vector2 point)
        {
            point = mapGrid.WorldToMap(point);
            if (!mapObstacles.Contains(point) && !IsOutsideMapBounds(point))
            {
                pathStartPosition = point;
                return true;
            }
            return false;
        }
        private bool SetEndPosition(Vector2 point)
        {
            point = mapGrid.WorldToMap(point);
            if (!mapObstacles.Contains(point) && !IsOutsideMapBounds(point))
            {
                pathEndPosition = point;
                return true;
            }
            return false;
        }
        private void SetPointWeight(Vector2 worldPosition, float weight)
        {
            int pointIndex = CalculatePointIndex(mapGrid.WorldToMap(worldPosition));
            aStar.SetPointWeightScale(pointIndex, weight);
        }
        public List<Vector2> getAPath(Vector2 worldStart, Vector2 worldEnd)
        {
            List<Vector2> worldPath = new List<Vector2>();
            if (SetStartPosition(worldStart) && SetEndPosition(worldEnd))
            {
                foreach (Vector3 point in GetRecalulatedPath())
                {
                    worldPath.Add(mapGrid.MapToWorld(new Vector2(point.x, point.y)) + HALF_CELL_SIZE);
                }
            }
            return worldPath;
        }
        public Vector2 GetGridPosition(Vector2 worldPosition)
        {
            return mapGrid.MapToWorld(mapGrid.WorldToMap(worldPosition)) + HALF_CELL_SIZE;
        }
        public Vector2 RequestMove(Vector2 currentWorldPosition, Vector2 direction)
        {
            Vector2 cellStart = mapGrid.WorldToMap(currentWorldPosition);
            Vector2 cellTarget = cellStart + direction;
            int pointIndex = CalculatePointIndex(cellTarget);
            if (aStar.HasPoint(pointIndex) && aStar.GetPointWeightScale(pointIndex) == ASTAR_NORMAL_WEIGHT)
            {
                aStar.SetPointWeightScale(CalculatePointIndex(cellStart), ASTAR_NORMAL_WEIGHT);
                aStar.SetPointWeightScale(pointIndex, ASTAR_OCCUPIED_WEIGHT);
                return mapGrid.MapToWorld(cellTarget) + HALF_CELL_SIZE;
            }
            return new Vector2();
        }
        public bool IsValidMove(Vector2 worldPosition)
        {
            int pointIndex = CalculatePointIndex(mapGrid.WorldToMap(worldPosition));
            if (aStar.HasPoint(pointIndex) && aStar.GetPointWeightScale(pointIndex) == ASTAR_NORMAL_WEIGHT)
            {
                return true;
            }
            return false;
        }
        public List<Vector2> ResetPath(List<Vector2> pointList)
        {
            foreach (Vector2 point in pointList)
            {
                SetPointWeight(point, ASTAR_NORMAL_WEIGHT);
            }
            return new List<Vector2>();
        }
        public List<Vector2> GetAttackSlot(Vector2 currentWorldPosition, Vector2 targetWorldPosition)
        {
            Vector2 targetCell = mapGrid.WorldToMap(targetWorldPosition);
            List<Vector2> pointList = new List<Vector2>();
            for (int localY = 0; localY < 3; localY++)
            {
                for (int localX = 0; localX < 3; localX++)
                {
                    Vector2 pointLocal = new Vector2(targetCell.x + localX - 1, targetCell.y + localY - 1);
                    int pointLocalIndex = CalculatePointIndex(pointLocal);
                    if (aStar.HasPoint(pointLocalIndex) && aStar.GetPointWeightScale(pointLocalIndex) != ASTAR_OCCUPIED_WEIGHT)
                    {
                        pointList.Add(pointLocal);
                    }
                }
            }
            Vector2 randomizedSlot = mapGrid.MapToWorld(pointList[(int)GD.Randi() % pointList.Count]) + HALF_CELL_SIZE;
            return getAPath(currentWorldPosition, randomizedSlot);
        }
        private void OccupyOriginCell(Vector2 worldPosition)
        {
            // Used in SetUnits method; used for placing units in they're origin cell
            Vector2 point = mapGrid.WorldToMap(worldPosition);
            if (!mapObstacles.Contains(point) && !IsOutsideMapBounds(point))
            {
                int pointIndex = CalculatePointIndex(point);
                aStar.SetPointWeightScale(pointIndex, ASTAR_OCCUPIED_WEIGHT);
            }
            else
            {
                GD.PrintErr(String.Format("Object incorrectly placed at\ngrid position: {0}\nglobal position: {1}\n",
                    point, mapGrid.MapToWorld(point)));
            }
        }
    }
}