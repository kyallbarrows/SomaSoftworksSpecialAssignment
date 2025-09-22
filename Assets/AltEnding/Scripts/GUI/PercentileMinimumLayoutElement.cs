using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PercentileMinimumLayoutElement : UIBehaviour
{
    [SerializeField] private LayoutElement myLayoutElement;
    [SerializeField] private RectTransform targetRectTransform;
    [SerializeField, Min(0f)] private float widthFactor;

#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		OnRectTransformDimensionsChange();
	}
#endif

	[ContextMenu("Refresh Layout Element")]
	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		myLayoutElement.minWidth = targetRectTransform.rect.size.x * widthFactor;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (myLayoutElement == null || targetRectTransform == null) { enabled = false; return; }

		OnRectTransformDimensionsChange();
	}

	protected override void Start()
	{
		base.Start();
		OnRectTransformDimensionsChange();
	}
}
