using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Chaye
{

	public class CanvasController : MonoBehaviour
	{
		private static CanvasController _instance;
		public static CanvasController Instance => _instance;

		[SerializeField]
		private CanvasView _canvasView = default;
		[SerializeField]
		private RectTransform _curveContainer = default;
		[SerializeField]
		private GameObject _pathPrefab = default;
		[SerializeField]
		private GameObject _controlPointPrefab = default;
		[SerializeField]
		private GameObject _knotPointPrefab = default;
		[SerializeField]
		private UnityEngine.UI.InputField _input = default;

		private PathModel _model;

		private ControlPointView _currentEditingControlPoint = default;
		private readonly Dictionary<Guid, ControlPointView> _controlPointViews = new Dictionary<Guid, ControlPointView>();

		private KnotPointView _currentEditingKnotPoint = default;
		private readonly Dictionary<Guid, KnotPointView> _knotPointViews = new Dictionary<Guid, KnotPointView>();

		private PathView _currentPathView;
		private Color _currentPathColor = Color.black;

		private void Awake()
		{
			_instance = this;
			BSplineCurves.Segments = 100;
			_model = new PathModel();
			_model.IsAutoAdjustRank = true;
			_currentPathView = InstantiatePathView();
		}

		private void Start()
		{
			_canvasView.OnPointerDown(position =>
			{
				if (Input.GetMouseButton(0))
					AddControlPoint(position);
				else
					DeleteControlPointCallback();
			});

			_canvasView.OnClickToggleIsUniform((isOk) =>
			{
				_model.IsUniform = isOk;
				_model.UpdateKnotVector();
				_canvasView.UpdatePoints();
			});

			_canvasView.OnChangedInputFieldRank((rankStr) =>
			{
				uint rank;
				if (uint.TryParse(rankStr, out rank))
				{
					ChangeRank(rank);
					_canvasView.UpdatePoints();
				}
			});

			_canvasView.OnClickButtonDelete(DeleteControlPointCallback);

			_canvasView.OnClickButtonClear(Clear);

			_canvasView.OnClickButtonFinish(Finish);

			_canvasView.OnClickToggleShowControlPoint((isOk) =>
			{
				_canvasView.SetShowControlPoints(isOk);
				foreach (var view in _controlPointViews.Values)
				{
					Utilities.TrySetActive(view.gameObject, isOk);
				}
			});

			_canvasView.OnClickToggleShowKnotPoint((isOk) =>
			{
				_canvasView.SetShowKnotPoints(isOk);
				foreach (var view in _knotPointViews.Values)
				{
					Utilities.TrySetActive(view.gameObject, isOk);
				}
			});

			_canvasView.OnColorChanged(color =>
			{
				_currentPathColor = color;
				_currentPathView.SetColor(_currentPathColor);
			});
		}

		private void LateUpdate()
		{
			if (_model.IsDirty)
			{
				_model.IsDirty = false;
				UpdatePath();
			}
		}


		#region AddControlPoint

		private void AddControlPoint(Vector3 anchore)
		{
			var id = Guid.NewGuid();
			var view = InstantiateControlPointView(id);
			_controlPointViews.Add(id, view);

			var controlPoint = CreateControlPoint(anchore);
			_model.AddControlPoint(id, controlPoint);
			view.UpdateControlPoint(controlPoint);

			SelectControlPoint(id);
			_canvasView.UpdatePoints();
		}
		
		private ControlPointView InstantiateControlPointView(Guid id)
		{
			var go = Instantiate(_controlPointPrefab, _curveContainer);
			var view = go.GetComponent<ControlPointView>();
			view.Id = id;
			view.OnPointerDown(SelectControlPoint);
			view.OnDrag(DragControlPoint);

			return view;
		}

		private void SelectControlPoint(Guid id)
		{
			var point = _model.GetControlPoint(id);
			if (point == null)
			{
				return;
			}

			foreach (var view in _controlPointViews.Values)
			{
				view.SetSelected(view.Id == id);
			}

			_currentEditingControlPoint = _controlPointViews[id];
		}

		private void DragControlPoint(Guid id, Vector2 position)
		{
			_model.DragControlPoint(id, position);
			var controlPoint = _model.GetControlPoint(id);
			_controlPointViews[id].UpdateControlPoint(controlPoint);
			foreach(var pair in _knotPointViews)
			{
				var guid = pair.Key;
				_model.SetKnotPointAnchor(guid);
				pair.Value.UpdateKnotPoint(_model.GetKnotPoint(guid));
			}
		}

		private ControlPoint CreateControlPoint(Vector3 position)
		{
			return new ControlPoint
			{
				Anchor = position
			};
		}

		#endregion

		#region AddKnotPoint

		public void UpdateKnotVector(int knotCount)
		{
			_model.ClearKnotVector();
			_currentEditingKnotPoint = null;
			ClearKnotPointViews();
			SetRankInputField(_model.Rank);

			var knotVector = BSplineCurves.GenerateKnotVector(knotCount, _model.Rank, _model.IsUniform);
			if (knotVector == null)
				return;

			foreach(var value in knotVector)
			{
				var id = Guid.NewGuid();
				var knotPoint = CreateKnotPoint(value);
				_model.AddKnotPoint(id, knotPoint);
			}

			Dictionary<Guid, KnotPoint> knotDict = _model.KnotPoints;
			foreach(var pair in knotDict)
			{
				var id = pair.Key;
				var knotPoint = pair.Value;
				AddKnotPoint(id, knotPoint);
			}
		}

		private void AddKnotPoint(Guid id, KnotPoint knotPoint)
		{
			_model.SetKnotPointAnchor(id);

			var view = InstantiateKnotPointView(id);
			view.UpdateKnotPoint(knotPoint);
			_knotPointViews.Add(id, view);

			SelectKnotPoint(id);
		}

		private KnotPointView InstantiateKnotPointView(Guid id)
		{
			var go = Instantiate(_knotPointPrefab, _curveContainer);
			var view = go.GetComponent<KnotPointView>();
			view.Id = id;
			view.OnPointerDown(SelectKnotPoint);
			view.OnDrag(DragKnotPoint);

			return view;
		}

		private void SelectKnotPoint(Guid id)
		{
			var point = _model.GetKnotPoint(id);
			if (point == null)
			{
				return;
			}

			foreach (var view in _knotPointViews.Values)
			{
				view.SetSelected(view.Id == id);
			}

			_currentEditingKnotPoint = _knotPointViews[id];
		}

		private void DragKnotPoint(Guid id, Vector2 position)
		{
			_model.DragKnotPoint(id, position);
			foreach (var pair in _knotPointViews)
			{
				var guid = pair.Key;
				_model.SetKnotPointAnchor(guid);
				pair.Value.UpdateKnotPoint(_model.GetKnotPoint(guid));
			}
		}

		private KnotPoint CreateKnotPoint(float value)
		{
			return new KnotPoint
			{
				Value = value
			};
		}

		#endregion


		#region Private Methods

		private void DeleteControlPointCallback()
		{
			if (IsControlPointSelected() == false)
				return;
			DeleteControlPoint(_currentEditingControlPoint.Id);
		}

		private void DeleteControlPoint(Guid id)
		{
			Guid? lastId = _model.DeleteControlPoint(id);
			_controlPointViews.Remove(id);
			Destroy(_currentEditingControlPoint.gameObject);

			if (lastId.HasValue)
				SelectControlPoint(lastId.Value);

			ChangeRank(_model.Rank);
			_canvasView.UpdatePoints();
		}

		private void ChangeRank(uint rank)
		{
			uint newRank = _model.ClampRank(rank);
			_model.ChangeRank(newRank);
			SetRankInputField(newRank);
		}

		private void SetRankInputField(uint rank)
		{
			var callback = _input.onValueChanged;
			_input.onValueChanged = null;
			_input.text = rank.ToString();
			_input.onValueChanged = callback;
		}

		private bool IsControlPointSelected()
		{
			return _currentEditingControlPoint != null;
		}

		private void Clear()
		{
			_model.Clear();
			ClearKnotPointViews();
			ClearControlPointViews();
			_canvasView.UpdatePoints();
		}

		private void ClearKnotPointViews()
		{
			_currentEditingKnotPoint = null;
			foreach (var view in _knotPointViews.Values)
				if (view != null)
					Destroy(view.gameObject);
			_knotPointViews.Clear();
		}

		private void ClearControlPointViews()
		{
			_currentEditingControlPoint = null;
			foreach (var view in _controlPointViews.Values)
				if (view != null)
					Destroy(view.gameObject);
			_controlPointViews.Clear();
		}

		private void Finish()
		{
			_currentPathView = InstantiatePathView();
			Clear();
		}

		private PathView InstantiatePathView()
		{
			var go = Instantiate(_pathPrefab, _curveContainer);
			var view = go.GetComponent<PathView>();
			view.SetColor(_currentPathColor);
			return view;
		}

		private void UpdatePath()
		{
			var points = BSplineCurves.GetPoints(_model.GetPath());
			_currentPathView.UpdatePath(points);
		}

		private void UpdateLineColor(Color color)
		{
			_currentPathView.SetColor(color);
		}

		#endregion

	}
}