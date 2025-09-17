using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable] public class SerializableType : ISerializationCallbackReceiver
{
	[NonSerialized] public Type cacheType; // Unity can not serialize Type anyway!
	[SerializeField] public string fullTypeName;
	
	public void OnBeforeSerialize()
	{
		if (cacheType != null) fullTypeName = cacheType.FullName;
	}
	
	public void OnAfterDeserialize()
	{
		if (string.IsNullOrEmpty(fullTypeName)) return;
		cacheType = GetTypeByName(fullTypeName);
	}

	public void RefreshCacheType()
	{
		if (string.IsNullOrEmpty(fullTypeName))
		{
			cacheType = null;
			return;
		}
		
		cacheType = GetTypeByName(fullTypeName);
	}
	
	// ----------------------
	[NonSerialized] private static Dictionary<string, Type> TypeMapCache = new Dictionary<string, Type>();
	static bool typeMapInited = false;

	public static void RegisterCacheTypes(params Type[] types)
	{
		if (typeMapInited) return; // ignore if already scan all types
		foreach (var type in types)
		{
			TypeMapCache.Add(type.FullName, type);
		}
	}
	
	public static Type GetTypeByName(string fullTypeName)
	{
		if (string.IsNullOrEmpty(fullTypeName)) return null;

		fullTypeName = fullTypeName.Trim();
		if (fullTypeName == string.Empty) return null;

		if (TypeMapCache.TryGetValue(fullTypeName, out var result1)) return result1;

		if (typeMapInited == false)
		{
			typeMapInited = true;
			// var typeC = typeof(Component);
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type type in assembly.GetTypes())
				{	
					// if (typeC.IsAssignableFrom(type) == false) continue;
					var fn = type.FullName;
					if (TypeMapCache.ContainsKey(fn)) continue; // same full name??
					TypeMapCache.Add(fn, type);
				}
			}
		}

		TypeMapCache.TryGetValue(fullTypeName, out var result);
		
		#if UNITY_EDITOR
		{
			// Hack to prevent logWarning in Edit mode
			logWarningMessages.Add
			(
				$"[Editor] Please add {fullTypeName} to TypeMapCache to improve performance!\n\nSerializableType.TypeMapCache.Add(\"{fullTypeName}\", typeof({fullTypeName}));\n"
			);
			
			EditorApplication.update -= LogWarningIfPlaying;
			EditorApplication.update += LogWarningIfPlaying;
		}
		#endif
		
		return result;
	}
	
	#if UNITY_EDITOR
	static List<string> logWarningMessages = new List<string>();
	static void LogWarningIfPlaying()
	{
		EditorApplication.update -= LogWarningIfPlaying;
		if (EditorApplication.isPlaying == false) return; // Do not log in Edit Mode
		
		for (var i = 0; i < logWarningMessages.Count; i++)
		{
			Debug.LogWarning(logWarningMessages[i]);
		}
		logWarningMessages.Clear();
	}
	#endif
	
	public static List<T> GetDerrivedType<T>()
	{
		return (List<T>)(object)GetDerrivedType(typeof(T));
	}
	
	public static List<Type> GetDerrivedType(Type parentType)
	{
		var result = new List<Type>();
		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			foreach (Type type in assembly.GetTypes())
			{
				if (parentType.IsAssignableFrom(type) == false) continue;
				result.Add(type);
			}
		}

		return result;
	}
}

