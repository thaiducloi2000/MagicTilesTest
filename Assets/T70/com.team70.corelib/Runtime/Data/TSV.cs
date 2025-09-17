using System.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.team70
{
	internal class TempString
	{
		private const int MAX_SIZE = 1024;
		public string current;
		public unsafe void TempSlice(string source, int st, int ed)
		{
			if (current == null) // tempString.value must be create at runtime to be dynamic
			{
				current = new string('\0', MAX_SIZE); //maxsize: 1024
			}

			if (source.Length <= ed)
			{
				Debug.LogWarning($"Invalid ed: {st} --> {ed}/{source.Length}");
				return;
			}
			
			var length = ed - st;
			if (length < 0 || length >= MAX_SIZE)
				throw new ArgumentOutOfRangeException($"Invalid tempString.TempSlice length {length} > MaxSize = {MAX_SIZE}");

			// copy chars
			fixed (char* p = current)
			{
				var pi = (int*)p;
				pi[-1] = length;
				p[length] = '\0';

				for (var i = 0; i < length; i++)
				{
					p[i] = source[st + i];
				}
			}
		}
	}
	
	public class TSV
	{
		const char CR 	= '\r';
		const char LF 	= '\n';
		const char TAB	= '\t';
		
		public static bool ParseField<T>(string tsv, ref int stIndex, ref T result)
		{
			var length = tsv.Length;
			
			var isEOF = stIndex == length;
			if (isEOF)
			{
				return Parser.TryParse(tsv, stIndex, length, ref result);
			}
			
			for (int i = stIndex; i < length; i++)
			{
				var c = tsv[i];
				var isTab = c == TAB;
				isEOF = i == (length - 1);
				var isEOL = c == CR || c == LF;
				
				if (isTab || isEOL || isEOF)
				{
					var edIdx = i;
					if (isEOF)
					{
						edIdx = isTab ? length - 1 : length;
					}
					var success = Parser.TryParse(tsv, stIndex, edIdx, ref result);
					
					if (isEOL)
					{
						while (i < length) // skip through empty lines
						{
							c = tsv[i++];
							if (c == CR || c == LF) continue;
						}
					}
					
					stIndex = i + 1;
					return success;
				}
			}
			
			return false;
		}
		
		public class Token 
		{
			public int stCharIdx;
			public int edCharIdx;
		}

		

		public interface ITSVLine
		{
			void FromTSVLine(TSVLineInfo line);
		}

		public partial class TSVLineInfo
		{
			private static TempString tempString = new TempString();
			static NumberStyles NUMBER_STYLE = 	NumberStyles.AllowLeadingWhite | 
												NumberStyles.AllowLeadingSign | 
												NumberStyles.AllowThousands |
												NumberStyles.AllowDecimalPoint | 
												NumberStyles.AllowExponent | 
												NumberStyles.AllowTrailingWhite;
			
			public string source;
			public int lineIndex;
			public int nTokens;
			public int stCharIdx; // line start char index
			public int edCharIdx; // line end char index

			public List<Token> tokens;

			public void LogTokens()
			{
				for (int i =0 ;i< nTokens;i ++)
				{
					Debug.Log($"Token {i}: <{ReadString(i)}>");
				}
			}

			internal Token GetTokenAt(int tokenIndex)
			{
				if (tokens.Count > tokenIndex) return tokens[tokenIndex];
				var token = new Token();
				tokens.Add(token);
				return token;
			}
			
			public string ReadLine()
			{
				return source.Substring(stCharIdx, edCharIdx - stCharIdx);
			}

			public object Read(int tokenIdx, Type type)
			{
				// C# PRIMITY
				if (type == typeof(string))		return ReadString(tokenIdx);
				if (type == typeof(int)) 		return ReadInt(tokenIdx);
				if (type == typeof(float)) 		return ReadFloat(tokenIdx);
				if (type == typeof(bool)) 		return ReadBool(tokenIdx);
				if (type == typeof(long)) 		return ReadLong(tokenIdx);

				if (type == typeof(uint)) 		return ReadUint(tokenIdx);
				if (type == typeof(double)) 	return ReadDouble(tokenIdx);
				if (type == typeof(decimal)) 	return ReadDecimal(tokenIdx);
				if (type == typeof(ulong)) 		return ReadUlong(tokenIdx);

				if (type.IsEnum)
				{
					var token = tokens[tokenIdx];
					tempString.TempSlice(source, token.stCharIdx, token.edCharIdx);
					return int.TryParse(tempString.current, NUMBER_STYLE, CultureInfo.InvariantCulture, out int result) 
						? result : Enum.Parse(type, tempString.current);

				}
				
				// UNITY STRUCTS
				if (type == typeof(Vector2)) 	return ReadVector2(tokenIdx);
				if (type == typeof(Vector3)) 	return ReadVector3(tokenIdx);
				if (type == typeof(Vector4)) 	return ReadVector4(tokenIdx);

				if (type == typeof(Color)) 		return ReadColor(tokenIdx);
				if (type == typeof(Color32)) 	return (Color32)ReadColor(tokenIdx);
				
				Debug.LogWarning($"Unsupported type: {type}");
				return null;
			}
			

			public object ReadEnum(Type enumType, int tokenIdx)
			{
				var token = tokens[tokenIdx];
				tempString.TempSlice(source, token.stCharIdx, token.edCharIdx);
				
				if (token.stCharIdx == token.edCharIdx) return Activator.CreateInstance(enumType);

				try 
				{
					return Enum.Parse(enumType, tempString.current);
				} 
				catch 
				{
					Debug.LogWarning($"ReadEnum Failed: {enumType} : <{tempString.current}>");
					return Activator.CreateInstance(enumType);
				}
			}

			public T ReadEnum<T>(int tokenIdx)
			{
				return (T)ReadEnum(typeof(T), tokenIdx);
			}
			
			T ReadCSVArray<T>(int tokenIdx, Func<TSVLineInfo, T> func, T defaultValue)
			{
				var token 	= tokens[tokenIdx];
				var st 		= token.stCharIdx;
				var ed 		= token.edCharIdx;
				
				foreach (var tk in Tokenize(source, st, ed, ','))
				{
					return func(tk);
				}

				return defaultValue;
			}

			public Vector2 ReadVector2(int tokenIdx, Vector2 fallback = default(Vector2))
			{
				return ReadCSVArray(tokenIdx, (tk)=>
				{	
					Debug.Log("<" + tk.ReadString(0) + "> | <" + tk.ReadString(1) + ">");

					return (tk.nTokens == 2) 
						? new Vector2(tk.ReadFloat(0), tk.ReadFloat(1)) 
						: fallback;
				}, fallback);
			}
			
			public Vector3 ReadVector3(int tokenIdx, Vector3 fallback = default(Vector3))
			{
				return ReadCSVArray(tokenIdx, (tk)=>
				{
					return (tk.nTokens == 3) 
						? new Vector3(tk.ReadFloat(0), tk.ReadFloat(1), tk.ReadFloat(2))
						: fallback;
				}, fallback);
			}

			public Vector4 ReadVector4(int tokenIdx, Vector4 fallback = default(Vector4))
			{
				return ReadCSVArray(tokenIdx, (tk)=>
				{
					return (tk.nTokens == 4) 
						? new Vector4(tk.ReadFloat(0), tk.ReadFloat(1), tk.ReadFloat(2), tk.ReadFloat(3))
						: fallback;
				}, fallback);
			}

			public Color ReadColor(int tokenIdx, Color fallback = default(Color))
			{
				var token = tokens[tokenIdx];
				tempString.TempSlice(source, token.stCharIdx, token.edCharIdx);
				return ColorUtility.TryParseHtmlString(tempString.current, out Color color) ? color : fallback;
			}
			
			public string ReadString(int tokenIdx, bool trim = true, string fallback = null, bool useCache = false)
			{

				if (tokenIdx >= nTokens)
				{
					#if UNITY_EDITOR
					tempString.TempSlice(source, stCharIdx, edCharIdx);
					Debug.LogWarning("invalid Token index: " + tokenIdx + " / " + nTokens + "\n[" + tempString.current + "]");
					#endif
					return string.Empty;
				}
				
				var token = tokens[tokenIdx];

				var st = token.stCharIdx;
				var ed = token.edCharIdx;

				if (trim)
				{
					while (st < ed && source[st] == ' ')
					{
						st++;
					}

					while (ed > st && source[ed] == ' ')
					{
						ed--;
					}
				}
				
				if (st == ed) return fallback ?? string.Empty;
				return useCache ? CachedString.Substring(source, st, ed): source.Substring(st, ed - st);
			}

			public int ReadInt(int tokenIdx, int fallback = 0)
			{
				var token = tokens[tokenIdx];
				tempString.TempSlice(source, token.stCharIdx, token.edCharIdx);
				return int.TryParse(tempString.current, NUMBER_STYLE, CultureInfo.InvariantCulture, out int result) ? result : fallback;
			}

			public uint ReadUint(int tokenIdx, uint fallback = 0)
			{
				var token = tokens[tokenIdx];
				tempString.TempSlice(source, token.stCharIdx, token.edCharIdx);
				return uint.TryParse(tempString.current, NUMBER_STYLE, CultureInfo.InvariantCulture, out uint result) ? result : fallback;
			}

			public long ReadLong(int tokenIdx, long fallback = 0)
			{
				var token = tokens[tokenIdx];
				tempString.TempSlice(source, token.stCharIdx, token.edCharIdx);
				return long.TryParse(tempString.current, NUMBER_STYLE, CultureInfo.InvariantCulture, out long result) ? result : fallback;
			}

			public ulong ReadUlong(int tokenIdx, ulong fallback = 0)
			{
				var token = tokens[tokenIdx];
				tempString.TempSlice(source, token.stCharIdx, token.edCharIdx);
				return ulong.TryParse(tempString.current, NUMBER_STYLE, CultureInfo.InvariantCulture, out ulong result) ? result : fallback;
			}
			
			public float ReadFloat(int tokenIdx, float fallback = 0)
			{
				var token = tokens[tokenIdx];
				tempString.TempSlice(source, token.stCharIdx, token.edCharIdx);
				return float.TryParse(tempString.current, NUMBER_STYLE, CultureInfo.InvariantCulture, out float result) ? result : fallback;
			}

			public double ReadDouble(int tokenIdx, double fallback = 0)
			{
				var token = tokens[tokenIdx];
				tempString.TempSlice(source, token.stCharIdx, token.edCharIdx);
				return double.TryParse(tempString.current, NUMBER_STYLE, CultureInfo.InvariantCulture, out double result) ? result : fallback;
			}

			public decimal ReadDecimal(int tokenIdx, decimal fallback = 0)
			{
				var token = tokens[tokenIdx];
				tempString.TempSlice(source, token.stCharIdx, token.edCharIdx);
				return Decimal.TryParse(tempString.current, NUMBER_STYLE, CultureInfo.InvariantCulture, out decimal result) ? result : fallback;
			}

			public bool ReadBool(int tokenIdx, bool fallback = false)
			{
				var token = tokens[tokenIdx];
				tempString.TempSlice(source, token.stCharIdx, token.edCharIdx);
				
				switch (tempString.current)
				{
					case "1":
					case "true":
					case "T":
						return true;
					case "0":
					case "false":
					case "F":
						return false;
					default:
						return fallback;
				}
			}

			public override string ToString()
			{
				return $"{lineIndex} | {nTokens} | <{ReadLine()}>";
			}
		}

		public partial class TSVLineInfo // support for parse specific data types: floats, ints, strings
		{
			//
			//	READ FLOATS
			//
			public void ReadFloats(ref float var1, int tk1)
			{
				var1 = ReadFloat(tk1);
			}

			public void ReadFloats(ref float var1, int tk1, ref float var2, int tk2)
			{
				var1 = ReadFloat(tk1);
				var2 = ReadFloat(tk2);
			}

			public void ReadFloats(ref float var1, int tk1, ref float var2, int tk2, ref float var3, int tk3)
			{
				var1 = ReadFloat(tk1);
				var2 = ReadFloat(tk2);
				var3 = ReadFloat(tk3);
			}

			public void ReadFloats(	ref float var1, int tk1, ref float var2, int tk2, ref float var3, int tk3, 
									ref float var4, int tk4)
			{
				var1 = ReadFloat(tk1);	var2 = ReadFloat(tk2);
				var3 = ReadFloat(tk3);	var4 = ReadFloat(tk4);
			}

			public void ReadFloats(	ref float var1, int tk1, ref float var2, int tk2, ref float var3, int tk3, 
									ref float var4, int tk4, ref float var5, int tk5)
			{
				var1 = ReadFloat(tk1);	var2 = ReadFloat(tk2);
				var3 = ReadFloat(tk3); 	var4 = ReadFloat(tk4);
				var5 = ReadFloat(tk5);
			}

			public void ReadFloats(	ref float var1, int tk1, ref float var2, int tk2, ref float var3, int tk3, 
									ref float var4, int tk4, ref float var5, int tk5, ref float var6, int tk6)
			{
				var1 = ReadFloat(tk1);	var2 = ReadFloat(tk2);
				var3 = ReadFloat(tk3); 	var4 = ReadFloat(tk4);
				var5 = ReadFloat(tk5);	var6 = ReadFloat(tk6);
			}

			public void ReadFloats(	ref float var1, int tk1, ref float var2, int tk2, ref float var3, int tk3, 
									ref float var4, int tk4, ref float var5, int tk5, ref float var6, int tk6,
									ref float var7, int tk7)
			{
				var1 = ReadFloat(tk1);	var2 = ReadFloat(tk2);
				var3 = ReadFloat(tk3); 	var4 = ReadFloat(tk4);
				var5 = ReadFloat(tk5);	var6 = ReadFloat(tk6);
				var7 = ReadFloat(tk7);
			}

			public void ReadFloats(	ref float var1, int tk1, ref float var2, int tk2, ref float var3, int tk3, 
									ref float var4, int tk4, ref float var5, int tk5, ref float var6, int tk6,
									ref float var7, int tk7, ref float var8, int tk8)
			{
				var1 = ReadFloat(tk1);	var2 = ReadFloat(tk2);
				var3 = ReadFloat(tk3); 	var4 = ReadFloat(tk4);
				var5 = ReadFloat(tk5);	var6 = ReadFloat(tk6);
				var7 = ReadFloat(tk7);	var8 = ReadFloat(tk8);
			}

			public void ReadFloats(	ref float var1, int tk1, ref float var2, int tk2, ref float var3, int tk3, 
									ref float var4, int tk4, ref float var5, int tk5, ref float var6, int tk6,
									ref float var7, int tk7, ref float var8, int tk8, ref float var9, int tk9)
			{
				var1 = ReadFloat(tk1);	var2 = ReadFloat(tk2);
				var3 = ReadFloat(tk3); 	var4 = ReadFloat(tk4);
				var5 = ReadFloat(tk5);	var6 = ReadFloat(tk6);
				var7 = ReadFloat(tk7);	var8 = ReadFloat(tk8);
				var9 = ReadFloat(tk9);	
			}

			public void ReadFloats(	ref float var1, int tk1, ref float var2, int tk2, ref float var3, int tk3, 
									ref float var4, int tk4, ref float var5, int tk5, ref float var6, int tk6,
									ref float var7, int tk7, ref float var8, int tk8, ref float var9, int tk9,
									ref float var10, int tk10)
			{
				var1 = ReadFloat(tk1);	var2 = ReadFloat(tk2);
				var3 = ReadFloat(tk3); 	var4 = ReadFloat(tk4);
				var5 = ReadFloat(tk5);	var6 = ReadFloat(tk6);
				var7 = ReadFloat(tk7);	var8 = ReadFloat(tk8);
				var9 = ReadFloat(tk9);	var10 = ReadFloat(tk10);
			}
			
			//
			//	READ INTS
			//
			public void ReadInts(ref int var1, int tk1)
			{
				var1 = ReadInt(tk1);
			}

			public void ReadInts(ref int var1, int tk1, ref int var2, int tk2)
			{
				var1 = ReadInt(tk1);
				var2 = ReadInt(tk2);
			}

			public void ReadInts(ref int var1, int tk1, ref int var2, int tk2, ref int var3, int tk3)
			{
				var1 = ReadInt(tk1);
				var2 = ReadInt(tk2);
				var3 = ReadInt(tk3);
			}

			public void ReadInts(	ref int var1, int tk1, ref int var2, int tk2, ref int var3, int tk3, 
									ref int var4, int tk4)
			{
				var1 = ReadInt(tk1);	var2 = ReadInt(tk2);
				var3 = ReadInt(tk3);	var4 = ReadInt(tk4);
			}

			public void ReadInts(	ref int var1, int tk1, ref int var2, int tk2, ref int var3, int tk3, 
									ref int var4, int tk4, ref int var5, int tk5)
			{
				var1 = ReadInt(tk1);	var2 = ReadInt(tk2);
				var3 = ReadInt(tk3); 	var4 = ReadInt(tk4);
				var5 = ReadInt(tk5);
			}

			public void ReadInts(	ref int var1, int tk1, ref int var2, int tk2, ref int var3, int tk3, 
									ref int var4, int tk4, ref int var5, int tk5, ref int var6, int tk6)
			{
				var1 = ReadInt(tk1);	var2 = ReadInt(tk2);
				var3 = ReadInt(tk3); 	var4 = ReadInt(tk4);
				var5 = ReadInt(tk5);	var6 = ReadInt(tk6);
			}

			public void ReadInts(	ref int var1, int tk1, ref int var2, int tk2, ref int var3, int tk3, 
									ref int var4, int tk4, ref int var5, int tk5, ref int var6, int tk6,
									ref int var7, int tk7)
			{
				var1 = ReadInt(tk1);	var2 = ReadInt(tk2);
				var3 = ReadInt(tk3); 	var4 = ReadInt(tk4);
				var5 = ReadInt(tk5);	var6 = ReadInt(tk6);
				var7 = ReadInt(tk7);
			}

			public void ReadInts(	ref int var1, int tk1, ref int var2, int tk2, ref int var3, int tk3, 
									ref int var4, int tk4, ref int var5, int tk5, ref int var6, int tk6,
									ref int var7, int tk7, ref int var8, int tk8)
			{
				var1 = ReadInt(tk1);	var2 = ReadInt(tk2);
				var3 = ReadInt(tk3); 	var4 = ReadInt(tk4);
				var5 = ReadInt(tk5);	var6 = ReadInt(tk6);
				var7 = ReadInt(tk7);	var8 = ReadInt(tk8);
			}

			public void ReadInts(	ref int var1, int tk1, ref int var2, int tk2, ref int var3, int tk3, 
									ref int var4, int tk4, ref int var5, int tk5, ref int var6, int tk6,
									ref int var7, int tk7, ref int var8, int tk8, ref int var9, int tk9)
			{
				var1 = ReadInt(tk1);	var2 = ReadInt(tk2);
				var3 = ReadInt(tk3); 	var4 = ReadInt(tk4);
				var5 = ReadInt(tk5);	var6 = ReadInt(tk6);
				var7 = ReadInt(tk7);	var8 = ReadInt(tk8);
				var9 = ReadInt(tk9);	
			}

			public void ReadInts(	ref int var1, int tk1, ref int var2, int tk2, ref int var3, int tk3, 
									ref int var4, int tk4, ref int var5, int tk5, ref int var6, int tk6,
									ref int var7, int tk7, ref int var8, int tk8, ref int var9, int tk9, 
									ref int var10, int tk10)
			{
				var1 = ReadInt(tk1);	var2 = ReadInt(tk2);
				var3 = ReadInt(tk3); 	var4 = ReadInt(tk4);
				var5 = ReadInt(tk5);	var6 = ReadInt(tk6);
				var7 = ReadInt(tk7);	var8 = ReadInt(tk8);
				var9 = ReadInt(tk9);	var10 = ReadInt(tk10);	
			}

			//
			//	READ STRING
			//

			public void ReadStrings(ref string var1, int tk1)
			{
				var1 = ReadString(tk1);
			}

			public void ReadStrings(ref string var1, int tk1, ref string var2, int tk2)
			{
				var1 = ReadString(tk1);
				var2 = ReadString(tk2);
			}

			public void ReadStrings(ref string var1, int tk1, ref string var2, int tk2, ref string var3, int tk3)
			{
				var1 = ReadString(tk1);
				var2 = ReadString(tk2);
				var3 = ReadString(tk3);
			}

			public void ReadStrings(ref string var1, int tk1, ref string var2, int tk2, ref string var3, int tk3, 
									ref string var4, int tk4)
			{
				var1 = ReadString(tk1);	var2 = ReadString(tk2);
				var3 = ReadString(tk3);	var4 = ReadString(tk4);
			}

			public void ReadStrings(ref string var1, int tk1, ref string var2, int tk2, ref string var3, int tk3, 
									ref string var4, int tk4, ref string var5, int tk5)
			{
				var1 = ReadString(tk1);	var2 = ReadString(tk2);
				var3 = ReadString(tk3);	var4 = ReadString(tk4);
				var5 = ReadString(tk5);
			}

			public void ReadStrings(ref string var1, int tk1, ref string var2, int tk2, ref string var3, int tk3, 
									ref string var4, int tk4, ref string var5, int tk5, ref string var6, int tk6)
			{
				var1 = ReadString(tk1);	var2 = ReadString(tk2);
				var3 = ReadString(tk3);	var4 = ReadString(tk4);
				var5 = ReadString(tk5);	var6 = ReadString(tk6);
			}

			public void ReadStrings(ref string var1, int tk1, ref string var2, int tk2, ref string var3, int tk3, 
									ref string var4, int tk4, ref string var5, int tk5, ref string var6, int tk6,
									ref string var7, int tk7)
			{
				var1 = ReadString(tk1);	var2 = ReadString(tk2);
				var3 = ReadString(tk3);	var4 = ReadString(tk4);
				var5 = ReadString(tk5);	var6 = ReadString(tk6);
				var7 = ReadString(tk7);
			}

			public void ReadStrings(ref string var1, int tk1, ref string var2, int tk2, ref string var3, int tk3, 
									ref string var4, int tk4, ref string var5, int tk5, ref string var6, int tk6,
									ref string var7, int tk7, ref string var8, int tk8)
			{
				var1 = ReadString(tk1);	var2 = ReadString(tk2);
				var3 = ReadString(tk3);	var4 = ReadString(tk4);
				var5 = ReadString(tk5);	var6 = ReadString(tk6);
				var7 = ReadString(tk7);	var8 = ReadString(tk8);
			}

			public void ReadStrings(ref string var1, int tk1, ref string var2, int tk2, ref string var3, int tk3, 
									ref string var4, int tk4, ref string var5, int tk5, ref string var6, int tk6,
									ref string var7, int tk7, ref string var8, int tk8, ref string var9, int tk9)
			{
				var1 = ReadString(tk1);	var2 = ReadString(tk2);
				var3 = ReadString(tk3);	var4 = ReadString(tk4);
				var5 = ReadString(tk5);	var6 = ReadString(tk6);
				var7 = ReadString(tk7);	var8 = ReadString(tk8);
				var9 = ReadString(tk9);	
			}

			public void ReadStrings(ref string var1, int tk1, ref string var2, int tk2, ref string var3, int tk3, 
									ref string var4, int tk4, ref string var5, int tk5, ref string var6, int tk6,
									ref string var7, int tk7, ref string var8, int tk8, ref string var9, int tk9, 
									ref string var10, int tk10)
			{
				var1 = ReadString(tk1);	var2 = ReadString(tk2);
				var3 = ReadString(tk3);	var4 = ReadString(tk4);
				var5 = ReadString(tk5);	var6 = ReadString(tk6);
				var7 = ReadString(tk7);	var8 = ReadString(tk8);
				var9 = ReadString(tk9);	var10 = ReadString(tk10);
			}
		}

		public class TSVTokenInfo
		{
			public string source;

			// current line, current token
			public int lineIndex;
			public int tokenIndex;
			
			// character index in source
			public int stIdx;
			public int edIdx;
			
			// override public string ToString()
			// {
			// 	return string.Format($"{lineIndex} | {tokenIndex} | <{ReadString()}>");
			// }
		}

		public static IEnumerable<TSVLineInfo> Tokenize(string tsv)
		{
			return Tokenize(tsv, 0, tsv.Length, '\t');
		}
		
		

		
		public static T ParseTSVClass<T>(FieldInfo[] fields, TSVLineInfo info) where T: class, new ()
		{
			var result = new T();

			for (var i = 0;i < fields.Length; i++)
			{
				FieldInfo field = fields[i];
				if (field == null) continue;
				field.SetValue(result, info.Read(i, field.FieldType));
			}
			
			return result;
		}

		public static FieldInfo[] GetFieldInfos<T>(params string[] fieldNames) where T: class
		{
			var typeT = typeof(T);
			var result = new List<FieldInfo>();

			for (var i = 0;i < fieldNames.Length; i++)
			{
				var info = typeT.GetField(fieldNames[i]);
				result.Add(info);
			}

			return result.ToArray();
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem("T70/Dev/Test TSV Parse")]
		public static void TestSelectedTSV()
		{
			var s = UnityEditor.Selection.activeObject;
			if (s == null) return;

			var isTextAsset = s is TextAsset;
			if (isTextAsset == false) return;

			var tsv = (s as TextAsset).text;
			var sb = new StringBuilder();

			foreach (var line in Tokenize(tsv, 0, tsv.Length, '\t'))
			{
				sb.Clear();
				sb.AppendLine($"line={line.lineIndex} nTokens={line.nTokens}");
				
				for (var i = 0;i < line.nTokens; i++)
				{
					sb.AppendLine($"Token {i+1}: <{line.ReadString(i, false, null, false)}>");
				}
				
				Debug.Log(sb.ToString());
			}
		}
#endif
		
		public static IEnumerable<TSVLineInfo> Tokenize(string tsv, int st, int ed, char separator)
		{
			if (string.IsNullOrEmpty(tsv))
			{
				Debug.LogWarning($"Invalid TSV - should not be null or empty!");
				yield break;
			}
			
			var lineInfo = new TSVLineInfo()
			{
				source = tsv,
				tokens = new List<Token>()
			};
			
			var token = lineInfo.GetTokenAt(0);
			token.stCharIdx = st;
			
			var tokenIndex = 0;
			
			// if (tsv.Length <= st || tsv.Length <= ed)
			// {
			// 	Debug.LogWarning($"Invalid index: {st} : {ed} : length={tsv.Length}");
			// 	yield break;
			// }
			
			var length = Mathf.Min(ed + 1, tsv.Length);
			
			for (var i = st; i < length; i++)
			{
				var c = tsv[i];
				var isTab = c == separator;
				var isEOF = i == length - 1;
				var isEOL = c == CR || c == LF;

				var endToken = isTab || isEOF || isEOL;
				if (false == endToken) continue;
				
				// add & finish current token
				token.edCharIdx = i;

				if (isTab) // tab found: continue to next token
				{
					tokenIndex++;
					token = lineInfo.GetTokenAt(tokenIndex);
					token.stCharIdx = i + 1;

					if (isEOF) //special case: tab is the last character
					{
						token.edCharIdx = i + 1;

						// trigger callback for this last line
						lineInfo.nTokens = tokenIndex + 1;
						lineInfo.edCharIdx = i;
						yield return lineInfo;

						// all end!
						yield break;
					}

					continue;
				}
				
				// found line: trigger callback
				lineInfo.nTokens = tokenIndex + 1;
				lineInfo.edCharIdx = i;
				yield return lineInfo;

				if (isEOF) yield break; // no more line

				// Skip through empty lines
				while (i < length - 1)
				{
					c = tsv[i + 1];
					if (c != CR && c != LF) break;
					i++;
					continue;
				}

				// no more lines
				if (i == length) yield break;
				
				// Prepare next line + token
				lineInfo.stCharIdx = i + 1;
				lineInfo.lineIndex++;

				tokenIndex = 0;
				token = lineInfo.GetTokenAt(0);
				token.stCharIdx = i + 1;
			}
		}

		

		public static List<T> Parse<T>(string tsv) where T: class, new ()
		{
			var result = new List<T>();
			Type typeT = typeof(T);
			
			var isTSVLineSupport = typeof(TSV.ITSVLine).IsAssignableFrom(typeT);
			if (isTSVLineSupport)
			{
				foreach (var line in TSV.Tokenize(tsv))
				{
					if (line.lineIndex == 0) continue; //skip 1st line

					var item = new T();
					(item as TSV.ITSVLine).FromTSVLine(line);
					result.Add(item);
				}
				return result;
			}
			
			FieldInfo[] fields = null;

			foreach (var line in TSV.Tokenize(tsv))
			{
				if (line.lineIndex == 0)
				{	
					// parse fields
					var list = new List<FieldInfo>();
					for (int i = 0;i< line.nTokens;i ++)
					{
						var field = typeT.GetField(line.ReadString(i), BindingFlags.Public | BindingFlags.Instance);
						list.Add(field);
					}
					
					fields = list.ToArray();
					continue;
				}
				
				var lineData = ParseTSVClass<T>(fields, line);
				result.Add(lineData);

				// Debug.Log($"{line.lineIndex}:\n{JsonUtility.ToJson(lineData)}");
				// line.LogTokens();
				// if (line.lineIndex > 10) break;
			}

			return result;
		}


		// public static IEnumerable<TSVTokenInfo> Tokenize(string tsv)
		// {
		// 	var info = new TSVTokenInfo()
		// 	{
		// 		source = tsv
		// 	};

		// 	var length = tsv.Length;

		// 	for (int i = 0; i < length; i++)
		// 	{
		// 		var c = tsv[i];

		// 		var isTab = c == TAB;
		// 		var isEOF = i == length - 1;
		// 		var isEOL = c == CR || c == LF;
				
		// 		if (isTab || isEOL || isEOF)
		// 		{
		// 			info.edIdx = isEOF ? length : i;
		// 			info.tokenIndex++;
					
		// 			if (isEOL)
		// 			{
		// 				while (i < length) // skip through empty lines
		// 				{
		// 					c = tsv[i+1];
		// 					if (c != CR && c != LF) break;

		// 					i++;
		// 					continue;
		// 				}
		// 			}
					
		// 			// the last testing character is not EOL
		// 			yield return info;
					
		// 			info.stIdx = i+1;
		// 			if (isEOL)
		// 			{
		// 				info.tokenIndex = 0;
		// 				info.lineIndex ++;
		// 			}
		// 		}
		// 	}
		// }

		public enum EnumTSV { TSV1, TSV2, TSV3 }

		[Serializable] public class TSVData
		{
			public float floatValue;
			public int intValue;
			public string stringValue;

			public Vector2 vector2Value;
			public Vector3 vector3Value;
			public Vector4 vector4Value;
			public EnumTSV enumValue;

			public void Read(TSVLineInfo line)
			{
				floatValue 		= line.ReadFloat(0, 0);
				intValue 		= line.ReadInt(1, 0);
				stringValue 	= line.ReadString(2, false, "default-string-value");

				vector2Value 	= line.ReadVector2(3);
				// vector3Value 	= line.ReadVector3(4);
				// vector4Value 	= line.ReadVector4(5);
				enumValue 		= line.ReadEnum<EnumTSV>(4);
			}

			public string ToTSV()
			{
				return $"{floatValue}\t{intValue}\t{stringValue}\t{vector2Value.x},{vector2Value.y}\t{enumValue}";
			}
		}
	}
}