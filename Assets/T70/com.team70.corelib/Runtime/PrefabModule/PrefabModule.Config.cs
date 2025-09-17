using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace com.team70
{
	public partial class PrefabModule
	{
		public enum ConfigKType
		{
			Boolean,
			Float,
			Integer,
			String
		}

		[Serializable] public class ConfigKV
		{
			public string key;
			public string value;
			public ConfigKType type;

			public object GetData()
			{
				switch (type)
				{
					case ConfigKType.Boolean: return value != string.Empty;
					case ConfigKType.Float:   return float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var floatResult) ? floatResult : 0;
					case ConfigKType.Integer: return int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var intResult) ? intResult : 0;
					case ConfigKType.String:  return value;
					default:
					{
						Debug.LogWarning("Unsupported data type: " + type);
						return null;
					}
				}
			}

	#if UNITY_EDITOR
			bool isValidInput = true;
			public void DrawInspector(PrefabModule pm, Rect rect)
			{
				var r1 = rect;
				r1.width = rect.width / 2f - 20f;
				GUI.Label(r1, key);

				var r2 = r1;
				r2.x += r1.width;

				if (type == ConfigKType.Boolean)
				{
					var b1 = value != string.Empty;
					var b2 = GUI.Toggle(r2, b1, GUIContent.none);
					if (b1 != b2)
					{
						value = b2 ? "1" : string.Empty;
						EditorUtility.SetDirty(pm);
					}
					return;
				}

				var value2 = EditorGUI.DelayedTextField(r2, value);
				if (value != value2)
				{
					if (type == ConfigKType.String)
					{
						isValidInput = true;
						value = value2;
						EditorUtility.SetDirty(pm);
						return;
					}

					if (type == ConfigKType.Float)
					{
						isValidInput = float.TryParse(value2, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _);
						if (isValidInput)
						{
							value = value2;
							EditorUtility.SetDirty(pm);
						}
						return;
					}

					if (type == ConfigKType.Integer)
					{
						isValidInput = int.TryParse(value2, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out _);
						if (!isValidInput) return;

						value = value2;
						EditorUtility.SetDirty(pm);
					}
				}
			}
	#endif
		}
		
		public List<ConfigKV> config;

		[NonSerialized] private Dictionary<string, object> cacheConfig;
		public Dictionary<string, object> GetConfig()
		{
			if (cacheConfig != null) return cacheConfig;

			cacheConfig = new Dictionary<string, object>();
			for (var i = 0; i < config.Count; i++)
			{
				var item = config[i];
				cacheConfig.Add(item.key, item.GetData());
			}

			return cacheConfig;
		}

		public T GetConfig<T>(string key, T defaultValue)
		{
			var cfg = GetConfig();
			if (cfg.TryGetValue(key, out object result) == false) return defaultValue;

			if (result.GetType() == typeof(T)) return (T)result;
			return defaultValue;
		}

		public void OverwriteConfig(params object[] keyValues)
		{
			GetConfig();
			for (var i = 0; i < keyValues.Length; i += 2)
			{
				var key = (string)keyValues[i];
				var val = keyValues[i + 1];

				if (cacheConfig.ContainsKey(key))
				{
					cacheConfig[key] = val;
				}
				else
				{
					cacheConfig.Add(key, val);
				}
			}
		}
	}
}
