using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace com.team70
{
	public static class T70
	{
		public static Dictionary<TKey, T> ToDictionary<TKey, T>(List<T> list, Func<T, int, TKey> FuncGetKey)
		{
			var result = new Dictionary<TKey, T>();
			if (list == null || list.Count == 0) return result;

			for (int i = 0; i < list.Count; i++)
			{
				var item = list[i];
				if (item == null) continue;

				var key = FuncGetKey(item, i);
				if (key == null) continue;

				if (result.ContainsKey(key)) continue;
				result.Add(key, item);
			}
			return result;
		}

		public static List<Transform> GetParents(Transform child, bool includeMe = false, Transform root = null)
		{
			var result = new List<Transform>();
			if (includeMe) result.Add(child);
			if (child == null) return result;

			var p = child.parent;
			while (p != null)
			{
				result.Add(p);
				if (p == root) break;
				p = p.parent;
			}

			// reverse the result to preserve the hierarchy order
			result.Reverse();
			return result;
		}

		

		// public static string GetChildPath(Transform t)
		// {
			
		// }
		
		// public static string GetUniqueComponentID(Transform t)
		// {

		// }
		public static T GetComponent<T>(Transform t)
		{
			var typeT = typeof(T);
			var list = t.GetComponents<MonoBehaviour>();

			foreach (var m in list)
			{
				if (m == null) continue;
				var typeM = m.GetType();
				if (typeT.IsAssignableFrom(typeM)) return (T)(object)m;
			}
			return default(T);
		}

		public static List<T> GetComponentsInChildren<T>(Transform t)
		{
			var typeT = typeof(T);
			var result = new List<T>();

			if (typeof(Component).IsAssignableFrom(typeT)) // find components of Type T
			{
				AppendComponents(t, result);
			}
			else
			{
				AppendInterface(t, result);
			}

			return result;
		}

		static void AppendComponents<T>(Transform t, List<T> result)
		{
			result.AddRange(t.GetComponents<T>());
			if (t.childCount > 0)
			{
				foreach (Transform c in t)
				{
					AppendComponents(c, result);
				}
			}
		}

		static void AppendInterface<T>(Transform t, List<T> result)
		{
			var typeT = typeof(T);
			var list = t.GetComponents<MonoBehaviour>();

			foreach (var m in list)
			{
				if (m == null) continue;
				var typeM = m.GetType();
				if (typeT.IsAssignableFrom(typeM)) result.Add((T)(object)m);
			}

			if (t.childCount > 0)
			{
				foreach (Transform c in t)
				{
					AppendComponents(c, result);
				}
			}
		}

		public static bool TryParseTSV<T1>(string source, int stIndex, ref T1 t1)
		{
			if (string.IsNullOrEmpty(source)) return false;
			var result1 = TSV.ParseField(source, ref stIndex, ref t1);
			return result1;
		}
		
		public static bool TryParseTSV<T1, T2>(string source, int stIndex, ref T1 t1, ref T2 t2)
		{
			if (string.IsNullOrEmpty(source)) return false;
			var result1 = TSV.ParseField(source, ref stIndex, ref t1);
			var result2 = TSV.ParseField(source, ref stIndex, ref t2);
			return result1 && result2;
		}

		public static bool TryParseTSV<T1, T2, T3>(string source, int stIndex, ref T1 t1, ref T2 t2, ref T3 t3)
		{
			if (string.IsNullOrEmpty(source)) return false;

			var result1 = TSV.ParseField(source, ref stIndex, ref t1);
			var result2 = TSV.ParseField(source, ref stIndex, ref t2);
			var result3 = TSV.ParseField(source, ref stIndex, ref t3);
			return result1 && result2 & result3;
		}

		public static bool TryParseTSV<T1, T2, T3, T4>(string source, int stIndex, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4)
		{
			if (string.IsNullOrEmpty(source)) return false;

			var result1 = TSV.ParseField(source, ref stIndex, ref t1);
			var result2 = TSV.ParseField(source, ref stIndex, ref t2);
			var result3 = TSV.ParseField(source, ref stIndex, ref t3);
			var result4 = TSV.ParseField(source, ref stIndex, ref t4);

			return result1 && result2 & result3 & result4;
		}

		public static bool TryParseTSV<T1, T2, T3, T4, T5>(string source, int stIndex, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5)
		{
			if (string.IsNullOrEmpty(source)) return false;

			var result1 = TSV.ParseField(source, ref stIndex, ref t1);
			var result2 = TSV.ParseField(source, ref stIndex, ref t2);
			var result3 = TSV.ParseField(source, ref stIndex, ref t3);
			var result4 = TSV.ParseField(source, ref stIndex, ref t4);
			var result5 = TSV.ParseField(source, ref stIndex, ref t5);

			return result1 && result2 & result3 & result4 & result5;
		}

		public static bool TryParseTSV<T1, T2, T3, T4, T5, T6>(string source, int stIndex, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6)
		{
			if (string.IsNullOrEmpty(source)) return false;

			var result1 = TSV.ParseField(source, ref stIndex, ref t1);
			var result2 = TSV.ParseField(source, ref stIndex, ref t2);
			var result3 = TSV.ParseField(source, ref stIndex, ref t3);
			var result4 = TSV.ParseField(source, ref stIndex, ref t4);
			var result5 = TSV.ParseField(source, ref stIndex, ref t5);
			var result6 = TSV.ParseField(source, ref stIndex, ref t6);

			return result1 && result2 & result3 & result4 & result5 & result6;
		}

		public static bool TryParseTSV<T1, T2, T3, T4, T5, T6, T7>(string source, int stIndex, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7)
		{
			if (string.IsNullOrEmpty(source)) return false;

			var result1 = TSV.ParseField(source, ref stIndex, ref t1);
			var result2 = TSV.ParseField(source, ref stIndex, ref t2);
			var result3 = TSV.ParseField(source, ref stIndex, ref t3);
			var result4 = TSV.ParseField(source, ref stIndex, ref t4);
			var result5 = TSV.ParseField(source, ref stIndex, ref t5);
			var result6 = TSV.ParseField(source, ref stIndex, ref t6);
			var result7 = TSV.ParseField(source, ref stIndex, ref t7);

			return result1 && result2 & result3 & result4 & result5 & result6 & result7;
		}

		public static bool TryParseTSV<T1, T2, T3, T4, T5, T6, T7, T8>(string source, int stIndex, ref T1 t1, ref T2 t2, ref T3 t3,
			ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8)
		{
			if (string.IsNullOrEmpty(source)) return false;

			var result1 = TSV.ParseField(source, ref stIndex, ref t1);
			var result2 = TSV.ParseField(source, ref stIndex, ref t2);
			var result3 = TSV.ParseField(source, ref stIndex, ref t3);
			var result4 = TSV.ParseField(source, ref stIndex, ref t4);
			var result5 = TSV.ParseField(source, ref stIndex, ref t5);
			var result6 = TSV.ParseField(source, ref stIndex, ref t6);
			var result7 = TSV.ParseField(source, ref stIndex, ref t7);
			var result8 = TSV.ParseField(source, ref stIndex, ref t8);

			return result1 && result2 & result3 & result4 & result5 & result6 & result7 & result8;
		}

		public static bool TryParseTSV<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string source, int stIndex, ref T1 t1, ref T2 t2, ref T3 t3,
			ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9)
		{
			if (string.IsNullOrEmpty(source)) return false;

			var result1 = TSV.ParseField(source, ref stIndex, ref t1);
			var result2 = TSV.ParseField(source, ref stIndex, ref t2);
			var result3 = TSV.ParseField(source, ref stIndex, ref t3);
			var result4 = TSV.ParseField(source, ref stIndex, ref t4);
			var result5 = TSV.ParseField(source, ref stIndex, ref t5);
			var result6 = TSV.ParseField(source, ref stIndex, ref t6);
			var result7 = TSV.ParseField(source, ref stIndex, ref t7);
			var result8 = TSV.ParseField(source, ref stIndex, ref t8);
			var result9 = TSV.ParseField(source, ref stIndex, ref t9);

			return result1 && result2 & result3 & result4 & result5 & result6 & result7 & result8 & result9;
		}

		public static bool TryParseTSV<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string source, int stIndex, ref T1 t1, ref T2 t2, ref T3 t3,
			ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10)
		{
			if (string.IsNullOrEmpty(source)) return false;

			var result1 = TSV.ParseField(source, ref stIndex, ref t1);
			var result2 = TSV.ParseField(source, ref stIndex, ref t2);
			var result3 = TSV.ParseField(source, ref stIndex, ref t3);
			var result4 = TSV.ParseField(source, ref stIndex, ref t4);
			var result5 = TSV.ParseField(source, ref stIndex, ref t5);
			var result6 = TSV.ParseField(source, ref stIndex, ref t6);
			var result7 = TSV.ParseField(source, ref stIndex, ref t7);
			var result8 = TSV.ParseField(source, ref stIndex, ref t8);
			var result9 = TSV.ParseField(source, ref stIndex, ref t9);
			var result10 = TSV.ParseField(source, ref stIndex, ref t10);

			return result1 && result2 & result3 & result4 & result5 & result6 & result7 & result8 & result9 & result10;
		}
		
		public static bool TryParseTSV<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string source, int stIndex, ref T1 t1, ref T2 t2, ref T3 t3,
			ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10, ref T11 t11)
		{
			if (string.IsNullOrEmpty(source)) return false;

			var result1 = TSV.ParseField(source, ref stIndex, ref t1);
			var result2 = TSV.ParseField(source, ref stIndex, ref t2);
			var result3 = TSV.ParseField(source, ref stIndex, ref t3);
			var result4 = TSV.ParseField(source, ref stIndex, ref t4);
			var result5 = TSV.ParseField(source, ref stIndex, ref t5);
			var result6 = TSV.ParseField(source, ref stIndex, ref t6);
			var result7 = TSV.ParseField(source, ref stIndex, ref t7);
			var result8 = TSV.ParseField(source, ref stIndex, ref t8);
			var result9 = TSV.ParseField(source, ref stIndex, ref t9);
			var result10 = TSV.ParseField(source, ref stIndex, ref t10);
			var result11 = TSV.ParseField(source, ref stIndex, ref t11);

			return result1 && result2 & result3 & result4 & result5 & result6 & result7 & result8 & result9 & result10 & result11;
		}
		
		public static bool TryParseTSV<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string source, int stIndex, ref T1 t1, ref T2 t2, ref T3 t3,
			ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10, ref T11 t11, ref T12 t12)
		{
			if (string.IsNullOrEmpty(source)) return false;

			var result1 = TSV.ParseField(source, ref stIndex, ref t1);
			var result2 = TSV.ParseField(source, ref stIndex, ref t2);
			var result3 = TSV.ParseField(source, ref stIndex, ref t3);
			var result4 = TSV.ParseField(source, ref stIndex, ref t4);
			var result5 = TSV.ParseField(source, ref stIndex, ref t5);
			var result6 = TSV.ParseField(source, ref stIndex, ref t6);
			var result7 = TSV.ParseField(source, ref stIndex, ref t7);
			var result8 = TSV.ParseField(source, ref stIndex, ref t8);
			var result9 = TSV.ParseField(source, ref stIndex, ref t9);
			var result10 = TSV.ParseField(source, ref stIndex, ref t10);
			var result11 = TSV.ParseField(source, ref stIndex, ref t11);
			var result12 = TSV.ParseField(source, ref stIndex, ref t12);

			return result1 && result2 & result3 & result4 & result5 & result6 & result7 & result8 & result9 & result10 & result11 & result12;
		}
		
		public static bool TryParseTSV<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string source, int stIndex, ref T1 t1, ref T2 t2, ref T3 t3,
			ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10, ref T11 t11, ref T12 t12, ref T13 t13)
		{
			if (string.IsNullOrEmpty(source)) return false;

			var result1 = TSV.ParseField(source, ref stIndex, ref t1);
			var result2 = TSV.ParseField(source, ref stIndex, ref t2);
			var result3 = TSV.ParseField(source, ref stIndex, ref t3);
			var result4 = TSV.ParseField(source, ref stIndex, ref t4);
			var result5 = TSV.ParseField(source, ref stIndex, ref t5);
			var result6 = TSV.ParseField(source, ref stIndex, ref t6);
			var result7 = TSV.ParseField(source, ref stIndex, ref t7);
			var result8 = TSV.ParseField(source, ref stIndex, ref t8);
			var result9 = TSV.ParseField(source, ref stIndex, ref t9);
			var result10 = TSV.ParseField(source, ref stIndex, ref t10);
			var result11 = TSV.ParseField(source, ref stIndex, ref t11);
			var result12 = TSV.ParseField(source, ref stIndex, ref t12);
			var result13 = TSV.ParseField(source, ref stIndex, ref t13);

			return result1 && result2 & result3 & result4 & result5 & result6 & result7 & result8 & result9 & result10 & result11 & result12 & result13;
		}
		
		public static bool TryParseTSV<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(string source, int stIndex, ref T1 t1, ref T2 t2, ref T3 t3,
			ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10, ref T11 t11, ref T12 t12, ref T13 t13, ref T14 t14)
		{
			if (string.IsNullOrEmpty(source)) return false;

			var result1 = TSV.ParseField(source, ref stIndex, ref t1);
			var result2 = TSV.ParseField(source, ref stIndex, ref t2);
			var result3 = TSV.ParseField(source, ref stIndex, ref t3);
			var result4 = TSV.ParseField(source, ref stIndex, ref t4);
			var result5 = TSV.ParseField(source, ref stIndex, ref t5);
			var result6 = TSV.ParseField(source, ref stIndex, ref t6);
			var result7 = TSV.ParseField(source, ref stIndex, ref t7);
			var result8 = TSV.ParseField(source, ref stIndex, ref t8);
			var result9 = TSV.ParseField(source, ref stIndex, ref t9);
			var result10 = TSV.ParseField(source, ref stIndex, ref t10);
			var result11 = TSV.ParseField(source, ref stIndex, ref t11);
			var result12 = TSV.ParseField(source, ref stIndex, ref t12);
			var result13 = TSV.ParseField(source, ref stIndex, ref t13);
			var result14 = TSV.ParseField(source, ref stIndex, ref t14);

			return result1 && result2 & result3 & result4 & result5 & result6 & result7 & result8 & result9 & result10 & result11 & result12 & result13 & result14;
		}
		
		public static bool TryParseTSV<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(string source, int stIndex, ref T1 t1, ref T2 t2, ref T3 t3,
		    ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10, ref T11 t11, ref T12 t12, ref T13 t13, ref T14 t14, ref T15 t15)
		{
			if (string.IsNullOrEmpty(source)) return false;

			var result1 = TSV.ParseField(source, ref stIndex, ref t1);
			var result2 = TSV.ParseField(source, ref stIndex, ref t2);
			var result3 = TSV.ParseField(source, ref stIndex, ref t3);
			var result4 = TSV.ParseField(source, ref stIndex, ref t4);
			var result5 = TSV.ParseField(source, ref stIndex, ref t5);
			var result6 = TSV.ParseField(source, ref stIndex, ref t6);
			var result7 = TSV.ParseField(source, ref stIndex, ref t7);
			var result8 = TSV.ParseField(source, ref stIndex, ref t8);
			var result9 = TSV.ParseField(source, ref stIndex, ref t9);
			var result10 = TSV.ParseField(source, ref stIndex, ref t10);
			var result11 = TSV.ParseField(source, ref stIndex, ref t11);
			var result12 = TSV.ParseField(source, ref stIndex, ref t12);
			var result13 = TSV.ParseField(source, ref stIndex, ref t13);
			var result14 = TSV.ParseField(source, ref stIndex, ref t14);
			var result15 = TSV.ParseField(source, ref stIndex, ref t15);

			return result1 && result2 & result3 & result4 & result5 & result6 & result7 & result8 & result9 & result10 & result11 & result12 & result13 & result14 & result15;
		}
		
		public static bool TryParseTSV<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(string source, int stIndex, ref T1 t1, ref T2 t2, ref T3 t3, 
		    ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10, ref T11 t11, ref T12 t12, ref T13 t13, ref T14 t14, ref T15 t15, ref T16 t16)
		{
			if (string.IsNullOrEmpty(source)) return false;

			var result1 = TSV.ParseField(source, ref stIndex, ref t1);
			var result2 = TSV.ParseField(source, ref stIndex, ref t2);
			var result3 = TSV.ParseField(source, ref stIndex, ref t3);
			var result4 = TSV.ParseField(source, ref stIndex, ref t4);
			var result5 = TSV.ParseField(source, ref stIndex, ref t5);
			var result6 = TSV.ParseField(source, ref stIndex, ref t6);
			var result7 = TSV.ParseField(source, ref stIndex, ref t7);
			var result8 = TSV.ParseField(source, ref stIndex, ref t8);
			var result9 = TSV.ParseField(source, ref stIndex, ref t9);
			var result10 = TSV.ParseField(source, ref stIndex, ref t10);
			var result11 = TSV.ParseField(source, ref stIndex, ref t11);
			var result12 = TSV.ParseField(source, ref stIndex, ref t12);
			var result13 = TSV.ParseField(source, ref stIndex, ref t13);
			var result14 = TSV.ParseField(source, ref stIndex, ref t14);
			var result15 = TSV.ParseField(source, ref stIndex, ref t15);
			var result16 = TSV.ParseField(source, ref stIndex, ref t16);

			return result1 && result2 & result3 & result4 & result5 & result6 & result7 & result8 & result9 & result10 & result11 & result12 & result13 & result14 & result15 & result16;
		}
		
		public static bool TryParseTSV<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(string source, int stIndex, ref T1 t1, ref T2 t2, ref T3 t3, 
		    ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10, ref T11 t11, ref T12 t12, ref T13 t13, ref T14 t14, ref T15 t15, ref T16 t16, ref T17 t17)
		{
			if (string.IsNullOrEmpty(source)) return false;

			var result1 = TSV.ParseField(source, ref stIndex, ref t1);
			var result2 = TSV.ParseField(source, ref stIndex, ref t2);
			var result3 = TSV.ParseField(source, ref stIndex, ref t3);
			var result4 = TSV.ParseField(source, ref stIndex, ref t4);
			var result5 = TSV.ParseField(source, ref stIndex, ref t5);
			var result6 = TSV.ParseField(source, ref stIndex, ref t6);
			var result7 = TSV.ParseField(source, ref stIndex, ref t7);
			var result8 = TSV.ParseField(source, ref stIndex, ref t8);
			var result9 = TSV.ParseField(source, ref stIndex, ref t9);
			var result10 = TSV.ParseField(source, ref stIndex, ref t10);
			var result11 = TSV.ParseField(source, ref stIndex, ref t11);
			var result12 = TSV.ParseField(source, ref stIndex, ref t12);
			var result13 = TSV.ParseField(source, ref stIndex, ref t13);
			var result14 = TSV.ParseField(source, ref stIndex, ref t14);
			var result15 = TSV.ParseField(source, ref stIndex, ref t15);
			var result16 = TSV.ParseField(source, ref stIndex, ref t16);
			var result17 = TSV.ParseField(source, ref stIndex, ref t17);

			return result1 && result2 & result3 & result4 & result5 & result6 & result7 & result8 & result9 & result10 & result11 & result12 & result13 & result14 & result15 & result16 & result17;
		}


		// HELPERS
		
		public static bool TryParseTSV<T1>(string source, ref T1 t1)
		{
			return TryParseTSV(source, 0, ref t1);
		}
		
		public static bool TryParseTSV<T1, T2>(string source, ref T1 t1, ref T2 t2)
		{
			return TryParseTSV(source, 0, ref t1, ref t2);
		}

		public static bool TryParseTSV<T1, T2, T3>(string source, ref T1 t1, ref T2 t2, ref T3 t3)
		{
			return TryParseTSV(source, 0, ref t1, ref t2, ref t3);
		}
		public static bool TryParseTSV<T1, T2, T3, T4>(string source, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4)
		{
			return TryParseTSV(source, 0, ref t1, ref t2, ref t3, ref t4);
		}
		public static bool TryParseTSV<T1, T2, T3, T4, T5>(string source, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5)
		{
			return TryParseTSV(source, 0, ref t1, ref t2, ref t3, ref t4, ref t5);
		}
		public static bool TryParseTSV<T1, T2, T3, T4, T5, T6>(string source, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6)
		{
			return TryParseTSV(source, 0, ref t1, ref t2, ref t3, ref t4, ref t5, ref t6);
		}
		public static bool TryParseTSV<T1, T2, T3, T4, T5, T6, T7>(string source, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7)
		{
			return TryParseTSV(source, 0, ref t1, ref t2, ref t3, ref t4, ref t5, ref t6, ref t7);
		}
		public static bool TryParseTSV<T1, T2, T3, T4, T5, T6, T7, T8>(string source, ref T1 t1, ref T2 t2, ref T3 t3,
			ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8)
		{
			return TryParseTSV(source, 0, ref t1, ref t2, ref t3, ref t4, ref t5, ref t6, ref t7, ref t8);
		}
		public static bool TryParseTSV<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string source, ref T1 t1, ref T2 t2, ref T3 t3,
			ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9)
		{
			return TryParseTSV(source, 0, ref t1, ref t2, ref t3, ref t4, ref t5, ref t6, ref t7, ref t8, ref t9);
		}
		public static bool TryParseTSV<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string source, ref T1 t1, ref T2 t2, ref T3 t3,
			ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8, ref T9 t9, ref T10 t10)
		{
			return TryParseTSV(source, 0, ref t1, ref t2, ref t3, ref t4, ref t5, ref t6, ref t7, ref t8, ref t9, ref t10);
		}

		public static string ToTSV(params object[] data)
		{
			var sb = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				sb.Append(data[i].ToString());
				sb.Append("\t");
			}
			return sb.ToString();
		}

		public static int SecondsTS
		{
			get { return (int)(DateTime.Now - new DateTime(2010, 1, 1)).TotalSeconds; }
		}

		public static string GetTempPath(string path, string fileName, bool createDir = false)
		{
#if UNITY_EDITOR
			var localDir = Path.Combine(Application.dataPath.Replace("Assets", string.Empty), "Temp/" + path);
#else
        	var localDir = Path.Combine(Application.temporaryCachePath, path);
#endif
			if (createDir) Directory.CreateDirectory(localDir);
			return Path.Combine(localDir, fileName);
		}

		public static string GetPersistentPath(string path, string fileName, bool createDir = false)
		{
#if UNITY_EDITOR
			var localDir = Path.Combine(Application.dataPath.Replace("Assets", string.Empty), "Library/" + path);
#else
			var localDir = Path.Combine(Application.persistentDataPath, path);
#endif
			if (createDir) Directory.CreateDirectory(localDir);
			return Path.Combine(localDir, fileName);
		}


		[Obsolete("Deprecated, use GetTempPath() instead!")]
		public static string GetTemp(string path, string fileName, bool createDir = false)
		{
			return GetTempPath(path, fileName, createDir);
		}

		[Obsolete("Deprecated, use GetPersistentPath() instead!")]
		public static string GetPersistent(string path, string fileName, bool createDir = false)
		{
			return GetPersistentPath(path, fileName, createDir);
		}

