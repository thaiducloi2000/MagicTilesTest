using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace com.team70
{
	public class CachedString // cached short strings to reduce GC
	{
		internal static Dictionary<byte, CharNode> root = new Dictionary<byte, CharNode>();
		static readonly List<string> allCached = new List<string>();

#if UNITY_EDITOR
		[MenuItem("T70/Dev/Print cached Strings")]
#endif
		public static void Print()
		{
			var str = string.Join("\n", allCached.ToArray());
			File.WriteAllText("allCached.txt", str);
		}

		internal static int CNCount = 0;
		internal const int MAX_LENGTH = 30;

		internal class CharNode
		{
			public char c;
			public string value;
			public int counter;
			public Dictionary<byte, CharNode> dict; // do not new!
		}

		static CharNode Get(ref Dictionary<byte, CharNode> dict, char c)
		{
			if (dict == null) dict = new Dictionary<byte, CharNode>();

			if (!dict.TryGetValue((byte)c, out CharNode result))
			{
				result = new CharNode() { c = c };
				dict.Add((byte)c, result);
			}

			return result;
		}
		
		public static string Substring(string source, int st, int ed)
		{
			var char0 = source[st];
			if ((ed - st > MAX_LENGTH) || char.IsDigit(char0)) return source.Substring(st, ed-st); // prevent cache for numeric strings

			CharNode node = Get(ref root, char0);
			for (int i = st+1; i< ed; i++)
			{
				var c = source[i];
				node = Get(ref node.dict, c);
			}

			if (string.IsNullOrEmpty(node.value))
			{
				node.value = source.Substring(st, ed-st);
				allCached.Add(node.value);
			} else {
				node.counter++;
			}
			
			return node.value;
		}

		public static void Prelocate(params string[] sources)
		{
			for (int i =0;i< sources.Length; i ++)
			{
				var str = sources[i];
				Substring(str, 0, str.Length);
			}
		}
	}
}


