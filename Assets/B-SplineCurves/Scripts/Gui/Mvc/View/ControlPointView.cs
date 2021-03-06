﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Chaye
{
	public class ControlPointView : MonoBehaviour, IPointerDownHandler, IDragHandler
	{
		[HideInInspector]
		public Guid Id;

		[SerializeField]
		private Image _image = default;

		private RectTransform rectTransform;
		private Action<Vector2> onDrag;
		private Action onPointerDown;

		void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
		}

		public void SetSelected(bool isSelected)
		{
			var color = isSelected ? Color.red : Color.blue;
			_image.color = color;
		}

		public void UpdateControlPoint(ControlPoint point)
		{
			rectTransform.anchoredPosition3D = point.Anchor;
		}

		public void OnPointerDown(Action<Guid> callback)
		{
			onPointerDown = () => callback(Id);
		}

		public void OnDrag(Action<Guid, Vector2> callback)
		{
			onDrag = position => callback(Id, position);
		}

		void IPointerDownHandler.OnPointerDown(PointerEventData e)
		{
			e.Use();
			onPointerDown?.Invoke();
		}

		void IDragHandler.OnDrag(PointerEventData e)
		{
			e.Use();
			onDrag?.Invoke(e.position);
		}

	}
}