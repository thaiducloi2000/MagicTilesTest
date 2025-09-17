// #define VISUAL_STATE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// TODO: 
//
// [ ] Support Editor mode
// [ ] Support Always ready mode (does not need to require / wait for TSV parse)

[CreateAssetMenu(fileName = "localizev2.asset", menuName = "localizev2", order = 100)]
public partial class LocalizeV2 : ScriptableObject
{
	private const string NUMERIC_TOKEN = "{N}";
	private const string GUI_ID_STRING_PREFIX = "GUI_ID_STRING_";
	private const string PARAM_SUFFIX = "_PARAM";
	private const string PLAYER_PREF_KEY = "LocalizeV2.langCode";

	// 
	//	STATIC
	//
	[NonSerialized] private static LocalizeV2 _api;
	[NonSerialized] private static bool _inited;
	[NonSerialized] private static bool _isPlayMode;

	[NonSerialized] public static Action OnChange;
	[NonSerialized] private static readonly Dictionary<Text, string> _monitorDict = new Dictionary<Text, string>();

	public static string[] GetAllLocIds()
	{
		return IsReady() ? _api._cache.Keys.ToArray() : null;
	}
	public static List<string> GetAvailableLangCodes()
	{
		return IsReady() ? _api.langs : null;
	}

	public static string currentLangCode
	{
		get
		{
			return IsReady() ? _api.langs[_api._langIndex] : null;
		}

		set
		{
			SetLangCode(value);
		}
	}

	public static bool SetLangCode(string langCode, bool save = true)
	{
		if (!IsReady()) return false;
		var idx = _api.langs.IndexOf(langCode);
		if (idx == -1) return false;

		_api._langIndex = idx;
		// Debug.Log($"Set Lang: {langCode} ==> idx={idx}");

		if (_isPlayMode && save)
		{
			PlayerPrefs.SetString(PLAYER_PREF_KEY, langCode);
			PlayerPrefs.Save();
		}

		// Trigger changes
		DispatchOnChange();
		return true;
	}

	public static string Get(string locId, params object[] vars)
	{
		var convertArray = vars.Length == 1 && vars[0] is Array;
		var stringParams = ExtractCovariant(convertArray ? vars[0] as Array : vars);
		return !IsReady() ? null : _api.iGet(locId, stringParams);
	}

	// TODO : Cache Key->LocId 
	public static string CreateLocId(string key, bool hasParams)
	{
		if (Violate(string.IsNullOrEmpty(key), $"Can not create LocId from null or empty string {key}")) return null;

		var prefix = key.StartsWith(GUI_ID_STRING_PREFIX, StringComparison.Ordinal) ? string.Empty : GUI_ID_STRING_PREFIX;
		var suffix = hasParams ? PARAM_SUFFIX : String.Empty;
		var body = key.Replace(".", "_").ToUpper();
		return $"{prefix}{body}{suffix}";
	}

	public static string GetLocalizedString(string key, params object[] vars)
	{
		var locId = CreateLocId(key, vars.Length > 0);
		return Get(locId, vars);
	}

	[Obsolete("Use GetLocalizedString() instead")]
	public static string GetCustom(string key, string fallbackValue = null, params object[] vars)
	{
		var result = GetLocalizedString(key, vars);
		return result ?? fallbackValue;
	}

	public static string GetWithFallback(string key, string fallbackValue = null, params object[] vars)
	{
		var result = Get(key, vars);
		return result ?? fallbackValue;
	}

	public static void StartMonitor(Text lb, string locId)
	{
		if (Application.isPlaying == false) return;

		if (Violate(string.IsNullOrEmpty(locId), $"Can not monitor a null locId=<{locId}> (Text={lb})")) return;
		if (Violate(lb == null, $"Can not monitor a null Text (locId=<{locId}>)")) return;
		if (Violate(_api != null && _api.iGetElement(locId) == null, $"LocId=<{locId}> not found!")) return;

		AddOrSet(_monitorDict, lb, locId);
		lb.text = Get(locId);
	}

	public static void StopMonitor(Text lb)
	{
		if (Application.isPlaying == false) return;

		if (lb == null) return;
		if (!_monitorDict.ContainsKey(lb)) return;
		_monitorDict.Remove(lb);
	}

	static void DispatchOnChange()
	{
		if (Application.isPlaying == false) return;

		foreach (var kvp in _monitorDict)
		{
			if (kvp.Key == null) continue;
			kvp.Key.text = Get(kvp.Value);
		}
		OnChange?.Invoke();
	}

	public static void EditModeInit(bool force)
	{
		#if UNITY_EDITOR
		if (Application.isPlaying) return;

		// Debug.LogWarning("Edit Mode Init: " + Application.isPlaying);

		if (_api != null) return;
		if (_inited && !force) return;

		var guids = AssetDatabase.FindAssets("t:LocalizeV2");
		var nAsset = guids.Length;

		if (Violate(nAsset == 0, "LocalizeV2 asset not found!"))
		{
			_inited = true; // stop searching
			return;
		}

		Violate(nAsset > 1, $"Multiple ({nAsset}) instances of LocalizeV2 asset found!");
		var asset = AssetDatabase.LoadAssetAtPath<LocalizeV2>(AssetDatabase.GUIDToAssetPath(guids[0]));
		asset.Init();
		#else
		Debug.LogWarning("NOT IN EDITOR: " + Application.isPlaying);
		#endif
	}

