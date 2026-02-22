#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// simple editor helper that purges any leftover "chunk" objects when the editor
// loads or exits play mode so they don't stick around in the scene permanently.
[InitializeOnLoad]
public static class ChunkCleanup
{
    static ChunkCleanup()
    {
        // schedule removal when editor is ready
        EditorApplication.delayCall += RemoveChunks;
        // also remove when returning from play mode
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
            RemoveChunks();
    }

    private static void RemoveChunks()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        GameObject[] all = Object.FindObjectsOfType<GameObject>();
        foreach (var go in all)
        {
            if (go.name == "chunk")
                Object.DestroyImmediate(go);
        }
    }
}
#endif