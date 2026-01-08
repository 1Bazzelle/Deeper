using UnityEditor;
using UnityEngine;



public class AddPrefabToChildren : MonoBehaviour
{
#if UNITY_EDITOR
    public GameObject prefab;

    [ContextMenu("Attach Prefab To Children")]
    public void AttachPrefab()
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is not assigned.");
            return;
        }

        Transform[] children = GetComponentsInChildren<Transform>();

        foreach (Transform child in children)
        {
            if (child == transform) continue;

            GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instantiatedPrefab.transform.SetParent(child, false); // Keep local transform
            instantiatedPrefab.transform.localPosition = Vector3.zero;
            instantiatedPrefab.transform.localRotation = Quaternion.identity;

            EditorUtility.SetDirty(instantiatedPrefab);
            Debug.Log($"Attached prefab to {child.name}");
        }

        EditorUtility.SetDirty(gameObject);
    }

    [ContextMenu("Remove Nested Children")]
    public void RemoveNestedChildren()
    {
        foreach (Transform child in transform)
        {
            for (int i = child.childCount - 1; i >= 0; i--)
            {
                Transform grandChild = child.GetChild(i);
                Debug.Log($"Removing {grandChild.name} from {child.name}");

                DestroyImmediate(grandChild.gameObject);
            }
        }

        EditorUtility.SetDirty(gameObject);
    }
#endif
}