	// 
	//	INSTANCE
	//
	[NonSerialized] private int _langIndex = -1;
	public List<string> langs = new List<string>();
	public List<Element> data = new List<Element>();
	[NonSerialized] private readonly Dictionary<string, Element> _cache = new Dictionary<string, Element>();

	public void Init()
	{
		if (_inited) return;
		_inited = true;
		_api = this;
		_isPlayMode = Application.isPlaying;
		iBuildCache();

		if (!_isPlayMode) return;
		iRestoreLanguageSettings();
	}

	void iRestoreLanguageSettings()
	{
		var savedLang = PlayerPrefs.GetString(PLAYER_PREF_KEY, string.Empty);
		var willSave = false;

		if (string.IsNullOrEmpty(savedLang)) // set default language
		{
			savedLang = GetLangCode(Application.systemLanguage, langs[0]);
			willSave = true;
		}

		SetLangCode(savedLang, willSave);
	}

	[ContextMenu("Rebuild Cache")]
	void iBuildCache()
	{
		_cache.Clear();

		for (var i = data.Count - 1; i >= 0; i--)
		{
			Element elm = data[i];
			elm.locID = elm.locID.Trim();

			if (Violate(
				_cache.ContainsKey(elm.locID),
				$"Duplicated LocID <{elm.locID}> removed!")
			)
			{
				#if UNITY_EDITOR
				data.RemoveAt(i);
				#endif
				return;
			}

			_cache.Add(elm.locID, elm);
		}
	}

	public Element iGetElement(string locId)
	{
		if (Violate(string.IsNullOrEmpty(locId), $"Invalid locId=<{locId}> (should not be null or empty)"))
		{
			return null;
		}

		return _cache.TryGetValue(locId, out Element result) ? result : null;
	}

	internal string iGet(string locId, string[] vars)
	{
		Element elm = iGetElement(locId);
		if (elm == null) return null;
		
		try
		{
			var isValidLangIndex = (_langIndex >=0) && (_langIndex < elm.locValues.Count);
			var v = elm.locValues[isValidLangIndex ? _langIndex : 0];
			if (vars.Length == 0) return v;
			
			// TODO: Support variable based token
			var sb = new StringBuilder();
			var arr = v.Split(new[]{NUMERIC_TOKEN}, StringSplitOptions.None);
			Violate(arr.Length != (vars.Length + 1), $"Unmatched number of params in locId={locId}: nParams={vars.Length}\nLocValue={v}", true);

			var nParams = vars.Length;
			for (var i = 0; i < arr.Length - 1; i++)
			{
				sb.Append(arr[i]);
				sb.Append(i < nParams ? vars[i] : "???");
			}
			sb.Append(arr[arr.Length - 1]);
			return sb.ToString();
		}
		catch (Exception e)
		{
			Debug.LogWarning(e);
		}

		return $"ERROR : {locId}";
	}

	public void iImportTSVLines(string[] lines)
	{
		var dirty = false;

		for (var i = 0; i < lines.Length; i++)
		{
			var line = lines[i].Trim();
			if (string.IsNullOrEmpty(line)) continue;

			var isDelete = line.StartsWith("-", StringComparison.Ordinal);

			if (isDelete)
			{
				#if UNITY_EDITOR
				{
					Element delElement = iGetElement(ExtractLocId(line));
					if (delElement == null) continue;

					var idx = data.IndexOf(delElement);
					if (idx == -1) continue;

					_cache.Remove(delElement.locID);
					data[idx] = null;

					dirty = true;
					LogWarning($"Deleted: {delElement.locID}", true);
				}
				#endif

				continue;
			}

			var locId = ExtractLocId(line);
			Element elm = iGetElement(locId);

			if (elm == null)
			{
				elm = new Element() { locID = locId, enable = true };
				data.Add(elm);
				_cache.Add(locId, elm);
			}

			elm.FromTSV(line, true, langs); // force update!
		}

		if (!dirty) return;

		// Clean up!
		var nullCount = 0;
		for (var i = 0; i < data.Count; i++)
		{
			Element elm = data[i];
			if (elm == null)
			{
				nullCount++;
				continue;
			}

			if (nullCount == 0) continue;
			data[i - nullCount] = data[i];
		}

		data.RemoveRange(data.Count - 1 - nullCount, nullCount);
	}

	// 
	//	UTILS
	//
	static bool IsReady()
	{
		if (_api != null) return true;
		if (_inited) return false;

		#if UNITY_EDITOR
		{
			EditModeInit(false);
			return _api != null;
		}
		#else
		return false;
		#endif
	}

