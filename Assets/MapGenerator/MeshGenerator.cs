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

        [SerializeField] SquareGrid m_SquareGrid;
        [SerializeField] MeshFilter m_MeshFilter;

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
        }

        public void GenerateMesh(int[,] map, float squareSize)
        {
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

            mesh.RecalculateNormals();
        }
    }
}