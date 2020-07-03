using UnityEngine;
using System.Collections.Generic;

public class TerrainChunk
{
    private const float colliderGenerationDistanceThreshold = 10;
    public event System.Action<TerrainChunk, bool> OnVisibilityChanged;

    public Vector2 m_coord;

    private GameObject m_meshObject;
    private Vector2 m_sampleCenter;
    private Bounds m_bounds;

    private MeshRenderer m_meshRenderer;
    private MeshFilter m_meshFilter;
    private MeshCollider m_meshCollider;

    private LODInfo[] m_detailLevels;
    private LODMesh[] m_lodMeshes;
    private int m_colliderLODIndex;

    private HeightMap m_terrainHeightMap;
    private HeightMap m_treeHeightMap;
    private bool m_isHeightMapRecieved;
    private int m_previousLODIndex = -1;
    private bool m_hasSetCollider;
    private float m_maxViewDistance;

    private NoiseSettings m_heightMapSettings;
    private MeshSettings m_meshSettings;
    private Transform m_viewer;
    private TreeMapSettings m_treeMapSettings;
    private List<GameObject> m_treesPlacedInChunk = new List<GameObject>();


    /// <summary>
    /// Terrain Chunk spawner
    /// </summary>
    /// <param name="i_coord">Where in coordinate space will this chunk be placed</param>
    /// <param name="i_heightMapSettings">Height map data</param>
    /// <param name="i_meshSettings">Mesh data</param>
    /// <param name="i_treeMapSettings">Tree map data</param>
    /// <param name="i_detailLevels">How many LODs are there</param>
    /// <param name="i_colliderLODIndex">What LOD is used to generate mesh collider</param>
    /// <param name="i_parent">What is this object a child of</param>
    /// <param name="i_viewer">Player transform</param>
    /// <param name="i_material">Terrain material</param>
    public TerrainChunk(Vector2 i_coord, NoiseSettings i_heightMapSettings, MeshSettings i_meshSettings, TreeMapSettings i_treeMapSettings, LODInfo[] i_detailLevels, int i_colliderLODIndex, Transform i_parent, Transform i_viewer, Material i_material)
    {
        this.m_coord = i_coord;
        this.m_detailLevels = i_detailLevels;
        this.m_colliderLODIndex = i_colliderLODIndex;
        this.m_heightMapSettings = i_heightMapSettings;
        this.m_meshSettings = i_meshSettings;
        this.m_treeMapSettings = i_treeMapSettings;
        this.m_viewer = i_viewer;

        m_sampleCenter = m_coord * m_meshSettings.GetMeshWorldSize / m_meshSettings.m_meshScale;
        Vector2 l_position = m_coord * m_meshSettings.GetMeshWorldSize;
        m_bounds = new Bounds(l_position, Vector2.one * m_meshSettings.GetMeshWorldSize);

        m_meshObject = new GameObject("Terrain Chunk");
        m_meshRenderer = m_meshObject.AddComponent<MeshRenderer>();
        m_meshFilter = m_meshObject.AddComponent<MeshFilter>();
        m_meshCollider = m_meshObject.AddComponent<MeshCollider>();
        m_meshRenderer.material = i_material;

        m_meshObject.transform.position = new Vector3(l_position.x, 0, l_position.y);
        m_meshObject.transform.parent = i_parent;

        SetVisible(false);

        m_lodMeshes = new LODMesh[m_detailLevels.Length];
        for (int i = 0; i < m_detailLevels.Length; i++)
        {
            m_lodMeshes[i] = new LODMesh(m_detailLevels[i].lod);
            m_lodMeshes[i].updateCallback += UpdateTerrainChunk;
            if (i == m_colliderLODIndex)
            {
                m_lodMeshes[i].updateCallback += UpdateCollisionMesh;
            }
        }

        m_maxViewDistance = m_detailLevels[m_detailLevels.Length - 1].visibleDistanceThreshold;
    }

