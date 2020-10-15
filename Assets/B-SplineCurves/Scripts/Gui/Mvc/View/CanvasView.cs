using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Chaye
{
	[RequireComponent(typeof(RectTransform))]
	public class CanvasView : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
	{

		[SerializeField] InputField inputRank = default;
		[SerializeField] Button buttonDelete = default;
		[SerializeField] Button buttonClear = default;
		[SerializeField] Button buttonFinish = default;
		[SerializeField] Toggle toggleShowControlPoint = default;
		[SerializeField] Toggle toggleShowKnotPoint = default;

		Action<Vector2> onPointerDown;
		Action<Vector2> onDrag;
		Action<Vector2> onEndDrag;

		public void OnChangedInputFieldRank(UnityAction<string> callback)
		{
			inputRank.onValueChanged.AddListener(callback);
		}

		public void OnClickButtonClear(UnityAction callback)
		{
			buttonClear.onClick.AddListener(callback);
		}

		public void OnClickButtonDelete(UnityAction callback)
		{
			buttonDelete.onClick.AddListener(callback);
		}

		public void OnClickButtonFinish(UnityAction callback)
		{
			buttonFinish.onClick.AddListener(callback);
		}

		public void OnClickToggleShowControlPoint(UnityAction<bool> callback)
		{
			toggleShowControlPoint.onValueChanged.AddListener(callback);
		}

		public void OnClickToggleShowKnotPoint(UnityAction<bool> callback)
		{
			toggleShowKnotPoint.onValueChanged.AddListener(callback);
		}



		public void OnPointerDown(Action<Vector2> callback)
		{
			onPointerDown = callback;
		}

		public void OnDrag(Action<Vector2> callback)
		{
			onDrag = callback;
		}

		public void OnEndDrag(Action<Vector2> callback)
		{
			onEndDrag = callback;
		}

		void IPointerDownHandler.OnPointerDown(PointerEventData e)
		{
			onPointerDown?.Invoke(e.position);
		}

		void IDragHandler.OnDrag(PointerEventData e)
		{
			onDrag?.Invoke(e.position);
		}

		void IEndDragHandler.OnEndDrag(PointerEventData e)
		{
			onEndDrag?.Invoke(e.position);
		}

		public void ShowPoints(bool isShow)
		{
			toggleShowControlPoint.isOn = isShow;
			toggleShowKnotPoint.isOn = isShow;
		}

	}
}