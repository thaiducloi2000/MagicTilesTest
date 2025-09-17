using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace com.team70
{
	public interface IPrefabModuleLocalize
	{
		void OnLocalizeChange();
	}
	
	public class PrefabModuleLocalizeHook
	{
		public virtual void StartWatch(Component c, string id) { }
		public virtual void StopWatch(Component c) { }
		public Action onLocalizeChange;
	}
	
	public partial class PrefabModule
	{
		public static PrefabModuleLocalizeHook localizeHook;
		
		public List<LocInfo> localize;

		[Serializable] public class LocInfo
		{
			public string locId;
			public Component target;

			public string GetAutoId(Transform root)
			{
				if (target == null) return null;

				var hierarchy = T70.GetParents(target.transform, true, root);
				var hash = new HashSet<string>()
				{
					"ID",
					"PREFAB"
				};

				var sb = new StringBuilder();
				sb.Append("ID_PREFAB_");

				for (var i = 0; i < hierarchy.Count; i++)
				{
					var str = Normalize(hierarchy[i].name, hash);
					if (string.IsNullOrEmpty(str)) continue;

					if (i > 0) sb.Append("_");
					sb.Append(str);
				}

				return sb.ToString();
			}

			public static List<LocInfo> Scan<T>(Transform go, List<LocInfo> reuse = null, Func<Component, bool> validator = null)
			{
				var children = T70.GetComponentsInChildren<T>(go.transform);
				var cache = T70.ToDictionary(reuse, (item, index) => item.target);
				var result = reuse ?? new List<LocInfo>();

				for (var i = 0; i < children.Count; i++)
				{
					var c = (Component)(object)children[i];
					if (c == null) continue;

					if (cache.TryGetValue(c, out LocInfo _)) continue;
					if (validator != null && (validator(c) == false)) continue;
					result.Add(new LocInfo()
					{
						target = c,
						locId = string.Empty
					});
				}

				return result;
			}

			internal static readonly HashSet<char> NUMERIC_PREFIX = new HashSet<char>()
			{
				'-',
				'+',
				'$',
				'x',
				'X'
			};

			internal static string Normalize(string locId, HashSet<string> elementHash)
			{
				var elms = locId.Replace("-", " ").Replace("_", " ").ToUpper().Split(' ');

				for (int i = 0; i < elms.Length; i++)
				{
					var elm = elms[i];
					if (elementHash.Contains(elm))
					{
						elms[i] = null;
						continue;
					}

					elementHash.Add(elm);
				}

				var sb = new StringBuilder();
				for (var i = 0; i < elms.Length; i++)
				{
					var elm = elms[i];
					if (elm == null) continue;
					sb.Append(elms[i]);
				}

				return sb.ToString();
			}

			internal static bool ShouldLocalized(string text)
			{
				if (text == null) return false;

				text = text.Trim();

				// ignore place holder texts
				if (text == string.Empty || text == "-") return false;

				// ignore number texts
				if (float.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _)) return false;

				var c0 = text[0];
				if (!NUMERIC_PREFIX.Contains(c0)) return true;

				return !float.TryParse(text.Substring(1, text.Length - 1), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _);
			}
		}

		void Localize_OnEnable()
		{
			if (localizeHook == null) return;

			// Refresh static texts (LocID never changes)
			RefreshLocalize();

			if (logic == null) return;
			if (false == (logic is IPrefabModuleLocalize)) return;
			
			// Support for IPrefabModuleLocalize
			var pmLocalize = (IPrefabModuleLocalize)logic;
			if (pmLocalize == null) return;
			
			localizeHook.onLocalizeChange -= pmLocalize.OnLocalizeChange;
			localizeHook.onLocalizeChange += pmLocalize.OnLocalizeChange;
			pmLocalize.OnLocalizeChange();
		}
		
		void Localize_OnDisable()
		{
			if (localizeHook == null) return;

			for (var i = 0; i < localize.Count; i++)
			{
				LocInfo item = localize[i];
				if (item.target == null) continue;
				localizeHook.StopWatch(item.target);
			}

			if (logic == null) return;
			if ((logic is IPrefabModuleLocalize) == false) return;

			var pmLocalize = (IPrefabModuleLocalize)logic;
			if (pmLocalize != null)
			{
				localizeHook.onLocalizeChange -= pmLocalize.OnLocalizeChange;
			}
		}
		
		public void RefreshLocalize()
		{
			for (var i = 0; i < localize.Count; i++)
			{
				var item = localize[i];
				if (item.target == null) continue;
				
				localizeHook.StartWatch(item.target, item.locId);
			}
		}
		
		#if UNITY_EDITOR
		[ContextMenu("Scan Children 4 Localize")]
		public void ScanLocalizeChildren()
		{
			localize = LocInfo.Scan<Text>(transform, localize, IsValidTextComponent);
			SetAutoLocID();
			EditorUtility.SetDirty(this);
		}

		[ContextMenu("Auto LocID")]
		public void SetAutoLocID()
		{
			foreach (var item in localize)
			{
				if (string.IsNullOrEmpty(item.locId))
				{
					item.locId = item.GetAutoId(transform);
				}
			}

			EditorUtility.SetDirty(this);
		}

		public bool IsValidTextComponent(Component c)
		{
			if (c == null) return false;
			var typeC = c.GetType();
			return typeC == typeof(Text) && LocInfo.ShouldLocalized(((Text)c).text);
		}
	#endif
	}
}
