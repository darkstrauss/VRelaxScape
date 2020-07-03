using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(MapDisplay))]
public class MapGeneratorInEditor : Editor {

	public override void OnInspectorGUI()
    {
        MapDisplay l_mapGenerator = (MapDisplay)target;

        if (DrawDefaultInspector())
        {
            if (l_mapGenerator.GetIsAutoUpdateEnabled)
            {
                l_mapGenerator.DrawMapInEditor();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            l_mapGenerator.DrawMapInEditor();
        }

    }
}
