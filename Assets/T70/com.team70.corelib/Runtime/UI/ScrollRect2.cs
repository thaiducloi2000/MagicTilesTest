using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRect2 : ScrollRect
{
	public ScrollRect parent;
	bool willForwardToParent;

	public override void OnBeginDrag(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left)
			return;
            
		if (!IsActive())
			return;
		
		Vector2 pointerDelta = eventData.delta;
		
		if (horizontal == false)
		{
			if (Mathf.Abs(pointerDelta.x) > Mathf.Abs(pointerDelta.y))
			{
				if (parent != null)
				{
					willForwardToParent = true;
					parent.OnBeginDrag(eventData);
				}
				return;
			}
		}

		if (vertical == false)
		{
			if (Mathf.Abs(pointerDelta.x) < Mathf.Abs(pointerDelta.y))
			{
				if (parent != null)
				{
					willForwardToParent = true;
					parent.OnBeginDrag(eventData);
				}
				return;
			}
		}
		
		base.OnBeginDrag(eventData);
	}
	
	public override void OnEndDrag(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left)
			return;
            
		if (willForwardToParent)
		{
			parent.OnEndDrag(eventData);
			willForwardToParent = false;
			return;
		}
		
		base.OnEndDrag(eventData);
	}

	public override void OnDrag(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left)
			return;

		if (!IsActive())
			return;
		
		if (willForwardToParent)
		{
			parent.OnDrag(eventData);
			return;
		}
		
		base.OnDrag(eventData);
	}
}
