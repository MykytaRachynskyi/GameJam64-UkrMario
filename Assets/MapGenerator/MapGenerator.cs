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

        [SerializeField] MeshGenerator m_MeshGenerator;

        int[,] m_Map;

        [ExecuteInEditMode]
        [ContextMenu("RandomFillMap")]
        void GenerateMap()
        {
            RandomFillMap();

            for (int i = 0; i < m_SmoothingIterations; i++)
            {
                SmoothMap();
            }

            if (m_MeshGenerator != null)
                m_MeshGenerator.GenerateMesh(m_Map, 1f);
        }

        void RandomFillMap()
        {
            string seed = m_Seed;

            if (m_UseRandomSeed)
            {
                seed = System.Guid.NewGuid().ToString();
            }

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

                    if (neighbourY <= 0)
                        wallCount++;
                    else
                        wallCount += m_Map[neighbourX, neighbourY];
                }
            }

            return wallCount;
        }

        void OnValidate()
        {
            GenerateMap();
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