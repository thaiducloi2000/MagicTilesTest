using UnityEditor;

static class GizmosScene
{
    [MenuItem("T70/Tools/Toggle Gizmos %g")]
    static void Toggle()
    {
        SceneView.lastActiveSceneView.drawGizmos = !SceneView.lastActiveSceneView.drawGizmos;
    }
}