using System.Collections;
using UnityEngine;

[CreateAssetMenu()]
public class MeshSettings : UpdateableData
{
    public const int m_numberOfSupportedLODs = 5;
    public const int m_numberOfSupportedChunkSizes = 10;
    public const int m_numberOfSupportedFlatshadedChunkSizes = 4;
    public static readonly int[] m_supportedChunkSizes = { 24, 48, 72, 96, 120, 144, 168, 192, 216, 240 };

    public float m_meshScale = 2f;

    public bool m_useFlatShading;

    [Range(0, m_numberOfSupportedChunkSizes - 1)]
    public int chunkSizeIndex;
    [Range(0, m_numberOfSupportedFlatshadedChunkSizes - 1)]
    public int flatShadedChunkSizeIndex;


    /// <summary>
    /// Number of verts per line of mesh rendered at LOD = 0. Includes the 2 extra verts that are excluded from final mesh, but used for calculating normals.
    /// </summary>
    public int GetNumberOfVertsPerLine
    {
        get
        {
            return m_supportedChunkSizes[(m_useFlatShading) ? flatShadedChunkSizeIndex : chunkSizeIndex] + 5;
        }
    }

    public float GetMeshWorldSize
    {
        get
        {
            return (GetNumberOfVertsPerLine - 3) * m_meshScale;
        }
    }
}
