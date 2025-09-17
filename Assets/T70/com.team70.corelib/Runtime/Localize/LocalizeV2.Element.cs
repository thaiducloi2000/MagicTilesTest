using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
public partial class LocalizeV2 : ScriptableObject
{
	[Serializable]
	public partial class Element
	{
		public string locID;
		
		public string UnescapeTSVString(string import)
		{
			if (string.IsNullOrEmpty(import)) return string.Empty;

			return import
					.Replace("\\t", "\t").Replace("\\\t", "\\t")
					.Replace("\\r", "\r").Replace("\\\r", "\\r")
					.Replace("\\n", "\n").Replace("\\\n", "\\n")
				;
		}

		public void FromTSV(string value, bool allowClear, List<string> langs)
		{
			var arr = value.Split('\t');
			if (arr.Length < 2)
			{
				Debug.LogWarning("Invalid line value length : " + arr.Length + "\n" + value);
				return;
			}

			locID = arr[0];

			var n = Mathf.Min(arr.Length - 1, langs.Count);
			for (var i = 0; i < n; i++)
			{
				if (!allowClear && string.IsNullOrEmpty(arr[i + 1])) continue; // skip blank column
				SetValue(UnescapeTSVString(arr[i + 1]), i);
			}
		}


		public bool enable;
		public List<string> locValues;

		public void SetValue(string value, int langIndex)
		{
			if (locValues == null) locValues = new List<string>();
			while (locValues.Count <= langIndex) locValues.Add(string.Empty);
			locValues[langIndex] = value;
		}

		public string GetValue(int langIndex)
		{
			if (locValues == null || (locValues.Count <= langIndex)) return string.Empty;
			// while (locValues.Count <= langIndex) locValues.Add(string.Empty);
			return locValues[langIndex];
		}
		
		
		[NonSerialized] public int searchScore;
		[NonSerialized] string locGroup;

		public string GetGroup()
		{
			if (string.IsNullOrEmpty(locGroup)) locGroup = locID.Substring(0, locID.IndexOf('_'));
			return locGroup;
		}

		public string GetGroup2()
		{
			var idx1 = locID.LastIndexOf('_');
			if (idx1 == -1) return string.Empty;

			//var idx2 = locID.IndexOf('_', idx1+1);
			//if (idx2 == -1) return locID.Substring(0, idx1);

			return locID.Substring(0, idx1);
		}

		public string EscapeTSVString(string export)
		{
			if (string.IsNullOrEmpty(export)) return string.Empty;

			return export
					.Replace("\\t", "\\\\t").Replace("\t", "\\t")
					.Replace("\\r", "\\\\r").Replace("\r", "\\r")
					.Replace("\\n", "\\\\n").Replace("\n", "\\n")
				;
		}

		public string ToTSV(List<string> langs)
		{
			var sb = new StringBuilder();
			sb.Append(locID);
			sb.Append('\t');

			var n = langs.Count;
			for (var i = 0; i < n; i++)
			{
				sb.Append(EscapeTSVString(GetValue(i)));
				sb.Append('\t');
			}

			return sb.ToString();
		}
	}
}
