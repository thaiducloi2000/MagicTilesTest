#define VREF_USE_VASSET

using System;
using UnityEngine;

#if VREF_USE_VASSET
    using nano.va2;
#else
    using UnityEngine.Events;
#endif

namespace nano.va2
{
    public interface IVRef
    { 
        Type targetTypeT { get; }
        UnityEngine.Object currentRef { get; set; }
    }
    
    public interface IVRefComponent : IVRef { }
    
    public class VRefT<T> : VRefTBase<T>, IVRef where T: UnityEngine.Object
    {
        public string globalId;
        public static implicit operator T(VRefT<T> asset) { return asset != null ? (T)asset._value : default(T); }

        public UnityEngine.Object currentRef
        {
            get { return _value; }
            set { SetValue((T)value); }
        }
        
        public Type targetTypeT { get { return typeof(T); } }
    }
    
    public class VRef_ComponentT<T> : VRefT<T>, IVRefComponent where T : Component
    {
        
    }
}

#if VREF_USE_VASSET
public class VRefTBase<T> : VAssetT<T> where T: UnityEngine.Object
{
    protected override bool IsEqualT(T v1, T v2) { return v1 == v2; }
}
#else

public class VRefTBase<T> : ScriptableObject
{
    [SerializeField] public T _value;
    public UnityEvent onChange;
    
    public virtual bool SetValue(T v, bool triggerChange = true) // force: skip change check
    {
        _value = v;
        if (triggerChange && onChange!=null) onChange.Invoke();
        return true;
    }
}
#endif