// #define VA2_DISABLE_EDITOR
// #define VA2_DISABLE_WARNINGS
#define VA2_KEEP_OBSOLETE_METHOD


// BRAIN HACKS:
// We need to have VA2_DISABLE_WARNING off by default --> NO CUSTOM COMPILING DIRECTIVES = VA2_ENABLE_WARNINGS
// The same for VA2_DISABLE_EDITOR should be off by default

#if !VA2_DISABLE_WARNINGS
    #define VA2_ENABLE_WARNINGS
#endif

#if !VA2_DISABLE_EDITOR
    #define VA2_ENABLE_EDITOR
#endif



using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Debug = UnityEngine.Debug;
using UnityObject = UnityEngine.Object;


// VAssetType
// - RUNTIME (allow change but don't save after quit / must init before use)
// - CONFIG (allow change at runtime & save)
// - PERSISTENT (read / write from local)
// 

namespace nano.va2
{
    public enum VAssetType
    {
        Default,
        Runtime,
        Persistent
    }

    public abstract partial class VAsset : ScriptableObject
    {
        public bool readOnly;
        public bool debugChange;
        public bool check4Changes = true;
        public bool bubbleEvents = true;

        //public VAssetType type;
        
        
        // ------------------------ SUPPORT FOR READ / WRITE FROM GLOBAL CONFIG
        [NonSerialized] private string _variableName;
        public string variableName { get {
            if (!string.IsNullOrEmpty(_variableName)) return _variableName;
            _variableName = this.name.ToLowerInvariant().Replace('.', '_');
            return _variableName;
        }}

        
        // ------------------------ SUPPORT FOR NESTED ASSETS
        public VAsset parent;
        public string idFromParent; // id set from parent for faster lookup
        
        // ------------------------ SUPPORT FOR EVENT
        public VEvent onChange;
        public void FinishEditValueDirectly() 
        {
            SetRawValue(rawValue, false, true);
        }

        public virtual void TriggerChange(bool bubble = true)
        {
            if (onChange != null) onChange.Trigger();

            if (!bubble) return; // will not bubble changes to parent
            if (parent == null) return; // no parent attached

            // #if VA2_SANITY_CHECK
            // if (Application.isPlaying && !(parent is IParentAsset))
            // {
            //     LogWarning("Invalid parent asset: must be IParentAsset");
            //     parent = null; // remove invalid parent (only at runtime to prevent destructible changes)
            //     return;
            // }
            // #endif
            
            (parent as IParentAsset).OnChildAssetChange(this);
        }
        
        public object rawValue
        {
            get { return GetRawValue(); }
            set { SetRawValue(value, null, null); }
        }
        
        // ------------------------ FOR GENERIC USE 

        public T GetValueT<T>() { return (T)GetRawValue(); }

        public abstract object GetRawValue();
        public abstract bool SetRawValue(object raw, bool? overrideCheck4Changes = null, bool? overrideBubble = true, bool triggerChange = true);
        

        #if VA2_KEEP_OBSOLETE_METHOD // Backward APIs compatible

        [Obsolete("SetValueRaw() is deprecated, please use SetRawValue() instead.")]
        public virtual void SetValueRaw(object raw, bool check4Changes, bool bubble) { SetRawValue(raw, check4Changes, bubble); }
        
		[Obsolete("GetValueRaw() is deprecated, please use GetRawValue() instead.")]
        public virtual object GetValueRaw() { return GetRawValue(); }
        #endif
        

        // ----------------------- FOR DEVELOPMENT PURPOSE
        [Conditional("VA2_ENABLE_WARNINGS")] public void LogWarning(string message) { Debug.LogWarning(message); }

        public void DebugChange(string format, params object[] args)
        {
            if (debugChange) UnityEngine.Debug.Log(string.Format(format, args));
            Editor_PauseOnChange();
        }
        
        // ----------------------- ESSENTIAL / EDITOR-ONLY THINGS

        #if !UNITY_EDITOR
        [Conditional("UNITY_EDITOR")] public void Editor_PauseOnChange(){}
        [Conditional("UNITY_EDITOR")] public void Editor_MarkAsDirty(){}
        #endif
    }
    


    public partial class VAsset
    {
        public string subName;
        public bool drawDefault;
        [NonSerialized] public bool allowDelete;
        public bool pauseOnChange;

        
		#if UNITY_EDITOR
        public void Editor_PauseOnChange()
        {
            if (Application.isPlaying) return;
            if (pauseOnChange) Debug.Break();
        }

        public void Editor_MarkAsDirty()
        {
            if (Application.isPlaying) return;
        }
        
        [ContextMenu("Allow Delete")] public void DoAllowDelete() { allowDelete = !allowDelete; }
        [ContextMenu("Default Inspector")] public void DoDrawDefault() { drawDefault = !drawDefault; }
        public string GetChildEventName(string eventName)
        {
            return (string.IsNullOrEmpty(subName) ? this.name : subName) + "_" + eventName;
        }

        public virtual void OnDestroy()
        {
            if (onChange != null) // Delete children, if any
            {
                DestroyImmediate(onChange, true);
                onChange = null;
            }

            if (parent != null && parent is IParentAsset) 
            {
                var p = (parent as IParentAsset);
                p.DestroyChild(this);
            }
        }
		#endif
    }
}