#if UNITY_EDITOR

		#region CodeGenerator

		public static void CreateEnum(string enumName, string saveLocation, params string[] enumEntries)
		{
			string classDefinition = string.Empty;
			classDefinition += "public enum " + enumName + "\n";
			classDefinition += "{" + "\n";
			classDefinition = enumEntries.Aggregate(classDefinition, (current, t)
				=> current + ("    " + ClearSpecialChar(t) + "," + "\n"));

			classDefinition += "}" + "\n";
			File.WriteAllText(saveLocation, classDefinition);
		}

		public static string ClearSpecialChar(string input)
		{
			var res = input.Replace('.', '_');
			res = res.Replace(' ', '_');
			res = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(res);
			res = res.Replace("_", "");
			res = res.Trim(' ');
			return res;
		}

		public static void CreateConstString(string className, string saveLocation, params string[] collectors)
		{
			string classDefinition = string.Empty;
			classDefinition += "public static class " + className + "\n";
			classDefinition += "{" + "\n";
			foreach (var item in collectors)
			{
				var nameField = ClearSpecialChar(item);
				classDefinition += "    " + "public const string " + nameField;
				classDefinition += " = \"" + item + "\";" + "\n";
			}

			classDefinition += "}" + "\n";
			File.WriteAllText(saveLocation, classDefinition);
		}

		public static void CreateConstIndex(string className, string saveLocation, params string[] collectors)
		{
			string classDefinition = string.Empty;
			classDefinition += "public static class " + className + "\n";
			classDefinition += "{" + "\n";
			for (var index = 0; index < collectors.Length; index++)
			{
				var item = collectors[index];
				var nameField = ClearSpecialChar(item);
				classDefinition += "    " + "public const int " + nameField;
				classDefinition += " = " + index + ";" + "\n";
			}
			classDefinition += "}" + "\n";
			File.WriteAllText(saveLocation, classDefinition);
		}

		public static void CreateStringCollector(string saveLocation, params AudioClip[] collectors)
		{
			StringCollector asset = ScriptableObject.CreateInstance<StringCollector>();
			var lstGet = collectors.Select(k => k.name);
			asset.list = lstGet.ToList();
			AssetDatabase.CreateAsset(asset, saveLocation);
			AssetDatabase.SaveAssets();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;
		}

		#endregion

#endif
	}
}


