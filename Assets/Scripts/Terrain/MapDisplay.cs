using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {

    [SerializeField]
    private Renderer m_renderer;
    [SerializeField]
    private MeshFilter m_meshFilter;
    [SerializeField]
    private MeshRenderer m_meshRenderer;
    [SerializeField]
    private MeshCollider m_meshCollider;

    /// <summary>
    /// What mode do we want to apply the procedural generation to
    /// </summary>
    public enum DrawMode
    {
        NoiseMap,
        Mesh,
        FalloffMap,
        TreeNoiseMap,
        ComparedNoiseMap
    }

    /// <summary>
    /// Current active draw mode
    /// </summary>
    public DrawMode m_drawMode;

    public MeshSettings m_meshSettings;
    public NoiseSettings m_terrainNoiseSettings;
    public TextureData m_textureSettings;
    public NoiseSettings m_treeNoiseSettings;

    public Material m_terrainMaterial;

    [SerializeField]
    [Range(0, MeshSettings.m_numberOfSupportedLODs - 1)]
    private int editorPreviewLod;
    public int GetSetLevelOfDetail
    {
        get { return editorPreviewLod; }
        set { editorPreviewLod = value; }
    }

    [SerializeField]
    private bool m_isAutoUpdateEnabled;
    public bool GetIsAutoUpdateEnabled
    {
        get { return m_isAutoUpdateEnabled; }
    }
    [SerializeField]
    private bool m_isAutoAnimateEnabled;
    public bool GetIsAutoAnimateEnabled
    {
        get { return m_isAutoAnimateEnabled; }
    }

    /// <summary>
    /// Used to "preview" a map chunk before launching the game.
    /// </summary>
    public void DrawMapInEditor()
    {
        m_textureSettings.ApplyToMaterial(m_terrainMaterial);
        m_textureSettings.UpdatedMeshHeight(m_terrainMaterial, m_terrainNoiseSettings.GetMinHeight, m_terrainNoiseSettings.GetMaxHeight);

        HeightMap l_heightMap = new HeightMap();

        if (m_drawMode == DrawMode.NoiseMap || m_drawMode == DrawMode.Mesh || m_drawMode == DrawMode.FalloffMap)
        {
            l_heightMap = HeightMapGenerator.GenerateHeightMap(m_meshSettings.GetNumberOfVertsPerLine, m_meshSettings.GetNumberOfVertsPerLine, m_terrainNoiseSettings, Vector2.zero);
        }
        else if (m_drawMode == DrawMode.TreeNoiseMap)
        {
            l_heightMap = HeightMapGenerator.GenerateHeightMap(m_meshSettings.GetNumberOfVertsPerLine, m_meshSettings.GetNumberOfVertsPerLine, m_treeNoiseSettings, Vector2.zero);
        }
        else if (m_drawMode == DrawMode.ComparedNoiseMap)
        {
            l_heightMap = HeightMapGenerator.GenerateHeightMapByComparison(m_meshSettings.GetNumberOfVertsPerLine, m_meshSettings.GetNumberOfVertsPerLine, m_terrainNoiseSettings, m_treeNoiseSettings, Vector2.zero);
        }

        if (m_drawMode == DrawMode.Mesh)
        {
            if (m_terrainNoiseSettings.m_autoAnimate && GetIsAutoAnimateEnabled)
            {
                if (m_terrainNoiseSettings.m_xAxisAnimate)
                {
                    m_terrainNoiseSettings.m_offset.x += m_terrainNoiseSettings.m_animationSpeed * Time.deltaTime;
                    m_terrainNoiseSettings.m_offset.y += m_terrainNoiseSettings.m_animationSpeed * Time.deltaTime;

                }
                else
                {
                    m_terrainNoiseSettings.m_offset.x += m_terrainNoiseSettings.m_animationSpeed * Time.deltaTime;

                    m_terrainNoiseSettings.m_offset.y += m_terrainNoiseSettings.m_animationSpeed * Time.deltaTime;
                }
            }

            DrawMesh(MeshGenerator.GenerateTerrainMesh(l_heightMap.m_values, m_meshSettings, GetSetLevelOfDetail));
        }
        else if (m_drawMode == DrawMode.FalloffMap)
        {
            DrawTexture(TerrainTextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(m_meshSettings.GetNumberOfVertsPerLine, m_terrainNoiseSettings.m_falloffSharpness, m_terrainNoiseSettings.m_falloffRange), 0, 1)));
        }
        else
        {
            DrawTexture(TerrainTextureGenerator.TextureFromHeightMap(l_heightMap));
        }
    }

    /// <summary>
    /// Applies texture to the renderer,
    /// </summary>
    /// <param name="i_texture">Given texture</param>
    public void DrawTexture(Texture2D i_texture)
    {
        m_renderer.sharedMaterial.mainTexture = i_texture;
        m_renderer.transform.localScale = new Vector3(-i_texture.width, 1, i_texture.height) / 2.5f;

        m_renderer.gameObject.SetActive(true);
        m_meshFilter.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Draws the mesh onto the mesh filter and renderer.
    /// </summary>
    /// <param name="i_meshData">Mesh data</param>
    /// <param name="i_texture">Texture data</param>
    public void DrawMesh(MeshData i_meshData)
    {
        
        if (m_meshFilter != null)
        {
            m_meshFilter.sharedMesh = i_meshData.CreateMesh();
            m_meshCollider.sharedMesh = m_meshFilter.sharedMesh;
        }

        if (m_meshRenderer != null)
        {
            m_renderer.gameObject.SetActive(false);
        }

        if (m_meshFilter != null)
        {
            m_meshFilter.gameObject.SetActive(true);
        }
    }

    private void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    private void OnTextureValuesUpdates()
    {
        m_textureSettings.ApplyToMaterial(m_terrainMaterial);
    }

    /// <summary>
    /// Used by the inspector, this prevents the designer from crashing Unity or the map generator.
    /// </summary>
    private void OnValidate()
    {
        if (m_meshSettings != null)
        {
            m_meshSettings.OnValuesUpdated -= OnValuesUpdated;
            m_meshSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if (m_terrainNoiseSettings != null)
        {
            m_terrainNoiseSettings.OnValuesUpdated -= OnValuesUpdated;
            m_terrainNoiseSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if (m_textureSettings != null)
        {
            m_textureSettings.OnValuesUpdated -= OnTextureValuesUpdates;
            m_textureSettings.OnValuesUpdated += OnTextureValuesUpdates;
        }

        if (m_treeNoiseSettings != null)
        {
            m_treeNoiseSettings.OnValuesUpdated -= OnValuesUpdated;
            m_treeNoiseSettings.OnValuesUpdated += OnValuesUpdated;
        }
    }
}
