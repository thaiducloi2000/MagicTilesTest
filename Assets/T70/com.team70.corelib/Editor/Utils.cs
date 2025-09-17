using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace com.team70
{
	public class Utils
	{
		public static List<Component> DropZone(Rect rect, Type typeT, bool forceAdd = true)
		{
			EventType eventType = Event.current.type;

			if (!rect.Contains(Event.current.mousePosition))
			{
				return null;
			}

			bool isAccepted = false;
			if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

				if (eventType == EventType.DragPerform)
				{
					DragAndDrop.AcceptDrag();
					isAccepted = true;
				}
				Event.current.Use();
			}

			if (!isAccepted) return null;
			var data = DragAndDrop.objectReferences;
			var result = new List<Component>();

			foreach (var item in data)
			{
				GameObject go = null;

				if (item is Component) go = ((Component)item).gameObject;
				if (item is GameObject) go = (GameObject)item;

				if (go == null)
				{
					Debug.LogWarning("Unsupported: " + typeT + " <-- " + item);
					continue;
				}

				var c = go.GetComponent(typeT);
				if (c != null)
				{
					result.Add(c);
				}
				else if (forceAdd)
				{
					result.Add(go.GetComponent<Transform>());
				}
			}

			return result;
		}
	}
}
