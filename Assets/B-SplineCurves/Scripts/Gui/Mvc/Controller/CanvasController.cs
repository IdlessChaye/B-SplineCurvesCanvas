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
		private PathView _pathView = default;
		[SerializeField]
		private GameObject _controlPointPrefab = default;
		[SerializeField]
		private GameObject _knotPointPrefab = default;

		private PathModel _model;

		private ControlPointView _currentEditingControlPoint = default;
		private readonly Dictionary<Guid, ControlPointView> _controlPointViews = new Dictionary<Guid, ControlPointView>();

		private KnotPointView _currentEditingKnotPoint = default;
		private readonly Dictionary<Guid, KnotPointView> _knotPointViews = new Dictionary<Guid, KnotPointView>();

		private void Awake()
		{
			_instance = this;
			_model = new PathModel();
		}

		private void Start()
		{
			_canvasView.OnPointerDown(position =>
			{
				AddControlPoint(position);
			});

			_canvasView.OnChangedInputFieldRank(rankStr =>
			{
				uint rank;
				if (uint.TryParse(rankStr, out rank))
				{
					ChangeRank(rank);
				}
			});

			_canvasView.OnClickButtonDelete(() =>
			{
				if (IsControlPointSelected() == false)
					return;
				DeleteControlPoint(_currentEditingControlPoint.Id);
			});

			_canvasView.OnClickButtonClear(Clear);
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
			if (knotCount < 2)
				return;

			_currentEditingKnotPoint = null;
			ClearKnotPointViews();
			_model.ClearKnotVector();

			var knotVector = BSplineCurves.GenerateKnotVector(knotCount);
			foreach(var value in knotVector)
			{
				AddKnotPoint(value);
			}
		}

		private void AddKnotPoint(float value)
		{
			var id = Guid.NewGuid();
			var view = InstantiateKnotPointView(id);
			_knotPointViews.Add(id, view);

			var knotPoint = CreateKnotPoint(value);
			_model.AddKnotPoint(id, knotPoint);
			//view.KnotPoint = knotPoint;
		}

		private KnotPointView InstantiateKnotPointView(Guid id)
		{
			return null;
		}

		private KnotPoint CreateKnotPoint(float value)
		{
			return new KnotPoint
			{
				Value = value
			};
		}

		#endregion

		private void DeleteControlPoint(Guid id)
		{
			Guid? lastId = _model.DeleteControlPoint(id);
			_controlPointViews.Remove(id);
			Destroy(_currentEditingControlPoint.gameObject);

			if (lastId.HasValue)
				SelectControlPoint(lastId.Value);
		}


		private void ChangeRank(uint rank)
		{
			_model.ChangeRank(rank);
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

		private void UpdatePath()
		{
			const int Segments = 60;
			var points = BSplineCurves.GetPoints(_model.GetPath(), Segments);
			_pathView.UpdatePath(points);
		}
	}
}