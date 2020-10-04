using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TMS.MapGenerator
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] int m_Width;
        [SerializeField] int m_Height;

        [Range(0, 100)]
        [SerializeField] int m_RandomFillPercent;

        [SerializeField] string m_Seed;
        [SerializeField] bool m_UseRandomSeed;

        [SerializeField] int m_SmoothingIterations = 5;

        [Range(0, 20)]
        [SerializeField] int m_FloorLevel;
        [Range(1, 50)]
        [SerializeField] int m_BorderThickness;

        [SerializeField] MeshGenerator m_MeshGenerator;

        [SerializeField] int[,] m_InitialMap;
        [SerializeField] int[,] m_Map;
        [SerializeField] int[,] m_GroundMap;

        [ExecuteInEditMode]
        [ContextMenu("GenerateMap")]
        void GenerateMap()
        {
            RandomFillMap();

            for (int i = 0; i < m_SmoothingIterations; i++)
            {
                SmoothMap();
            }

            //int borderSize = m_BorderThickness;
            //int[,] borderedMap = new int[m_Width + borderSize * 2, m_Height + borderSize * 2];
            //
            //for (int x = 0; x < borderedMap.GetLength(0); x++)
            //{
            //    for (int y = 0; y < borderedMap.GetLength(1); y++)
            //    {
            //        if (x >= borderSize && x < m_Width + borderSize &&
            //            y >= borderSize && y < m_Height + borderSize)
            //        {
            //            borderedMap[x, y] = m_Map[x - borderSize, y - borderSize];
            //        }
            //        else if (y < m_Height)
            //        {
            //            borderedMap[x, y] = 1;
            //        }
            //    }
            //}

            RequestGenerateMesh(m_Map);
        }

        [ContextMenu("SaveMap")]
        void SaveMap()
        {
            if (m_Map == null || m_Map.Length == 0)
                return;

            m_InitialMap = m_Map;
        }

        [ContextMenu("ResetToSavedMap")]
        void ResetToSavedMap()
        {
            if (m_InitialMap == null || m_InitialMap.Length == 0)
                return;

            m_Map = m_InitialMap;
        }

        [ContextMenu("RemoveSmallIslands")]
        public void RemoveSmallIslands()
        {
            List<HashSet<Vector2>> islands = new List<HashSet<Vector2>>();

            HashSet<Vector2> visitedCells = new HashSet<Vector2>();

            for (int x = 0; x < m_Map.GetLength(0); x++)
            {
                for (int y = 0; y < m_Map.GetLength(1); y++)
                {
                    if (m_Map[x, y] == 0)
                        continue;

                    if (visitedCells.Contains(new Vector2(x, y)))
                        continue;

                    HashSet<Vector2> island = new HashSet<Vector2>();
                    islands.Add(island);
                    AddToIsland(x, y, visitedCells, island);
                }
            }

            Debug.Log(islands.Count + " islands");
            foreach (var island in islands)
            {
                if (island.Count < 10)
                {
                    foreach (var cell in island)
                    {
                        m_Map[(int)cell.x, (int)cell.y] = 0;
                    }
                }
            }

            //RequestGenerateMesh();
        }

        void AddToIsland(int x, int y, HashSet<Vector2> visitedCells, HashSet<Vector2> island)
        {
            var coords = new Vector2(x, y);

            if (visitedCells.Contains(coords))
                return;

            if (m_Map[x, y] == 0)
                return;

            visitedCells.Add(coords);
            island.Add(coords);

            if (x > 0)
                AddToIsland(x - 1, y, visitedCells, island);
            if (x < m_Map.GetLength(0) - 1)
                AddToIsland(x + 1, y, visitedCells, island);
            if (y > 0)
                AddToIsland(x, y - 1, visitedCells, island);
            if (y < m_Map.GetLength(1) - 1)
                AddToIsland(x, y + 1, visitedCells, island);
        }

        [ContextMenu("RequestGenerateMesh")]
        void RequestGenerateMesh(int[,] map)
        {
            if (m_MeshGenerator != null)
                m_MeshGenerator.GenerateMesh(map, 1f);
        }

        void RandomFillMap()
        {
            //string seed = m_Seed;
            //
            //if (m_UseRandomSeed)
            //{
            string seed = System.Guid.NewGuid().ToString();
            //}

            System.Random pseudoRandom = new System.Random(seed.GetHashCode());

            m_Map = new int[m_Width, m_Height];

            for (int x = 0; x < m_Width; x++)
            {
                for (int y = 0; y < m_Height; y++)
                {
                    if (y == 0)
                    {
                        m_Map[x, y] = 1;
                        continue;
                    }

                    bool isWall = pseudoRandom.Next(0, 100) < m_RandomFillPercent;
                    m_Map[x, y] = isWall ? 1 : 0;
                }
            }
        }

        void SmoothMap()
        {
            for (int x = 0; x < m_Width; x++)
            {
                for (int y = 0; y < m_Height; y++)
                {
                    int neighbourWallTiles = GetNeighbourWallCount(x, y);

                    if (neighbourWallTiles > 4)
                        m_Map[x, y] = 1;
                    else if (neighbourWallTiles < 4)
                        m_Map[x, y] = 0;
                }
            }
        }

        int GetNeighbourWallCount(int x, int y)
        {
            int wallCount = 0;
            for (int neighbourX = x - 1; neighbourX <= x + 1; neighbourX++)
            {
                for (int neighbourY = y - 1; neighbourY <= y + 1; neighbourY++)
                {
                    if (neighbourX == x && neighbourY == y)
                        continue;

                    if (neighbourX < 0 || neighbourX >= m_Width)
                        continue;

                    if (neighbourY >= m_Height)
                        continue;

                    if (neighbourY <= m_FloorLevel)
                        wallCount++;
                    else
                        wallCount += m_Map[neighbourX, neighbourY];
                }
            }

            return wallCount;
        }

        void OnValidate()
        {
            //GenerateMap();
        }

        [ExecuteInEditMode]
        void OnDrawGizmos()
        {
            // DrawMapGizmos();
        }

        void DrawMapGizmos()
        {
            if (m_Map == null || m_Map.Length == 0)
                return;
            for (int x = 0; x < m_Width; x++)
            {
                for (int y = 0; y < m_Height; y++)
                {
                    bool isWall = m_Map[x, y] > 0;
                    Gizmos.color = isWall ? Color.black : Color.white;
                    m_Map[x, y] = isWall ? 1 : 0;

                    Vector3 pos = new Vector3(-m_Width / 2 + x + .5f, 0f, -m_Height / 2 + y + .5f);

                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }
}