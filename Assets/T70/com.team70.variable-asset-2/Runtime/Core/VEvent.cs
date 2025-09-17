using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;


namespace nano.va2
{
    [CreateAssetMenu(fileName = "new.event.asset", menuName = "VAsset2/Primity/Event", order = 100)]
    public class VEvent : ScriptableObject
    {
        // Support for UnityEvent (persistent)
        public UnityEvent evt;
        public List<UnityAction> listRemove = new List<UnityAction>();
        
        public void AddListener(UnityAction ac, bool once = false)
        {
            evt.RemoveListener(ac);
            evt.AddListener(ac);
            if (once) listRemove.Add(ac);
        }

        public void RemoveListener(UnityAction ac)
        {
            evt.RemoveListener(ac);
        }

        [ContextMenu("Trigger")]
        public virtual void Trigger()
        {
            Editor_DebugChange("Trigger: " + this.name);
            if (evt != null) evt.Invoke();

            for (var i = listRemove.Count - 1; i >= 0; i--)
            {
                evt.RemoveListener(listRemove[i]);
            }
            listRemove.Clear();
        }
        
        
#if UNITY_EDITOR
        public bool debugChange;
#endif

        [Conditional("UNITY_EDITOR")]
        public void Editor_DebugChange(string format, params object[] args)
        {
            #if UNITY_EDITOR
            if (!debugChange) return;
            UnityEngine.Debug.Log(string.Format(format, args));
            #endif
        }
    }
    
    public class VEventT<T> : VEvent
    {
        public T data;

        public void SetData(T newData, bool triggerEvent = true)
        {
            data = newData;
            if (triggerEvent) Trigger();
        }
    }
    
    public class VEnumEventT<T> : VEventT<T>
    {
        [NonSerialized] public readonly Dictionary<T, Action> handlers = new Dictionary<T, Action>();
	    
        public override void Trigger()
        {
            base.Trigger();
		
            // Support for Action-based callbacks
            if (handlers.TryGetValue(data, out var h))
            {
                h?.Invoke();
            }
        }
	
        public void AddListener(T gs, Action callback)
        {
            if (handlers.TryGetValue(gs, out var cb))
            {
                cb -= callback;
                cb += callback;
                handlers[gs] = cb;
            }
            else
            {
                handlers[gs] = callback;
            }
        }
	
        public void RemoveListener(T gs, Action callback)
        {
            if (!handlers.TryGetValue(gs, out var cb)) return;
		
            cb -= callback;
            handlers[gs] = cb;
        }
        
        public void AddListener(T gs1, Action cb1, T gs2, Action cb2)
        {
            AddListener(gs1, cb1);
            AddListener(gs2, cb2);
        }
	
        public void AddListener(T gs1, Action cb1, T gs2, Action cb2, T gs3, Action cb3)
        {
            AddListener(gs1, cb1);
            AddListener(gs2, cb2);
            AddListener(gs3, cb3);
        }
	
        public void AddListener(T gs1, Action cb1, T gs2, Action cb2, T gs3, Action cb3, T gs4, Action cb4)
        {
            AddListener(gs1, cb1);
            AddListener(gs2, cb2);
            AddListener(gs3, cb3);
            AddListener(gs4, cb4);
        }
	
        public void AddListener(T gs1, Action cb1, T gs2, Action cb2, T gs3, Action cb3, T gs4, Action cb4, T gs5, Action cb5)
        {
            AddListener(gs1, cb1);
            AddListener(gs2, cb2);
            AddListener(gs3, cb3);
            AddListener(gs4, cb4);
            AddListener(gs5, cb5);
        }
	
        public void RemoveListener(T gs1, Action cb1, T gs2, Action cb2)
        {
            RemoveListener(gs1, cb1);
            RemoveListener(gs2, cb2);
        }
	
        public void RemoveListener(T gs1, Action cb1, T gs2, Action cb2, T gs3, Action cb3)
        {
            RemoveListener(gs1, cb1);
            RemoveListener(gs2, cb2);
            RemoveListener(gs3, cb3);
        }
	
        public void RemoveListener(T gs1, Action cb1, T gs2, Action cb2, T gs3, Action cb3, T gs4, Action cb4)
        {
            RemoveListener(gs1, cb1);
            RemoveListener(gs2, cb2);
            RemoveListener(gs3, cb3);
            RemoveListener(gs4, cb4);
        }
	
        public void RemoveListener(T gs1, Action cb1, T gs2, Action cb2, T gs3, Action cb3, T gs4, Action cb4, T gs5, Action cb5)
        {
            RemoveListener(gs1, cb1);
            RemoveListener(gs2, cb2);
            RemoveListener(gs3, cb3);
            RemoveListener(gs4, cb4);
            RemoveListener(gs5, cb5);
        }
    }
}