using System.Text;
using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace nano.va2
{
    public class VListAssetT<T> : VAssetT<List<T>>, IListAssetT
    {
		public int defaultListSize = 128;

        [Serializable] internal class Wrapper
        {
            public List<T> data;
        }

        protected override bool IsEqualT(List<T> v1, List<T> v2)
        {
            return v1 == v2;
        }

        override public object GetRawValue()
        {
            if (_value == null) _value = new List<T>(defaultListSize);
            return Value;
        }
        
        public T this[int index]
        {
            get { return Value[index]; }
            set { Value[index] = value; }
        }

        public int Count { get { return Value.Count; } }

        public static implicit operator List<T>(VListAssetT<T> asset)
        {
            return asset != null ? asset.Value : null;
        }

        public virtual string ToJson()
        {
            var wrapper = new Wrapper() { data = _value };
            var json    = JsonUtility.ToJson(wrapper);
            var stIdx = json.IndexOf('[', 6);
            var edIdx = json.LastIndexOf(']');
            return json.Substring(stIdx, edIdx+1-stIdx);
        }

        public virtual bool FromJson(string data)
        {
            var json = string.Format("{0}\"data\":{1}{2}", "{", data, "}");
			Wrapper w;
			try
            {
                w = JsonUtility.FromJson<Wrapper>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Invalid Json Array format for: " + variableName + "\n" + e + "\n\n" + json);
                return false;
            }
            
            _value = w.data;
            FinishEditValueDirectly();
            Editor_MarkAsDirty();
            return true;
        }
    }

	public class VListAssetJson<T> : VListAssetT<T>, ISupportJson
        where T: class, ISupportJson, new()
    {
		
	}

    public class VListAssetTSV<T> : VListAssetT<T>, ISupportTSV
        where T: class, ISupportTSV, new()
    {
        public virtual bool FromTSV(string tsv)
        {
			const char CR = '\r';
			const char LF = '\n';
			const char TAB	= '\t';
			
			var list = new List<T>(defaultListSize);
			
			// line indexes
			var stIdx  		= 0;
			var isEOL  		= false;
			var failCount 	= 0;
			var length 		= tsv.Length;
			var nSkip		= 1;
			
			for (int i = 0; i < length; i++)
			{
				var c 		= tsv[i];
				var isCRLF	= (c == CR) || (c == LF);
				var isEOF 	= i == length-1;

				if (isCRLF || isEOF)
				{
					if (isEOL) continue; //already EOL : continue to ignore multiple \n \r
					isEOL = true;

					// line found!
					if (nSkip > 0)
					{
						nSkip--;
						continue;
					}

					var edIdx = isEOF ? length : i;

					if (tsv[stIdx] == TAB)
					{
						break;
					}

					T info = new T();

					if (info is ISupportTSV2)
					{	
						if ((info as ISupportTSV2).FromTSV(tsv, stIdx, edIdx))
						{
							
						}
						
						list.Add(info);
						continue;
					}
					else
					{
						var line = tsv.Substring(stIdx, edIdx-stIdx); // exclude the last CR | LF
						if (info.FromTSV(line))
						{
							list.Add(info);
							continue;
						}
					}
					
					failCount++;
					continue;
				}

				if (isEOL)
				{
					isEOL = false;
					stIdx = i;
				}
			}

#if UNITY_EDITOR
            if (failCount > 0)
            {
                Debug.LogWarning($"[Editor] {failCount} lines parse failed!");
            }
#endif

            Value = list;
            return true;
        }
        
        public virtual string ToTSV()
        {
            var sb = new StringBuilder();
            var list = Value;

            for (int i = 0; i < list.Count; i++)
            {
                sb.AppendLine(list[i].ToTSV());
            }
            return sb.ToString();
        }
    }
}