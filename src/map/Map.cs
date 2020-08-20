using System.Collections.Generic;
using System;
using Game.Database;
using Godot;
namespace Game.Map
{
    public class Map : Node2D
    {
        private TileMap mapGrid;
        private readonly AStar2D aStar = new AStar2D();
        private readonly Vector2 HALF_CELL_SIZE = new Vector2(8.0f, 8.0f);
        private const float ASTAR_OCCUPIED_WEIGHT = 50.0f;
        private const float ASTAR_ITEM_WEIGHT = 25.0f;
        private const float ASTAR_NORMAL_WEIGHT = 1.0f;
        private List<Vector2> mapObstacles = new List<Vector2>();
        private Vector2 mapSize;
        private Vector2 pathStartPosition;
        private Vector2 pathEndPosition;
        public static Map map;

        public Map()
        {
            map = this;
        }
        public override void _Ready()
        {
            mapGrid = GetNode<TileMap>("meta/coll_nav");
            mapSize = mapGrid.GetUsedRect().Size;
            MakeNav();
            SetVeilSize();
            // SetPlayerCameraLimits();
        }
        public void AddZChild(Node node)
        {
            GetNode("zed/z1").AddChild(node);
        }
        public void SetVeil(bool on)
        {
            Particles2D veil = GetNode<Particles2D>("veil_fog");
            veil.Emitting = on;
            if (on)
            {
                Material = (ShaderMaterial)GD.Load("res://asset/img/map/veil.tres");
                veil.Show();
            }
            else
            {
                Material = null;
                veil.Hide();
            }
        }
        private void SetPlayerCameraLimits()
        {
            Rect2 mapBorders = mapGrid.GetUsedRect();
            float yOffset = HALF_CELL_SIZE.y * -2.0f;
            Vector2 mapCellSize = mapGrid.CellSize;
            Camera2D playerCamera = Actor.Player.player.GetNode<Camera2D>("img/camera");
            playerCamera.LimitLeft = (int)(mapBorders.Position.x * mapCellSize.x);
            playerCamera.LimitRight = (int)(mapBorders.End.x * mapCellSize.x);
            playerCamera.LimitTop = (int)(mapBorders.Position.y * mapCellSize.y + yOffset);
            playerCamera.LimitBottom = (int)(mapBorders.End.y * mapCellSize.y + yOffset);
        }
        private void SetVeilSize()
        {
            Vector2 veilSize = mapSize * HALF_CELL_SIZE;
            Particles2D veil = GetNode<Particles2D>("veil_fog");
            ParticlesMaterial veilMaterial = (ParticlesMaterial)veil.ProcessMaterial;
            veilMaterial.EmissionBoxExtents = new Vector3(veilSize.x, veilSize.y, 0.0f);
            veil.Amount = (int)((veilSize.x + veilSize.y) / 2.0f);
            veil.VisibilityRect = new Rect2(-veilSize, veilSize * 2.0f);
            veil.GlobalPosition = veilSize;
        }
        private void MakeNav()
        {
            aStar.Clear();

            int y;
            int x;
            int tileId;
            Vector2 point;
            int pointIndex;

            byte adjY;
            byte adjX;
            Vector2 pointAdj;
            int pointAdjIndex;
            int adjTileId;

            CollNavDB.collNavTile from;
            CollNavDB.Ordinal ordinal;
            Vector2 direction;

            // for each tile in map
            for (y = 0; y < (int)mapSize.y; y++)
            {
                for (x = 0; x < (int)mapSize.x; x++)
                {
                    tileId = mapGrid.GetCell(x, y);
                    if (Enum.IsDefined(typeof(CollNavDB.collNavTile), tileId) && tileId != (int)CollNavDB.collNavTile.BLOCK)
                    {
                        from = (CollNavDB.collNavTile)tileId;
                        point = new Vector2(x, y);
                        pointIndex = CalculatePointIndex(point);

                        // Add node
                        aStar.AddPoint(pointIndex, point);

                        // add edges from neighbors
                        for (adjY = 0; adjY < 3; adjY++)
                        {
                            for (adjX = 0; adjX < 3; adjX++)
                            {
                                pointAdj = new Vector2(point.x + adjX - 1, point.y + adjY - 1);
                                pointAdjIndex = CalculatePointIndex(pointAdj);

                                if (pointIndex == pointAdjIndex || IsOutsideMapBounds(pointAdj) || !aStar.HasPoint(pointAdjIndex))
                                {
                                    continue;
                                }

                                adjTileId = mapGrid.GetCellv(pointAdj);
                                if (Enum.IsDefined(typeof(CollNavDB.collNavTile), adjTileId) && adjTileId != (int)CollNavDB.collNavTile.BLOCK)
                                {
                                    direction = pointAdj - point;
                                    if (direction.x == -1 && direction.y == -1)
                                    {
                                        ordinal = CollNavDB.Ordinal.NW;
                                    }
                                    else if (direction.x == 0 && direction.y == -1)
                                    {
                                        ordinal = CollNavDB.Ordinal.N;
                                    }
                                    else if (direction.x == 1 && direction.y == -1)
                                    {
                                        ordinal = CollNavDB.Ordinal.NE;
                                    }
                                    else if (direction.x == -1 && direction.y == 0)
                                    {
                                        ordinal = CollNavDB.Ordinal.W;
                                    }
                                    else if (direction.x == 1 && direction.y == 0)
                                    {
                                        ordinal = CollNavDB.Ordinal.E;
                                    }
                                    else if (direction.x == -1 && direction.y == 1)
                                    {
                                        ordinal = CollNavDB.Ordinal.SW;
                                    }
                                    else if (direction.x == 0 && direction.y == 1)
                                    {
                                        ordinal = CollNavDB.Ordinal.S;
                                    }
                                    else if (direction.x == 1 && direction.y == 1)
                                    {
                                        ordinal = CollNavDB.Ordinal.SE;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (CollNavDB.CanConnect(from, (CollNavDB.collNavTile)adjTileId, ordinal))
                                    {
                                        aStar.AddPoint(pointAdjIndex, pointAdj);
                                        aStar.ConnectPoints(pointIndex, pointAdjIndex);
                                    }
                                }
                            }
                        }
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
        private Vector2[] GetRecalulatedPath()
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
                foreach (Vector2 point in GetRecalulatedPath())
                {
                    worldPath.Add(mapGrid.MapToWorld(point) + HALF_CELL_SIZE);
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
            int targetPointIndex = CalculatePointIndex(cellTarget);
            if (aStar.HasPoint(targetPointIndex) && aStar.GetPointWeightScale(targetPointIndex) == ASTAR_NORMAL_WEIGHT)
            {
                int currentPointIndex = CalculatePointIndex(cellStart);
                if (aStar.GetPointWeightScale(currentPointIndex) != ASTAR_ITEM_WEIGHT)
                {
                    aStar.SetPointWeightScale(currentPointIndex, ASTAR_NORMAL_WEIGHT);
                }
                aStar.SetPointWeightScale(targetPointIndex, ASTAR_OCCUPIED_WEIGHT);
                return mapGrid.MapToWorld(cellTarget) + HALF_CELL_SIZE;
            }
            return new Vector2();
        }
        public bool IsValidMove(Vector2 worldPosition)
        {
            int pointIndex = CalculatePointIndex(mapGrid.WorldToMap(worldPosition));
            return aStar.HasPoint(pointIndex) && aStar.GetPointWeightScale(pointIndex) == ASTAR_NORMAL_WEIGHT;
        }
        public void ResetPath(List<Vector2> pointList)
        {
            foreach (Vector2 point in pointList)
            {
                SetPointWeight(point, ASTAR_NORMAL_WEIGHT);
            }
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
            GD.Randomize();
            Vector2 randomizedSlot = mapGrid.MapToWorld(pointList[(int)GD.Randi() % pointList.Count]) + HALF_CELL_SIZE;
            return getAPath(currentWorldPosition, randomizedSlot);
        }
        public Vector2 SetGetPickableLoc(Vector2 worldPosition, bool setWeight)
        {
            SetPointWeight(worldPosition, (setWeight) ? ASTAR_ITEM_WEIGHT : ASTAR_NORMAL_WEIGHT);
            return GetGridPosition(worldPosition);
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
                GD.Print($"Object incorrectly placed at\ngrid position: {point}\nglobal position: {mapGrid.MapToWorld(point)}\n");
            }
        }
    }
}