using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

namespace com.team70
{
    public class DragUI
    {
        public int id = -1;
        public MouseCursor cursor = MouseCursor.ResizeHorizontal;
        private Vector2 startPos;
        private Vector2 mousePos;
        private Vector2 offsetPos;
        private Rect rect;
        private float startValue;

        // public float dx { get { return mousePos.x - startPos.x; }}
        // public float dy { get { return mousePos.y - startPos.y; }}
        public float cx { get { return mousePos.x - offsetPos.x + rect.width/2f; }} // center of the dragging rect
        public float lx { get { return mousePos.x - offsetPos.x; }} // left side of the dragging rect

        public bool Check(int index, Rect r, float value, UnityObject undoTarget = null) // TODO : Save offset to compensate
        {
            var evt = Event.current;
            if (evt.type == EventType.Layout) return false;

            EditorGUIUtility.AddCursorRect(r, cursor);

            if (id == -1) // check start drag
            {
                if (evt.type != EventType.MouseDown) return false; // mouse isn't down
                if (evt.button != 0) return false; // left mouse isn't down
                if (!r.Contains(evt.mousePosition)) return false; // not in dragging rect

                id = index;
                rect = r;
                startPos = evt.mousePosition;
                mousePos = evt.mousePosition;
                offsetPos = evt.mousePosition - new Vector2(r.x, r.y);
                startValue = value;
                Event.current.Use();

                if (undoTarget != null)
                {
                    Undo.RegisterFullObjectHierarchyUndo(undoTarget, "drag");
                    EditorUtility.SetDirty(undoTarget);
                }

                return true;
            }

            if (id != index) return false; // not the currenly dragging id

            mousePos = evt.mousePosition;

            if (evt.type == EventType.MouseUp) // stop dragging
            {
                id = -1;
                Event.current.Use();
                
                if (undoTarget != null)
                {
                    EditorUtility.SetDirty(undoTarget);
                }

                return true;
            }
            
            return true;
        }
    }
}
