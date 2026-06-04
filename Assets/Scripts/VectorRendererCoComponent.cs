using System;
using UnityEditor;
using UnityEngine;
using Vectors;
public class VectorRendererCoComponent : MonoBehaviour
{
    [NonSerialized] 
    private VectorRenderer vectors;

    private Grid grid;

    [SerializeField]
    public Vector3 vectorA = new Vector3(3, 0, 0);
    
    [SerializeField]
    public Vector3 vectorB = new Vector3(0, 3, 0);
    
    [SerializeField]
    public Vector3 vectorC = new Vector3(0, 0, 3);
    
    void OnEnable() {
        vectors = GetComponent<VectorRenderer>();
        grid = GetComponent<Grid>();
    }

    void Update()
    {
        using (vectors.Begin()) {
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
        var a = Handles.PositionHandle(ex.vectorA, Quaternion.identity);
        var b = Handles.PositionHandle(ex.vectorB, Quaternion.identity);
        var c = Handles.PositionHandle(ex.vectorC, Quaternion.identity);

        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(target, "Vector Positions");
            ex.vectorA = a;
            ex.vectorB = b;
            ex.vectorC = c;
            EditorUtility.SetDirty(target);
        }
    }
}
