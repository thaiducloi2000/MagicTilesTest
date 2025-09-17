using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace nano.va2
{
    public class EnumAssetT<T> : VAssetT<T>, IInt
    {
        protected override bool IsEqualT(T v1, T v2)
        {
            return (int)(object)v1 == (int)(object)v2;
        }
        
        public int intValue { get { return (int)(object) _value; } }
        private Dictionary<T, VEvent> enumDict = new Dictionary<T, VEvent>();
        
        protected void AttachHandler(T enumValue, VEvent evt)
        {
            if (evt != null && !enumDict.ContainsKey(enumValue))
            {
                enumDict.Add(enumValue, evt);
            }
        }
        
        protected void TriggerHandler(T enumValue)
        {   
            VEvent evt;
            if (enumDict.TryGetValue(enumValue, out evt))
            {
                evt.Trigger();
            }
        }
        
        protected void SetupListener(VEvent evt, string eventName, T enumValue, UnityAction handler, bool isAdd)
        {
            if (evt == null)
            {
                Debug.LogWarning(this + " Event <" + eventName+ "> not yet enabled! ");
                return;
            }
            
            if (isAdd)
            {
                evt.AddListener(handler);
            }
            else
            {
                evt.RemoveListener(handler);
            }
        }
        
        public override void TriggerChange(bool bubble)
        {
            base.TriggerChange(bubble);
            
            VEvent evt = null;
            if (enumDict.TryGetValue(_value, out evt))
            {
                if (evt != null) evt.Trigger();
            }
        }
    }
}