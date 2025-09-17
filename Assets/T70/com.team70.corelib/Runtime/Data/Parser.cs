using System.Collections.Generic;
using System;
using System.Globalization;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace com.team70
{
	public class Parser
	{
#if UNITY_EDITOR
		[MenuItem("T70/Dev/Test string")]
#endif
		public static void Test()
		{
			var str = "A23456789 B23456789 C23456789 D23456789 E23456789 F23456789 G23456789 H23456789 I23456789 J23456789 K23456789 L23456789 M23456789 N23456789 M23456789";

			UnityEngine.Profiling.Profiler.BeginSample("Test string");
			{
				for (int j = 0; j < 100000; j++)
				{
					for (int i = 1; i < 100; i++)
					{
						TempSlice(str, 0, i);
						// if ((i+j) % 99 == 0) Debug.Log(tempString);
					}
				}
			}
			UnityEngine.Profiling.Profiler.EndSample();
		}

		static HashSet<Type> UnsupportedTypes = new HashSet<Type>();
		public static bool useCacheString = false;

		public static bool TryParse<T>(string source, int st, int ed, ref T output)
		{
			if (string.IsNullOrEmpty(source)) return false;

			var typeT = typeof(T);

			if (typeT == typeof(string))
			{
				var subString = useCacheString ? CachedString.Substring(source, st, ed) : source.Substring(st, ed - st);
				output = (T)(object)subString;
				return true;
			}

			output = default(T);
			if (typeT == typeof(bool))
			{
				TempSlice(source, st, ed);//source.Substring(st, ed-st);
				output = (T)(object)(tempString == "true" || tempString == "1");
				return true;
			}

			if (typeT == typeof(int))
			{
				var intValue = 0;
				if (ReadInt(source, st, ed, ref intValue))
				{
					output = (T)(object)intValue;
					return true;
				}
#if UNITY_EDITOR
				// Debug.LogWarning($"Parse int failed: <{source.Substring(st, ed - st)}>");
#endif
				output = (T)(object)-1;
				return true;
				// return false;
			}

			if (typeT == typeof(float))
			{
				var floatValue = 0f;
				if (ReadFloat(source, st, ed, ref floatValue))
				{
					output = (T)(object)floatValue;
					return true;
				}
#if UNITY_EDITOR
				// Debug.LogWarning($"Parse float failed: <{source.Substring(st, ed - st)}>");
#endif
				output = (T)(object)-1f;
				return true;
				// return false;
			}

			if (typeT == typeof(double))
			{
				var doubleValue = 0d;
				if (ReadDouble(source, st, ed, ref doubleValue))
				{
					output = (T)(object)doubleValue;
					return true;
				}
#if UNITY_EDITOR			
				// Debug.LogWarning($"Parse double failed: <{source.Substring(st, ed - st)}>");
#endif
				output = (T)(object)-1d;
				return true;
				// return false;
			}

#if UNITY_EDITOR
			if (UnsupportedTypes.Contains(typeT)) return false;
			UnsupportedTypes.Add(typeT);
			Debug.LogWarning("Unsupported type: " + typeT);
#endif
			return false;
		}

		static string tempString; // do not init default value here!
		static unsafe void TempSlice(string source, int st, int ed)
		{
			if (tempString == null) // tempString must be create at runtime to be dynamic
			{
				tempString = new string('\0', 1024); //maxsize: 1024
			}

			var length = ed - st;
			if (length < 0 || length >= 1024)
				throw new ArgumentOutOfRangeException($"Invalid TempSlice length {length}");

			// copy chars
			fixed (char* p = tempString)
			{
				int* pi = (int*)p;
				pi[-1] = length;
				p[length] = '\0';

				for (int i = 0; i < length; i++)
				{
					p[i] = source[st + i];
				}
			}
		}

		static bool ReadInt(string source, int st, int ed, ref int result)
		{
			TempSlice(source, st, ed);
			return int.TryParse(tempString, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out result);
		}

		static bool ReadDouble(string source, int st, int ed, ref double result)
		{
			TempSlice(source, st, ed);
			return double.TryParse(tempString, NumberStyles.Float | NumberStyles.AllowThousands, System.Globalization.CultureInfo.InvariantCulture, out result);
		}

		static bool ReadFloat(string source, int st, int ed, ref float result)
		{
			TempSlice(source, st, ed);
			return float.TryParse(tempString, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
		}
	}
}