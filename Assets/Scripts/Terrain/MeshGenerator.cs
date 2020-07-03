using UnityEngine;
using System.Collections;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] i_heightMap, MeshSettings i_meshSettings, int i_levelOfDetail)
    {

        int m_skipIncrement = (i_levelOfDetail == 0) ? 1 : i_levelOfDetail * 2;
        int m_numberOfVertsPerLine = i_meshSettings.GetNumberOfVertsPerLine;

        Vector2 m_topLeft = new Vector2(-1, 1) * i_meshSettings.GetMeshWorldSize / 2f;

        MeshData m_meshData = new MeshData(m_numberOfVertsPerLine, m_skipIncrement, i_meshSettings.m_useFlatShading);

        int[,] m_vertexIndicesMap = new int[m_numberOfVertsPerLine, m_numberOfVertsPerLine];
        int m_meshVertexIndex = 0;
        int m_OutOfMeshVertexIndex = -1;

        for (int y = 0; y < m_numberOfVertsPerLine; y++)
        {
            for (int x = 0; x < m_numberOfVertsPerLine; x++)
            {
                bool l_isOutOfMeshVertex = y == 0 || y == m_numberOfVertsPerLine - 1 || x == 0 || x == m_numberOfVertsPerLine - 1;
                bool l_isSkippedVertex = x > 2 && x < m_numberOfVertsPerLine - 3 && y > 2 && y < m_numberOfVertsPerLine - 3 && ((x - 2) % m_skipIncrement != 0 || (y - 2) % m_skipIncrement != 0);

                if (l_isOutOfMeshVertex)
                {
                    m_vertexIndicesMap[x, y] = m_OutOfMeshVertexIndex;
                    m_OutOfMeshVertexIndex--;
                }
                else if (!l_isSkippedVertex)
                {
                    m_vertexIndicesMap[x, y] = m_meshVertexIndex;
                    m_meshVertexIndex++;
                }
            }
        }

        for (int y = 0; y < m_numberOfVertsPerLine; y++)
        {
            for (int x = 0; x < m_numberOfVertsPerLine; x++)
            {
                bool l_isSkippedVertex = x > 2 && x < m_numberOfVertsPerLine - 3 && y > 2 && y < m_numberOfVertsPerLine - 3 && ((x - 2) % m_skipIncrement != 0 || (y - 2) % m_skipIncrement != 0);

                if (!l_isSkippedVertex)
                {
                    bool l_isOutOfMeshVertex = y == 0 || y == m_numberOfVertsPerLine - 1 || x == 0 || x == m_numberOfVertsPerLine - 1;
                    bool l_isMeshEdgeVertex = (y == 1 || y == m_numberOfVertsPerLine - 2 || x == 1 || x == m_numberOfVertsPerLine - 2) && !l_isOutOfMeshVertex;
                    bool l_isMainVertex = (x - 2) % m_skipIncrement == 0 && (y - 2) % m_skipIncrement == 0 && !l_isOutOfMeshVertex && !l_isMeshEdgeVertex;
                    bool l_isEdgeConnectionVertex = (y == 2 || y == m_numberOfVertsPerLine - 3 || x == 2 || x == m_numberOfVertsPerLine - 3) && !l_isOutOfMeshVertex && !l_isMeshEdgeVertex && !l_isMainVertex;


                    int l_vertexIndex = m_vertexIndicesMap[x, y];
                    Vector2 l_percent = new Vector2(x - 1, y - 1) / (m_numberOfVertsPerLine - 3);
                    Vector2 l_vertexPosition2D = m_topLeft + new Vector2(l_percent.x, -l_percent.y) * i_meshSettings.GetMeshWorldSize;
                    float l_height = i_heightMap[x, y];

                    if (l_isEdgeConnectionVertex)
                    {
                        bool l_isVertical = x == 2 || x == m_numberOfVertsPerLine - 3;


                        int l_dstToMainVertexA = ((l_isVertical) ? y - 2 : x - 2) % m_skipIncrement;
                        int l_dstToMainVertexB = m_skipIncrement - l_dstToMainVertexA;
                        float l_dstPercentFromAtoB = l_dstToMainVertexA / (float)m_skipIncrement;

                        float l_heightMainVertexA = i_heightMap[(l_isVertical) ? x : x - l_dstToMainVertexA, (l_isVertical) ? y - l_dstToMainVertexA : y];
                        float l_heightMainVertexB = i_heightMap[(l_isVertical) ? x : x + l_dstToMainVertexB, (l_isVertical) ? y + l_dstToMainVertexB : y];

                        l_height = l_heightMainVertexA * (1 - l_dstPercentFromAtoB) + l_heightMainVertexB * l_dstPercentFromAtoB;
                    }

                    m_meshData.AddVertex(new Vector3(l_vertexPosition2D.x, l_height, l_vertexPosition2D.y), l_percent, l_vertexIndex);

                    bool l_createTriangle = x < m_numberOfVertsPerLine - 1 && y < m_numberOfVertsPerLine - 1 && (!l_isEdgeConnectionVertex || (x != 2 && y != 2));

                    if (l_createTriangle)
                    {
                        int l_currentIncrement = (l_isMainVertex && x != m_numberOfVertsPerLine - 3 && y != m_numberOfVertsPerLine - 3) ? m_skipIncrement : 1;

                        int a = m_vertexIndicesMap[x, y];
                        int b = m_vertexIndicesMap[x + l_currentIncrement, y];
                        int c = m_vertexIndicesMap[x, y + l_currentIncrement];
                        int d = m_vertexIndicesMap[x + l_currentIncrement, y + l_currentIncrement];
                        m_meshData.AddTriangle(a, d, c);
                        m_meshData.AddTriangle(d, a, b);
                    }
                }
            }
        }

        m_meshData.ProcessMesh();

        return m_meshData;

    }
}