    public void Load()
    {
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(m_meshSettings.GetNumberOfVertsPerLine, m_meshSettings.GetNumberOfVertsPerLine, m_heightMapSettings, m_sampleCenter), OnHeightMapRecieved);
    }

    /// <summary>
    /// Places trees based on a compared noise map to create natural bloches of trees in the world.
    /// </summary>
    /// <param name="i_heightMapObject">Tree height map</param>
    private void OnTreeMapRecieved(object i_heightMapObject)
    {
        this.m_treeHeightMap = (HeightMap)i_heightMapObject;

        Random.InitState(m_treeMapSettings.m_noiseSettings.m_seed);

        for (int i = 2; i < m_treeHeightMap.m_values.GetLength(0) - 2; i += 2)
        {
            for (int j = 2; j < m_treeHeightMap.m_values.GetLength(1) - 2; j += 2)
            {
                if (m_treeHeightMap.m_values[i, j] == 1)
                {
                    GameObject l_newTree = GameObject.Instantiate(m_treeMapSettings.m_trees[(int)Random.Range(0, m_treeMapSettings.m_trees.Count)]) as GameObject;
                    l_newTree.transform.parent = this.m_meshObject.transform;

                    Vector2 l_topLeft = new Vector2(-1, 1) * m_meshSettings.GetMeshWorldSize / 2f;

                    Vector3 l_treePosition = new Vector3(m_meshObject.transform.position.x - (l_topLeft.y - i * m_meshSettings.m_meshScale),
                                                        m_terrainHeightMap.m_values[i, j],
                                                        m_meshObject.transform.position.z - (l_topLeft.x + j * m_meshSettings.m_meshScale));

                    l_treePosition += RandomOffset();

                    l_newTree.transform.position = l_treePosition;
                    m_treesPlacedInChunk.Add(l_newTree);
                }
            }
        }
    }

    private Vector3 RandomOffset()
    {
        Vector3 l_randomOffset = new Vector3(Random.value, 0, Random.value) * 1.5f;
        l_randomOffset *= m_meshSettings.m_meshScale;

        return l_randomOffset;
    }

    private void OnHeightMapRecieved(object i_heightMapObject)
    {
        this.m_terrainHeightMap = (HeightMap)i_heightMapObject;
        m_isHeightMapRecieved = true;

        UpdateTerrainChunk();

        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMapByComparison(m_meshSettings.GetNumberOfVertsPerLine - 3, m_meshSettings.GetNumberOfVertsPerLine - 3, m_heightMapSettings, m_treeMapSettings.m_noiseSettings, m_sampleCenter), OnTreeMapRecieved);
    }

    Vector2 GetViewerPosition
    {
        get
        {
            return new Vector2(m_viewer.position.x, m_viewer.position.z);
        }
    }

    public void UpdateTerrainChunk()
    {
        if (m_isHeightMapRecieved)
        {
            float l_viewerDistanceFromNearestEdge = Mathf.Sqrt(m_bounds.SqrDistance(GetViewerPosition));
            bool l_wasVisible = IsVisible();
            bool l_isChunkVisible = l_viewerDistanceFromNearestEdge <= m_maxViewDistance;

            if (l_isChunkVisible)
            {
                int l_lodIndex = 0;

                for (int i = 0; i < m_detailLevels.Length - 1; i++)
                {
                    if (l_viewerDistanceFromNearestEdge > m_detailLevels[i].visibleDistanceThreshold)
                    {
                        l_lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (l_lodIndex != m_previousLODIndex)
                {
                    LODMesh l_lodMesh = m_lodMeshes[l_lodIndex];
                    if (l_lodMesh.m_isRecievedMesh)
                    {
                        m_previousLODIndex = l_lodIndex;
                        m_meshFilter.mesh = l_lodMesh.m_mesh;
                    }
                    else if (!l_lodMesh.m_isRequestedMesh)
                    {
                        l_lodMesh.RequestMesh(m_terrainHeightMap, m_meshSettings);
                    }
                }

            }

            if (l_wasVisible != l_isChunkVisible)
            {
                SetVisible(l_isChunkVisible);

                if (OnVisibilityChanged != null)
                {
                    OnVisibilityChanged(this, l_isChunkVisible);
                }
            }
        }
    }

    public void UpdateCollisionMesh()
    {
        if (!m_hasSetCollider)
        {
            float sqrDstFromViewerToEdge = m_bounds.SqrDistance(GetViewerPosition);

            if (sqrDstFromViewerToEdge < m_detailLevels[m_colliderLODIndex].SqrVisibleDstThreshold)
            {
                if (!m_lodMeshes[m_colliderLODIndex].m_isRequestedMesh)
                {
                    m_lodMeshes[m_colliderLODIndex].RequestMesh(m_terrainHeightMap, m_meshSettings);
                }
            }

            if (sqrDstFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
            {
                if (m_lodMeshes[m_colliderLODIndex].m_isRecievedMesh)
                {
                    m_meshCollider.sharedMesh = m_lodMeshes[m_colliderLODIndex].m_mesh;
                    m_hasSetCollider = true;
                }
            }
        }
    }

    public void SetVisible(bool i_isVisible)
    {
        m_meshObject.SetActive(i_isVisible);
    }

    public bool IsVisible()
    {
        return m_meshObject.activeInHierarchy;
    }

}

public class LODMesh
{
    public Mesh m_mesh;
    public bool m_isRequestedMesh;
    public bool m_isRecievedMesh;
    private int m_lod;

    public event System.Action updateCallback;

    public LODMesh(int i_lod)
    {
        this.m_lod = i_lod;
    }

    private void OnMeshDataReceived(object meshDataObject)
    {
        m_mesh = ((MeshData)meshDataObject).CreateMesh();
        m_isRecievedMesh = true;

        updateCallback();
    }

    public void RequestMesh(HeightMap i_heightMap, MeshSettings i_meshSettings)
    {
        m_isRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(i_heightMap.m_values, i_meshSettings, m_lod), OnMeshDataReceived);
    }
}