#define VA2_SANITY_CHECK

using System;
using System.Collections.Generic;
using UnityEngine;

namespace nano.va2
{
    public class VAssetT<T> : VAsset
    {
        //TODO : ON/ OFF LOCK WHEN DISPATCHING EVENT TO PREVENT INFINITE LOOP?

        [SerializeField] public T _value;
        public static implicit operator T(VAssetT<T> asset) { return asset != null ? asset.Value : default(T); }


        // OVERRIDABLE APIs FOR DESCENDANTS : 
        public virtual T Value
        {
            get { return _value; }
            set { SetValue(value, null, null, true); }
        }

        protected virtual bool IsEqualT(T v1, T v2)
        {
            throw new Exception(this.name + " ---> " + typeof(T) + " --> Descendant class must override (implement) isEqualT<T, T>");
        }

        public virtual bool SetValue(T v, bool? overrideCheck4Changes = null, bool? overrideBubble = true, bool triggerChange = true) // force: skip change check
        {
            if (readOnly)
            {   
                if (IsEqualT(v, _value)) return false;

                Debug.LogWarning(this + " is [READONLY]\n Failed to change value " + _value + " --> " + v);
                return false;
            }

            var pCheck4Changes = overrideCheck4Changes ?? check4Changes;
            var pBubble = overrideBubble ?? bubbleEvents;
            if (pCheck4Changes && IsEqualT(v, _value)) return false; // set value to the same value: skip

            _value = v;
            
            // Copy changes to children assets
            if (this is IParentAsset) (this as IParentAsset)?.CopyChangesToChildren();

            if (triggerChange) TriggerChange(pBubble);
            return true;
        }


            
        // STRUCTURAL
        public override object GetRawValue() { return _value; }
        public override bool SetRawValue(object raw, bool? overrideCheck4Changes = null, bool? overrideBubble = true, bool triggerChange = true)
        { 
            return SetValue((T)raw, overrideCheck4Changes, overrideBubble, triggerChange);
        }
    }
}