using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace nano.vs2
{
    [Serializable]
    public class PropertyItem
    {
        public string jsonValue;
        public Object referenceValue;
    }
    [System.Serializable]
    public enum OverrideType
    {
        None,
        Curve,
        PercentOnLifeTime,
        MinMax
    }
    [System.Serializable]
    public class AnimOverrideData
    {
        public float PercentOnLifeTime;
        public AnimationCurve curve;

        public OverrideType type = OverrideType.MinMax;
        public float min = 0, max = 1f;

    }
    [System.Serializable]
    public class VSPropertyBase
    {
		#if UNITY_EDITOR
        public int ID;
        #endif
		public int objectIndex;
        public Object target;
        public List<PropertyItem> stateValues = new List<PropertyItem>();

		public bool Validate()
		{
			if (stateValues.Count < 2) return true;

			for (int i = 0;i < stateValues.Count-1; i ++)
			{
				var s0 = stateValues[i];
				var s1 = stateValues[i+1];
				
				if (s0.jsonValue != s1.jsonValue) return true;
				if (s0.referenceValue != s1.referenceValue) return true;
			}

			return false;
		}
		
        public AnimOverrideData animOverrideData = new AnimOverrideData
        {
            type = OverrideType.MinMax
        };
        internal VSGetSet _field;
        private List<AmountData> _amounts;
        public List<AmountData> amounts
        {
            get
            {
                return _amounts;
            }
            set
            {
                _amounts = value;
            }
        }
        const int nStates = 2;
        public void InitStateAmountIfNeeded(int nStates)
        {
            if (_amounts == null || _amounts.Count != nStates)
            {
                _amounts = new List<AmountData>(nStates);
                for (int i = 0; i < nStates; i++) _amounts.Add(new AmountData
                {
                    amount = i == 0 ? 1 : 0
                }
                );
            }
        }
        internal VSGetSet field
        {
            get
            {
                if (_field != null) return _field;
                _field = VSGetSet.Create(target, property);
                if (_field == null)
                {
                    // Debug.LogWarning("Field is null ! " + target + ":" + property);    
                }
                return _field;
            }
            set
            {
                _field = value;
            }
        }
        public Type getFieldType()
        {
            return field == null ? null : field.dataType;
        }
        public PropertyItem defaultValue;
        public bool isIgnore = false;
		public bool isReference;
        public bool isStructValue;

        public string property;

        private bool dirty;

        [NonSerialized] List<object> _states = new List<object>();
        public List<object> states {
            get{
                if(dirty || (_states.Count != stateValues.Count))
                {
                    ReInitState();
                }
                return _states;
            }
        }

		public virtual bool SetState(int state)
		{
			if (field == null)
            {
               // #if UNITY_EDITOR
               // Debug.LogWarning("[Editor] Property not found (might only exist in Editor) : " + property);
               // #endif
                return false;
            }

			var result = false;

			if (isReference)
			{
				SetRef(state);
				result = true;
			}
			else
			{
				result = SetValue(state);
			}

			if (target is IVSListener) (target as IVSListener).OnVSChange();
			VisualState_Unity.CheckUpdateDirtyState(target);
			var targetType = target.GetType();
			
			//Debug.Log(targetType + " --> " + targetType.FullName);

			if (targetType.FullName == "PrefabModule")
			{
				var method = VSGetSet.ReflectionCache.GetMethod(targetType, "RefreshLocalize");
				method.Invoke(target, new object[0]);
			}

			return result;
        }
        public virtual void SetValueAmount(List<AmountData> amounts,  int fromState, int toState)
        {
//#if UNITY_EDITOR
//            Debug.Log("====");
//            foreach(var item in amounts)
//            {
//                Debug.Log(item.amount);
//            }
//#endif

            if (target == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[Editor] Target is null! \nVisualState: " + this);
                #endif
                return;
            }
            
            
            if (field == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("[Editor] Field is null! \nVisualState: " + this);
#endif
                return;
            }

            if (animOverrideData!= null)
            {
                if(animOverrideData.type == OverrideType.PercentOnLifeTime)
                {
                    var val = amounts[toState].amount >= animOverrideData.PercentOnLifeTime ? states[toState] :states[fromState];
                    if (!SetValue(val))
                    {
                        Debug.LogWarning("Error on: " + target);
                    }
                    return;
                }

                if(animOverrideData.type == OverrideType.Curve)
                {
                    //curve = animOverrideData.curve;
                }
            }
            
            if(isReference)
            {
                var index = 0;
                for(int i = 1; i < amounts.Count; i++)
                {
                    if(getAmountWithCurve(amounts[index].amount) < getAmountWithCurve(amounts[i].amount))
                    {
                        index = i;
                    }
                }

                if (!SetValue(states[index]))
                {
                    Debug.LogWarning("Error on: " + target);
                }
                return;
            }

            object result = null;
            var amount = 0.0f;
            for (int k = 0; k < amounts.Count; k++) amount += amounts[k].amount;
            for (int k = 0; k < amounts.Count; k++)
            {
                var animAmount = getAmountWithCurve(amounts[k].amount);
                if (k == 0)
                {
                    result = GetStateValuePercent(k, animAmount);
                    continue;
                }
                object val2 = GetStateValuePercent(k, animAmount);
                VisualState_Unity.Add2Value(result, val2, ref result, amount);
            }
            
            if (!SetValue(result))
            {
                Debug.LogWarning("Error on: " + target);
            }
        }
        private float getAmountWithCurve(float amount)
        {
            return amount;
        }
        public object cacheCurValue;
        public virtual bool SetValue(object value)
        {
            cacheCurValue = value;
            if (field == null)
            {
                return false;
            }

            if (!_field.SetValue(value))
            {
                Debug.LogWarning("SetValue error on target: " + target + " : " + property);
                return false;
            }
            
            VisualState_Unity.CheckUpdateDirtyState(target);
            return true;
        }
        public virtual object GetStateValuePercent(int state, float percent)
        {
            object val = null;
            VisualState_Unity.GetPercentValue(GetStateValue(state), percent, out val);
            return val;
        }
        public virtual object GetStateValue(int state)
        {
            if (state < 0 || state > states.Count - 1) return null;
            return states[state];
        }
        bool SetValue(int state)
        {
            if (state < 0 || state > states.Count - 1) return false;
            return SetValue(states[state]);
        }
        // --------------- REFERENCE SUPPORT -----------------
        void SetRef(int state)
        {
            var currentRef = defaultValue.referenceValue;
            {
                if (state >= 0 && state < stateValues.Count)
                {
                    currentRef = stateValues[state].referenceValue;
                }
            }

            if (currentRef != null)
            {
                SetValue(currentRef);
            }
            else
            {
                SetValue(null);
            }
        }

        private void SetDirty()
        {
            dirty = true;
            
        }
        private void ReInitState()
        {
            _states.Clear();
            if (stateValues == null)
            {
                Debug.LogWarning("StateValues is null!");
                return;
            }

            if (field == null)
            {
                Debug.LogWarning("Field is null! ");
                return;
            }
            
            for (var i = 0; i < stateValues.Count; i++)
            {
                _states.Add(
                    isReference ? stateValues[i].referenceValue
                                : JsonWrapper.FromJson(stateValues[i].jsonValue, _field.dataType, !isStructValue)
                );
            }
        }
        
        public virtual void Resize(int size)
        {
            stateValues = VSUtils.Resize(stateValues, size, defaultValue);
        }
        
        public virtual void AddState(int stateClone)
        {
            if (isReference)
            {
                Object currentRef = defaultValue.referenceValue;
                if (stateClone >= 0 && stateClone < stateValues.Count)
                {
                    currentRef = stateValues[stateClone].referenceValue;
                }
                stateValues.Add(new PropertyItem() { referenceValue = currentRef });
            }
            else
            {
                string currentValue = defaultValue.jsonValue;
                if (stateClone >= 0 && stateClone < stateValues.Count)
                {
                    currentValue = stateValues[stateClone].jsonValue;
                }
                stateValues.Add(new PropertyItem() { jsonValue = currentValue });
            }
        }
        
        public virtual void RemoveState(int index)
        {
            stateValues.RemoveAt(index);
        }

#region Editor
#if UNITY_EDITOR
        public object Clone()
        {
            return this.MemberwiseClone();
        }
		public virtual void WriteDefault(int nStates, PropertyModification prop, string propertyPath)
        {
            // Debug.Log(prop.objectReference + " path " + prop.propertyPath + " realPath: " + propertyPath+ "   " + prop.value + "  property save: " + property);
            // IMPORTANT NOTE : DO NOT USE prop.propertyPath (this instance might be a field of struct and not containing current property)
            if (field == null)
            {
                isReference = false;
            }
            else
            {
                // Debug.Log("field type " + field.dataType);
                isReference = typeof(Object).IsAssignableFrom(field.dataType);
            }
            defaultValue = new PropertyItem();

            if (isReference)
            {
                defaultValue.referenceValue = prop.objectReference;
            }
            else
            {
                defaultValue.jsonValue = prop.value;
                
            }

            // Debug.Log("defaultValue: " + defaultValue.jsonValue);



            Resize(nStates);
            SetDirty();
        }
        

        private void WriteValue(ref PropertyItem item, PropertyModification prop)
        {
            // var ser = GetProperty();
            // item.jsonValue = getStructJson(ser);
            if (isReference)
            {
                item.referenceValue = prop.objectReference;
            }
            else
            {
                item.jsonValue = prop.value;
            }
            
        }

        private string getStructJson(SerializedProperty prop)
        {
            object obj = null; 
            if(VisualState_Unity.TryGetPropertyValue(prop, out obj))
            {
                return JsonWrapper.ToJson(obj, !isStructValue);
            }
            else
            {
                //Debug.Log("count: " + prop.CountInProperty());
                var res = JsonWrapper.GetFullJson(prop);
                Debug.Log("full json: " + res);
                return res;
            }
        }


        public virtual void WriteChange(int index, PropertyModification prop)
        {
            if (field == null) field = VSGetSet.Create(prop.target, prop.propertyPath);

            var n = stateValues.Count;
            if (n <= index)
            {
                Debug.LogWarning("Something might be wrong : setting index " + index + " of " + n);
                Resize(index + 1);
            }

            // if (isLocked || isIgnored)
            // {
            //     Debug.LogWarning("Locked, do not change value! " + prop);
            //     return;
            // }
            var p = stateValues[index];
            WriteValue(ref p, prop);
            SetDirty();
        }

        //helper
        protected SerializedProperty GetProperty()
        {
            return new SerializedObject(target).FindProperty(property);
        }
#endif
#endregion
    }


    public class VSUtils
    {
        static public List<T> Resize<T>(List<T> list, int size, T defaultValue)
        {
            if (list == null) list = new List<T>();
            while (list.Count < size)
            {
                list.Add(defaultValue);
            }

            if (list.Count > size) list.RemoveRange(size, list.Count - size);
            return list;
        }

        static public List<PropertyItem> Resize(List<PropertyItem> list, int size, PropertyItem defaultValue)
        {
            if (list == null) list = new List<PropertyItem>();
            while (list.Count < size)
            {
                var n = new PropertyItem();
                n.jsonValue = defaultValue.jsonValue;
                n.referenceValue = defaultValue.referenceValue;
                list.Add(n);
            }

            if (list.Count > size) list.RemoveRange(size, list.Count - size);
            return list;
        }

        static public void ResizeProperty<T>(List<T> list, int size) where T : VSPropertyBase
        {
            for (var i = 0; i < list.Count; i++)
            {
                list[i].Resize(size);
            }
        }

        public static bool SetState<T>(List<T> list, int state) where T : VSPropertyBase
        {

            var isOK = true;
                for (var i = 0; i < list.Count; i++)
                {
                    var p = list[i];
                if(p.isIgnore) continue;
                    if (!p.SetState(state))
                    {
                        p.isIgnore = true; // so it will only trigger log warning once
                        isOK = false;
                    }
                }

            return isOK;
        }

        public static void AddState<T>(List<T> list, int stateClone) where T : VSPropertyBase
        {
            for (var i = 0; i < list.Count; i++)
            {
                //if (list[i].isIgnored) continue;
                list[i].AddState(stateClone);
            }
        }
        public static void RemoveState<T>(List<T> list, int index) where T : VSPropertyBase
        {
            for (var i = 0; i < list.Count; i++)
            {
                //if (list[i].isIgnored) continue;
                list[i].RemoveState(index);
            }
        }
    }
}

