using UnityEngine;
using UnityEditor;

public class CopyPasteTransformComponent
{
    struct RectTransformData
    {
        public Vector3 anchorPosition;
        public Vector2 anchorMax;
        public Vector2 anchorMin;
        public Vector2 pivot;
        public Quaternion localRotation;
        public Vector3 localScale;
        public Vector2 sizeDelta;

        public RectTransformData(Vector2 anchorMax, Vector2 anchorMin, Vector2 pivot, Vector2 sizeDelta, Vector3 anchorPosition, Quaternion localRotation, Vector3 localScale)
        {
            this.pivot = pivot;
            this.anchorMax = anchorMax;
            this.anchorMin = anchorMin;
            this.anchorPosition = anchorPosition;
            this.sizeDelta = sizeDelta;
            this.localRotation = localRotation;
            this.localScale = localScale;
        }
    }

    public struct TransformData
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;

        public TransformData(Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            this.localPosition = localPosition;
            this.localRotation = localRotation;
            this.localScale = localScale;
        }
    }

    private static RectTransformData _dataRectTransform;
    private static TransformData _data;
    private static Vector3? _dataCenter;

    [MenuItem("T70/Tools/Transform/Copy Transform Values &c", false, -101)]
    public static void CopyTransformValues()
    {
        if (Selection.gameObjects.Length == 0) return;
        var selectionTr = Selection.gameObjects[0].transform;
        var selectionRectTransform = selectionTr.GetComponent<RectTransform>();
        if (selectionRectTransform == null)
        {
            _data = new TransformData(selectionTr.position, selectionTr.rotation, selectionTr.localScale);
            return;
        }
        _dataRectTransform = new RectTransformData(selectionRectTransform.anchorMax, selectionRectTransform.anchorMin, selectionRectTransform.pivot, 
                    selectionRectTransform.sizeDelta, selectionRectTransform.anchoredPosition, selectionRectTransform.localRotation, 
                    selectionRectTransform.localScale);
    }

    [MenuItem("T70/Tools/Transform/Paste Transform Values &v", false, -101)]
    public static void PasteTransformValues()
    {
        foreach (var selection in Selection.gameObjects)
        {
            Transform selectionTr = selection.transform;
            RectTransform selectRectTransform = selectionTr.GetComponent<RectTransform>();
            if (selectRectTransform == null)
            {
                Undo.RecordObject(selectionTr, "Paste Transform Values");
                selectionTr.position = _data.localPosition;
                selectionTr.rotation = _data.localRotation;
                selectionTr.localScale = _data.localScale;
                return;
            }
            Undo.RecordObject(selectionTr, "Paste RectTransform Values");
            selectRectTransform.anchorMax = _dataRectTransform.anchorMax;
            selectRectTransform.anchorMin = _dataRectTransform.anchorMin;
            selectRectTransform.pivot = _dataRectTransform.pivot;
            selectRectTransform.sizeDelta = _dataRectTransform.sizeDelta;
            selectRectTransform.anchoredPosition = _dataRectTransform.anchorPosition;
            selectRectTransform.localRotation = _dataRectTransform.localRotation;
            selectRectTransform.localScale = _dataRectTransform.localScale;
        }
    }

    [MenuItem("T70/Tools/Transform/Clear Transform Values #&c", false, -101)]
    public static void ClearTransformValues()
    {
        foreach (var selection in Selection.gameObjects)
        {
            Transform selectionTr = selection.transform;
            var lastPosition = selectionTr.position;
            RectTransform selectRectTransform = selectionTr.GetComponent<RectTransform>();
            if (selectRectTransform == null)
            {
                Undo.RecordObject(selectionTr, "Paste Transform Values");
                selectionTr.localPosition = Vector3.zero;
                selectionTr.localRotation = Quaternion.identity;
                selectionTr.localScale = Vector3.one;
                return;
            }
            Undo.RecordObject(selectionTr, "Paste RectTransform Values");
            var currentRealSize = selectRectTransform.rect.size;
            selectRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            selectRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            // selectRectTransform.pivot = new Vector2(0.5f, 0.5f);
            // selectRectTransform.anchoredPosition = Vector2.zero;
            // selectRectTransform.localRotation = Quaternion.identity;
            // selectRectTransform.localScale = Vector3.one;
            selectRectTransform.position = lastPosition;
            selectRectTransform.sizeDelta = currentRealSize;
        }
    }

    [MenuItem("T70/Tools/Transform/Copy Center Position %&c", false, -101)]
    public static void CopyCenterPosition()
    {
        if (Selection.gameObjects.Length == 0) return;
        var render = Selection.gameObjects[0].GetComponent<Renderer>();
        if (render == null) return;
        _dataCenter = render.bounds.center;
    }

    [MenuItem("T70/Tools/Transform/Paste Center Position %&v", false, -101)]
    public static void PasteCenterPosition()
    {
        if (_dataCenter == null) return;
        foreach (var selection in Selection.gameObjects)
        {
            Undo.RecordObject(selection.transform, "Paste Center Position");
            selection.transform.position = _dataCenter.Value;
        }
    }
}