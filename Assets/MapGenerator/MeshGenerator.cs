using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TMS.MapGenerator
{
    public class MeshGenerator : MonoBehaviour
    {
        public class SquareGrid
        {
            public Square[,] squares;

            public SquareGrid(int[,] map, float squareSize)
            {
                int nodeCountX = map.GetLength(0);
                int nodeCountY = map.GetLength(1);

                float mapWidth = nodeCountX * squareSize;
                float mapHeight = nodeCountY * squareSize;

                ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

                for (int x = 0; x < nodeCountX; x++)
                {
                    for (int y = 0; y < nodeCountY; y++)
                    {
                        Vector3 position =
                            new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2f,
                                        0,
                                        -mapHeight / 2 + y * squareSize + squareSize / 2f);

                        controlNodes[x, y] = new ControlNode(position, map[x, y] == 1, squareSize);
                    }
                }

                squares = new Square[nodeCountX - 1, nodeCountY - 1];

                for (int x = 0; x < nodeCountX - 1; x++)
                {
                    for (int y = 0; y < nodeCountY - 1; y++)
                    {
                        squares[x, y] = new Square(controlNodes[x, y + 1],
                                                   controlNodes[x + 1, y + 1],
                                                   controlNodes[x, y],
                                                   controlNodes[x + 1, y]);
                    }
                }
            }
        }

        public class Square
        {
            public ControlNode topLeft;
            public ControlNode topRight;
            public ControlNode bottomRight;
            public ControlNode bottomLeft;

            public Node centerTop;
            public Node centerRight;
            public Node centerBottom;
            public Node centerLeft;

            public int configuration;

            public Square(ControlNode topLeft, ControlNode topRight,
                          ControlNode bottomLeft, ControlNode bottomRight)
            {
                this.topLeft = topLeft;
                this.topRight = topRight;
                this.bottomRight = bottomRight;
                this.bottomLeft = bottomLeft;

                this.centerTop = topLeft.right;
                this.centerRight = bottomRight.above;
                this.centerBottom = bottomLeft.right;
                this.centerLeft = bottomLeft.above;

                if (topLeft.active) configuration += 8;
                if (topRight.active) configuration += 4;
                if (bottomRight.active) configuration += 2;
                if (bottomLeft.active) configuration += 1;
            }
        }
        public class Node
        {
            public Vector3 position;
            public int vertexIndex = -1;

            public Node(Vector3 position)
            {
                this.position = position;
            }
        }

        public class ControlNode : Node
        {
            public bool active;
            public Node above;
            public Node right;

            public ControlNode(Vector3 position, bool active, float squareSize) : base(position)
            {
                this.active = active;
                above = new Node(position + Vector3.forward * squareSize / 2f);
                right = new Node(position + Vector3.right * squareSize / 2f);
            }
        }

        struct Triangle
        {
            public int vertexIndexA;
            public int vertexIndexB;
            public int vertexIndexC;

            int[] vertices;

            public Triangle(int a, int b, int c)
            {
                vertexIndexA = a;
                vertexIndexB = b;
                vertexIndexC = c;

                vertices = new int[3];
                vertices[0] = a;
                vertices[1] = b;
                vertices[2] = c;
            }

            public bool Contains(int vertIndex)
            {
                return vertIndex == vertexIndexA || vertIndex == vertexIndexB || vertIndex == vertexIndexC;
            }

            public int this[int i]
            {
                get
                {
                    return vertices[i];
                }
            }
        }

        [SerializeField] SquareGrid m_SquareGrid;
        [SerializeField] MeshFilter m_MeshFilter;
        [SerializeField] MeshFilter m_WallMeshFilter;

        Dictionary<int, List<Triangle>> m_VertexTriangleMap = new Dictionary<int, List<Triangle>>();
        List<List<int>> m_Outlines = new List<List<int>>();
        HashSet<int> m_CheckedVertices = new HashSet<int>();

        List<Vector3> m_Vertices = new List<Vector3>();
        List<int> m_Triangles = new List<int>();

        void TriangulateSquare(Square square)
        {
            switch (square.configuration)
            {
                case 1:
                    MeshFromPoints(square.centerLeft, square.centerBottom, square.bottomLeft);
                    break;
                case 2:
                    MeshFromPoints(square.bottomRight, square.centerBottom, square.centerRight);
                    break;
                case 4:
                    MeshFromPoints(square.topRight, square.centerRight, square.centerTop);
                    break;
                case 8:
                    MeshFromPoints(square.topLeft, square.centerTop, square.centerLeft);
                    break;

                // 2 points:
                case 3:
                    MeshFromPoints(square.centerRight, square.bottomRight, square.bottomLeft, square.centerLeft);
                    break;
                case 6:
                    MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.centerBottom);
                    break;
                case 9:
                    MeshFromPoints(square.topLeft, square.centerTop, square.centerBottom, square.bottomLeft);
                    break;
                case 12:
                    MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerLeft);
                    break;
                case 5:
                    MeshFromPoints(square.centerTop, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft, square.centerLeft);
                    break;
                case 10:
                    MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.centerBottom, square.centerLeft);
                    break;

                // 3 point:
                case 7:
                    MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.bottomLeft, square.centerLeft);
                    break;
                case 11:
                    MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.bottomLeft);
                    break;
                case 13:
                    MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft);
                    break;
                case 14:
                    MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centerBottom, square.centerLeft);
                    break;

                // 4 point:
                case 15:
                    MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                    m_CheckedVertices.Add(square.topLeft.vertexIndex);
                    m_CheckedVertices.Add(square.topRight.vertexIndex);
                    m_CheckedVertices.Add(square.bottomRight.vertexIndex);
                    m_CheckedVertices.Add(square.bottomLeft.vertexIndex);
                    break;
            }
        }

        void MeshFromPoints(params Node[] points)
        {
            AsssignVertices(points);

            if (points.Length >= 3)
            {
                CreateTriangle(points[0], points[1], points[2]);
            }
            if (points.Length >= 4)
            {
                CreateTriangle(points[0], points[2], points[3]);
            }
            if (points.Length >= 5)
            {
                CreateTriangle(points[0], points[3], points[4]);
            }
            if (points.Length >= 6)
            {
                CreateTriangle(points[0], points[4], points[5]);
            }
        }

        void AsssignVertices(Node[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].vertexIndex == -1)
                {
                    points[i].vertexIndex = m_Vertices.Count;
                    m_Vertices.Add(points[i].position);
                }
            }
        }

        void CreateTriangle(Node a, Node b, Node c)
        {
            m_Triangles.Add(a.vertexIndex);
            m_Triangles.Add(b.vertexIndex);
            m_Triangles.Add(c.vertexIndex);

            Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
            AddTriangleToMap(a.vertexIndex, triangle);
            AddTriangleToMap(b.vertexIndex, triangle);
            AddTriangleToMap(c.vertexIndex, triangle);
        }

        int GetConnectedOutlineVertex(int vertexIndex)
        {
            List<Triangle> trianglesContainingVertex = m_VertexTriangleMap[vertexIndex];

            for (int i = 0; i < trianglesContainingVertex.Count; i++)
            {
                var triangle = trianglesContainingVertex[i];

                for (int j = 0; j < 3; j++)
                {
                    int vertexB = triangle[j];

                    if (vertexB == vertexIndex)
                        continue;

                    if (m_CheckedVertices.Contains(vertexB))
                        continue;

                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }

            return -1;
        }

        void CalculateMeshOutline()
        {
            for (int vertexIndex = 0; vertexIndex < m_Vertices.Count; vertexIndex++)
            {
                if (!m_CheckedVertices.Contains(vertexIndex))
                {
                    int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                    if (newOutlineVertex != -1)
                    {
                        m_CheckedVertices.Add(vertexIndex);

                        List<int> newOutline = new List<int>();
                        newOutline.Add(vertexIndex);
                        m_Outlines.Add(newOutline);

                        FollowOutline(newOutlineVertex, m_Outlines.Count - 1);
                        m_Outlines[m_Outlines.Count - 1].Add(vertexIndex);
                    }
                }
            }
        }

        void FollowOutline(int vertexIndex, int outlineIndex)
        {
            m_Outlines[outlineIndex].Add(vertexIndex);
            m_CheckedVertices.Add(vertexIndex);
            int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

            if (nextVertexIndex != -1)
            {
                FollowOutline(nextVertexIndex, outlineIndex);
            }
        }

        bool IsOutlineEdge(int vertexA, int vertexB)
        {
            var vertexATriangles = m_VertexTriangleMap[vertexA];

            int sharedTrianglesCount = 0;
            for (int i = 0; i < vertexATriangles.Count; i++)
            {
                if (vertexATriangles[i].Contains(vertexB))
                {
                    sharedTrianglesCount++;
                    if (sharedTrianglesCount > 1)
                        return false;
                }
            }

            return sharedTrianglesCount == 1;
        }

        void AddTriangleToMap(int vertexKey, Triangle triangle)
        {
            if (!m_VertexTriangleMap.ContainsKey(vertexKey))
                m_VertexTriangleMap.Add(vertexKey, new List<Triangle>());

            m_VertexTriangleMap[vertexKey].Add(triangle);
        }

        public void GenerateMesh(int[,] map, float squareSize)
        {
            m_VertexTriangleMap.Clear();
            m_CheckedVertices.Clear();
            m_Outlines.Clear();
            m_Vertices.Clear();
            m_Triangles.Clear();

            m_SquareGrid = new SquareGrid(map, squareSize);
            for (int x = 0; x < m_SquareGrid.squares.GetLength(0); x++)
            {
                for (int y = 0; y < m_SquareGrid.squares.GetLength(1); y++)
                {
                    TriangulateSquare(m_SquareGrid.squares[x, y]);
                }
            }

            Mesh mesh = new Mesh();
            m_MeshFilter.mesh = mesh;

            mesh.vertices = m_Vertices.ToArray();
            mesh.triangles = m_Triangles.ToArray();

            CreateWallMesh(m_WallMeshFilter);
            mesh.RecalculateNormals();

            m_WallMeshFilter.gameObject.GetComponent<MeshCollider>().sharedMesh = m_WallMeshFilter.sharedMesh;
        }

        void CreateWallMesh(MeshFilter wallFilter)
        {
            CalculateMeshOutline();

            List<Vector3> wallVertices = new List<Vector3>();
            List<int> wallTriangles = new List<int>();

            Mesh wallMesh = new Mesh();
            float wallHeight = 5;

            foreach (var outline in m_Outlines)
            {
                for (int i = 0; i < outline.Count - 1; i++)
                {
                    int startIndex = wallVertices.Count;
                    wallVertices.Add(m_Vertices[outline[i]]);
                    wallVertices.Add(m_Vertices[outline[i + 1]]);

                    wallVertices.Add(m_Vertices[outline[i]] - Vector3.up * wallHeight);
                    wallVertices.Add(m_Vertices[outline[i + 1]] - Vector3.up * wallHeight);

                    wallTriangles.Add(startIndex + 0);
                    wallTriangles.Add(startIndex + 2);
                    wallTriangles.Add(startIndex + 3);

                    wallTriangles.Add(startIndex + 3);
                    wallTriangles.Add(startIndex + 1);
                    wallTriangles.Add(startIndex + 0);
                }
            }

            wallMesh.vertices = wallVertices.ToArray();
            wallMesh.triangles = wallTriangles.ToArray();
            wallMesh.RecalculateNormals();

            wallFilter.mesh = wallMesh;

            //var baseMeshVerts = mesh.vertices;
            //List<Vector3> combinedVerts = new List<Vector3>();
            //combinedVerts.AddRange(baseMeshVerts);
            //combinedVerts.AddRange(wallVertices);
            //mesh.vertices = combinedVerts.ToArray();

            //var baseMeshTriangles = mesh.triangles;
            //List<int> combinedTriangles = new List<int>();
            //combinedTriangles.AddRange(baseMeshTriangles);
            //combinedTriangles.AddRange(wallTriangles);
            //mesh.triangles = combinedTriangles.ToArray();
        }
    }
}