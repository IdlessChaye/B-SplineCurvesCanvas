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
		[SerializeField] Toggle toggleIsUniform = default;
		[SerializeField] InputField inputRank = default;
		[SerializeField] Button buttonDelete = default;
		[SerializeField] Button buttonClear = default;
		[SerializeField] Button buttonFinish = default;
		[SerializeField] Toggle toggleShowControlPoint = default;
		[SerializeField] Toggle toggleShowKnotPoint = default;
		[SerializeField] Button buttonColorPanel = default;
		[SerializeField] GameObject colorPanel = default;

		Action<Vector2> onPointerDown;
		Action<Vector2> onDrag;
		Action<Vector2> onEndDrag;

		bool _isShowControlPoints = true;
		bool _isShowKnotPoints = true;

		bool _isShowColorPanel = false;

		ColorManager _colorManager;


		private void Start()
		{
			_colorManager = colorPanel.GetComponent<ColorManager>();

			Utilities.TrySetActive(colorPanel, _isShowColorPanel);
			buttonColorPanel.onClick.AddListener(() =>
			{
				_isShowColorPanel = !_isShowColorPanel;
				Utilities.TrySetActive(colorPanel, _isShowColorPanel);
			});
		}

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

		public void OnClickToggleIsUniform(UnityAction<bool> callback)
		{
			toggleIsUniform.onValueChanged.AddListener(callback);
		}

		public void OnColorChanged(UnityAction<Color> callback)
		{
			_colorManager.OnColorChanged(callback);
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

		public void UpdatePoints()
		{
			toggleShowControlPoint.isOn = _isShowControlPoints;
			toggleShowControlPoint.onValueChanged.Invoke(_isShowControlPoints);
			toggleShowKnotPoint.isOn = _isShowKnotPoints;
			toggleShowKnotPoint.onValueChanged.Invoke(_isShowKnotPoints);
		}

		public void SetShowControlPoints(bool isShow)
		{
			_isShowControlPoints = isShow;
		}

		public void SetShowKnotPoints(bool isShow)
		{
			_isShowKnotPoints = isShow;
		}

	}
}