	private static readonly Dictionary<SystemLanguage, string> systemLanguageMap = new Dictionary<SystemLanguage, string>()
	{
		{SystemLanguage.Afrikaans, "af"},
		{SystemLanguage.Arabic, "ar"},
		{SystemLanguage.Basque, "eu"},
		{SystemLanguage.Belarusian, "be"},
		{SystemLanguage.Bulgarian, "bg"},
		{SystemLanguage.Catalan, "ca"},
		{SystemLanguage.Chinese, "zh-cn"}, // FORCE TO USE SIMPLIFIED CHINESE
		{SystemLanguage.Czech, "cs"},
		{SystemLanguage.Danish, "da"},
		{SystemLanguage.Dutch, "nl"},
		{SystemLanguage.English, "en" },
		{SystemLanguage.Estonian, "et"},
		{SystemLanguage.Faroese, "fo"},
		{SystemLanguage.Finnish, "fi"},
		{SystemLanguage.French, "fr"},
		{SystemLanguage.German, "de"},
		{SystemLanguage.Greek, "el"},
		{SystemLanguage.Hebrew, "he"},
		{SystemLanguage.Hungarian, "hu"},
		{SystemLanguage.Icelandic, "is"},
		{SystemLanguage.Indonesian, "id"},
		{SystemLanguage.Italian, "it"},
		{SystemLanguage.Japanese, "ja"},
		{SystemLanguage.Korean, "ko"},
		{SystemLanguage.Latvian, "lv"},
		{SystemLanguage.Lithuanian, "lt"},
		{SystemLanguage.Norwegian, "no"},
		{SystemLanguage.Polish, "pl"},
		{SystemLanguage.Portuguese, "pt"},
		{SystemLanguage.Romanian, "ro"},
		{SystemLanguage.Russian, "ru"},
		{SystemLanguage.SerboCroatian, "sh"},
		{SystemLanguage.Slovak, "sk"},
		{SystemLanguage.Slovenian, "sl"},
		{SystemLanguage.Spanish, "es"},
		{SystemLanguage.Swedish, "sv"},
		{SystemLanguage.Thai, "th"},
		{SystemLanguage.Turkish, "tr"},
		{SystemLanguage.Ukrainian, "uk"},
		{SystemLanguage.Vietnamese, "vi"},
		{SystemLanguage.ChineseSimplified, "zh-cn"},
		{SystemLanguage.ChineseTraditional, "zh-tw"},
	};

	public static string GetLangCode(SystemLanguage lang, string defaultLang = "en")
	{
		if (lang == SystemLanguage.Unknown) return defaultLang;
		return systemLanguageMap.TryGetValue(lang, out var result) ? result : defaultLang;
	}

	static string ExtractLocId(string line)
	{
		var stIndex = line.StartsWith("-", StringComparison.Ordinal) ? 1 : 0;
		var edIndex = line.IndexOf('\t');
		if (edIndex == -1) edIndex = line.Length;
		return line.Substring(stIndex, edIndex - stIndex).Trim();
	}

	static string[] ExtractCovariant(Array array)
	{
		var result = new string[array.Length];
		for (var i = 0; i < array.Length; i++)
		{
			result[i] = array.GetValue(i).ToString();
		}
		return result;
	}

	static void LogWarning(string message, bool editorOnly)
	{
		if (string.IsNullOrEmpty(message)) return;

		#if UNITY_EDITOR
		{
			var prefix = editorOnly ? "[Editor] " : string.Empty;
			Debug.LogWarning($"{prefix}{message}");
		}
		#else
		{
			if (editorOnly) return;
			Debug.LogWarning(message);
		}
		#endif
	}

	static bool AddOrSet<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey key, TValue val)
	{
		if (dict.ContainsKey(key))
		{
			dict[key] = val;
			return true;
		}

		dict.Add(key, val);
		return false;
	}

	static bool Violate(bool violateCond, string warningMessage, bool editorOnly = true)
	{
		if (!violateCond) return false;
		LogWarning(warningMessage, editorOnly);
		return true;
	}

	public void ClearAll()
	{
		_api._cache.Clear();
		_api.data.Clear();
	}
	
	public void ImportTSV(string[] lines)
	{
		var allowClear = Application.isPlaying == false; // Edit mode
		
		for (var i = 0; i < lines.Length; i++)
		{
			var line = lines[i].Trim();

			if (string.IsNullOrEmpty(line)) continue;

			var isRemove = line.StartsWith("-");
			var idx = line.IndexOf('\t');
			string locId;

			if (isRemove)
			{
				if (allowClear == false) continue;
				
				locId = line.Substring(1, idx == -1 ? line.Length - 1 : idx).Trim();
				if (_cache.ContainsKey(locId))
				{
					_cache.Remove(locId);
					for (var k = data.Count - 1; k >= 0; k--)
					{
						if (data[k].locID != locId) continue;

						data.RemoveAt(k);
						Debug.LogWarning("Deleted: " + locId);
						break;
					}
				}
				
				continue;
			}

			if (idx == -1) continue;
			locId = line.Substring(0, idx).Trim();
			if (string.IsNullOrEmpty(locId)) continue;

			if (!_cache.TryGetValue(locId, out Element e)) // new key
				{
				e = new Element() { locID = locId, enable = true };
				_cache.Add(locId, e);
				data.Add(e);
			}

			e.FromTSV(line, allowClear, langs);
		}
	}
}
