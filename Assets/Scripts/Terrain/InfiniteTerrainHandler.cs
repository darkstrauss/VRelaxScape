using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrainHandler : MonoBehaviour {

    private const float viewerMoveThresholdForChunkUpdate = 25f;
    private const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public static int INDEX_MAPGENERATOR = 0;
    public static int INDEX_CLOUDGENERATOR = 1;
    public static int INDEX_TREEGENERATOR = 2;
    public static int INDEX_PLAYERPARTICLES = 3;
    public static int INDEX_PREVEIWER = 4;

    public List<Transform> m_handlers = new List<Transform>();

    private int m_colliderLODIndex = 0;
    public LODInfo[] m_detailLevels;

    public MeshSettings m_meshSettings;
    public NoiseSettings m_heightMapNoiseSettings;
    public TextureData m_textureSettings;
    public CloudData m_cloudSettings;
    public TreeMapSettings m_treeMapSettings;

    public Transform m_viewer;
    public Light m_sun;

    public Material m_mapMaterial;

    public Vector2 m_viewerPosition;
    private Vector2 m_viewerPositionOld;
    private float m_meshWorldSize;
    private int m_chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> m_terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> m_visibleTerrainChunks = new List<TerrainChunk>();
    List<GameObject> m_visibleClouds = new List<GameObject>();

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        OVRManager.tiledMultiResLevel = OVRManager.TiledMultiResLevel.LMSHigh;
        OVRManager.display.displayFrequency = 72.0f;
        UnityEngine.QualitySettings.lodBias = 1.1f;
#endif

        if (m_sun != null)
        {
            m_sun.flare = m_textureSettings.m_directionalLightFlare;
        }

        m_textureSettings.ApplyToMaterial(m_mapMaterial);
        m_textureSettings.UpdatedMeshHeight(m_mapMaterial, m_heightMapNoiseSettings.GetMinHeight, m_heightMapNoiseSettings.GetMaxHeight);
        float m_maxViewDistance = m_detailLevels[m_detailLevels.Length - 1].visibleDistanceThreshold;
        m_meshWorldSize = m_meshSettings.GetMeshWorldSize;
        m_chunksVisibleInViewDistance = Mathf.RoundToInt(m_maxViewDistance / m_meshWorldSize);

        UpdateVisibleChunks();
    }

    private void Update()
    {
        m_viewerPosition = new Vector2(m_viewer.position.x, m_viewer.position.z);

        if (m_viewerPosition != m_viewerPositionOld)
        {
            foreach (TerrainChunk chunk in m_visibleTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }
        }

        if ((m_viewerPositionOld - m_viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            m_viewerPositionOld = m_viewerPosition;
            UpdateVisibleChunks();
        }

        UpdateVisibleClouds();
    }

    /// <summary>
    /// Handles chunk spawning and updating.
    /// </summary>
    private void UpdateVisibleChunks()
    {
        // We use a hash set instead of a dictionary because we want to make sure that a vector coordinate is never dubplicated.
        HashSet<Vector2> l_alreadyUpdatedChunkCoords = new HashSet<Vector2>();

        // Loop backwards to make sure we don't get any null reference exceptions.
        for (int i = m_visibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            l_alreadyUpdatedChunkCoords.Add(m_visibleTerrainChunks[i].m_coord);
            m_visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        // Simplified coordinate system for ease of use. Terrain chunk coords are represented is multiplication factors.
        int l_currentChunkCoordX = Mathf.RoundToInt(m_viewerPosition.x / m_meshWorldSize);
        int l_currentChunkCoordY = Mathf.RoundToInt(m_viewerPosition.y / m_meshWorldSize);

        for (int yOffset = -m_chunksVisibleInViewDistance; yOffset <= m_chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -m_chunksVisibleInViewDistance; xOffset <= m_chunksVisibleInViewDistance; xOffset++)
            {
                Vector2 l_viewedChunkCoord = new Vector2(l_currentChunkCoordX + xOffset, l_currentChunkCoordY + yOffset);

                // if this chunk is not already contained within the hash set we need to create a new one.
                if (!l_alreadyUpdatedChunkCoords.Contains(l_viewedChunkCoord))
                {
                    if (m_terrainChunkDictionary.ContainsKey(l_viewedChunkCoord))
                    {
                        m_terrainChunkDictionary[l_viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else
                    {
                        // Spawn a new chunk at the given coordinates. Pass on all of the settings from this class.
                        TerrainChunk l_newChunk = new TerrainChunk(l_viewedChunkCoord, m_heightMapNoiseSettings, m_meshSettings, m_treeMapSettings, m_detailLevels, m_colliderLODIndex, transform, m_viewer, m_mapMaterial);
                        m_terrainChunkDictionary.Add(l_viewedChunkCoord, l_newChunk);

                        // Register for callback to handle chunk behavior.
                        l_newChunk.OnVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        l_newChunk.Load();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Handles the clouds that float overhead.
    /// </summary>
    private void UpdateVisibleClouds()
    {
        // If there are clouds, we need to check how visible they are.
        if (m_visibleClouds.Count > 0)
        {
            for (int i = m_visibleClouds.Count - 1; i >= 0; i--)
            {
                // If the scale transform is 0 we delete the object.
                if (m_visibleClouds[i].transform.localScale == Vector3.zero)
                {
                    GameObject m_objectToBeDeleted = m_visibleClouds[i];
                    m_visibleClouds.Remove(m_visibleClouds[i]);
                    Destroy(m_objectToBeDeleted);
                }
            }
        }

        // If there are no clouds we want to spawn as many as the cloud settings has defined.
        if (m_visibleClouds.Count < m_cloudSettings.m_maxPossibleVisibleClouds)
        {
            for (int i = m_visibleClouds.Count; i < m_cloudSettings.m_maxPossibleVisibleClouds; i++)
            {
                // Spawn new game object and pass on the cloud settings.
                GameObject l_cloud = Instantiate(m_cloudSettings.m_clouds[Random.Range(0, m_cloudSettings.m_clouds.Count)]) as GameObject;
                l_cloud.GetComponent<Cloud>().LoadCloudBehaviour(m_cloudSettings, m_viewer.gameObject, m_handlers[INDEX_CLOUDGENERATOR]);
                m_visibleClouds.Add(l_cloud);
            }
        }
    }

    private void OnTerrainChunkVisibilityChanged(TerrainChunk i_chunk, bool i_isVisible)
    {
        if (i_isVisible)
        {
            m_visibleTerrainChunks.Add(i_chunk);
        }
        else
        {
            m_visibleTerrainChunks.Remove(i_chunk);
        }
    }
}

[System.Serializable]
public struct LODInfo
{
    [Range(0, MeshSettings.m_numberOfSupportedLODs - 1)]
    public int lod;
    public float visibleDistanceThreshold;

    public float SqrVisibleDstThreshold
    {
        get
        {
            return visibleDistanceThreshold * visibleDistanceThreshold;
        }
    }
}
