using System;
using UnityEditor;
using UnityEngine;
using Vectors;
public class VectorRendererCoComponent : MonoBehaviour
{
    [NonSerialized] 
    private VectorRenderer vectors;

    private Grid grid;
    void OnEnable() {
        vectors = GetComponent<VectorRenderer>();
        grid = GetComponent<Grid>();
    }

    void Update()
    {
        using (vectors.Begin()) {
            //Draw a vector between each node starting in the startNode and ending in the endNode.
            if (grid.path != null)
            {
                for (int i = 1; i < grid.path.Count; i++)
                {
                    vectors.Draw(
                        new Vector3(grid.path[i - 1].worldPosition.x,
                            grid.path[i - 1].worldPosition.y + 1,
                            grid.path[i - 1].worldPosition.z),
                        new Vector3(grid.path[i].worldPosition.x,
                            grid.path[i].worldPosition.y + 1,
                            grid.path[i].worldPosition.z),
                        Color.white);
                }
            }
        }
    }
}

[CustomEditor(typeof(VectorRendererCoComponent))]
public class ExampleGUI : Editor {
    void OnSceneGUI() {
        var ex = target as VectorRendererCoComponent;
        if (ex == null) return;

        EditorGUI.BeginChangeCheck();

        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(target, "Vector Positions");
            EditorUtility.SetDirty(target);
        }
    }
}