public class MeshData
{
    private Vector3[] m_vertices;
    private int[] m_triangles;
    private Vector2[] m_uvs;
    private Vector3[] m_bakedNormals;

    private Vector3[] m_outOfMeshVertices;
    private int[] m_outOfMeshTriangles;

    private int m_triangleIndex;
    private int m_outOfMeshTriangleIndex;

    bool m_isUsingFlatShading;

    public MeshData(int i_numVertsPerLine, int i_skipIncrement, bool i_isUsingFlatShading)
    {
        this.m_isUsingFlatShading = i_isUsingFlatShading;

        int l_numMeshEdgeVertices = (i_numVertsPerLine - 2) * 4 - 4;
        int numEdgeConnectionVertices = (i_skipIncrement - 1) * (i_numVertsPerLine - 5) / i_skipIncrement * 4;
        int numMainVerticesPerLine = (i_numVertsPerLine - 5) / i_skipIncrement + 1;
        int numMainVertices = numMainVerticesPerLine * numMainVerticesPerLine;

        m_vertices = new Vector3[l_numMeshEdgeVertices + numEdgeConnectionVertices + numMainVertices];
        m_uvs = new Vector2[m_vertices.Length];

        int numMeshEdgeTriangles = 8 * (i_numVertsPerLine - 4);
        int numMainTriangles = (numMainVerticesPerLine - 1) * (numMainVerticesPerLine - 1) * 2;
        m_triangles = new int[(numMeshEdgeTriangles + numMainTriangles) * 3];

        m_outOfMeshVertices = new Vector3[i_numVertsPerLine * 4 - 4];
        m_outOfMeshTriangles = new int[24 * (i_numVertsPerLine - 2)];
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        if (vertexIndex < 0)
        {
            m_outOfMeshVertices[-vertexIndex - 1] = vertexPosition;
        }
        else
        {
            m_vertices[vertexIndex] = vertexPosition;
            m_uvs[vertexIndex] = uv;
        }
    }

    public void AddTriangle(int a, int b, int c)
    {
        if (a < 0 || b < 0 || c < 0)
        {
            m_outOfMeshTriangles[m_outOfMeshTriangleIndex] = a;
            m_outOfMeshTriangles[m_outOfMeshTriangleIndex + 1] = b;
            m_outOfMeshTriangles[m_outOfMeshTriangleIndex + 2] = c;
            m_outOfMeshTriangleIndex += 3;
        }
        else
        {
            m_triangles[m_triangleIndex] = a;
            m_triangles[m_triangleIndex + 1] = b;
            m_triangles[m_triangleIndex + 2] = c;
            m_triangleIndex += 3;
        }
    }

    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[m_vertices.Length];
        int triangleCount = m_triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = m_triangles[normalTriangleIndex];
            int vertexIndexB = m_triangles[normalTriangleIndex + 1];
            int vertexIndexC = m_triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        int borderTriangleCount = m_outOfMeshTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = m_outOfMeshTriangles[normalTriangleIndex];
            int vertexIndexB = m_outOfMeshTriangles[normalTriangleIndex + 1];
            int vertexIndexC = m_outOfMeshTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0)
            {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0)
            {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0)
            {
                vertexNormals[vertexIndexC] += triangleNormal;
            }
        }


        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;

    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? m_outOfMeshVertices[-indexA - 1] : m_vertices[indexA];
        Vector3 pointB = (indexB < 0) ? m_outOfMeshVertices[-indexB - 1] : m_vertices[indexB];
        Vector3 pointC = (indexC < 0) ? m_outOfMeshVertices[-indexC - 1] : m_vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;

    }

    public void ProcessMesh()
    {
        if (m_isUsingFlatShading)
        {
            FlatShading();
        }
        else
        {
            BakeNormals();
        }
    }

    private void BakeNormals()
    {
        m_bakedNormals = CalculateNormals();
    }

    private void FlatShading()
    {
        Vector3[] m_flatShadedVertices = new Vector3[m_triangles.Length];
        Vector2[] m_flatShadedUvs = new Vector2[m_triangles.Length];

        for (int i = 0; i < m_triangles.Length; i++)
        {
            m_flatShadedVertices[i] = m_vertices[m_triangles[i]];
            m_flatShadedUvs[i] = m_uvs[m_triangles[i]];
            m_triangles[i] = i;
        }

        m_vertices = m_flatShadedVertices;
        m_uvs = m_flatShadedUvs;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = m_vertices;
        mesh.triangles = m_triangles;
        mesh.uv = m_uvs;

        if (m_isUsingFlatShading)
        {
            mesh.RecalculateNormals();
        }
        else
        {
            mesh.normals = m_bakedNormals;
        }

        return mesh;
    }

}