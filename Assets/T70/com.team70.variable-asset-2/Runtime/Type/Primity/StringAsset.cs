using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace nano.va2 
{
    [CreateAssetMenu(fileName = "new.string.asset", menuName = "VAsset2/Primity/String", order = 110)]
    public class StringAsset : VAssetT<string>, IPrimityT, ISupportJson
    {
        public bool notNull;
        public bool notEmpty;
        
        override protected bool IsEqualT(string v1, string v2)
        {
            return v1 == v2;
        }
        
        public string ToJson() { return _value; }
        public bool FromJson(string json)
        {
            if (notNull && json == null) return false;
            if (notEmpty && json == string.Empty) return false;

            _value = json;
            FinishEditValueDirectly();
            Editor_MarkAsDirty();

            return true;
        }
    }
    
    [Serializable]
    public class VString
    {
        public bool useAsset;
        public StringAsset _asset;
        public string _target;

        public string target
        {
            get { return useAsset ? _asset.Value : _target;}
        }
    }
}