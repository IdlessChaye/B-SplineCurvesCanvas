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
		[SerializeField]
		private UnityEngine.UI.InputField _input = default;

		private PathModel _model;

		private ControlPointView _currentEditingControlPoint = default;
		private readonly Dictionary<Guid, ControlPointView> _controlPointViews = new Dictionary<Guid, ControlPointView>();

		private KnotPointView _currentEditingKnotPoint = default;
		private readonly Dictionary<Guid, KnotPointView> _knotPointViews = new Dictionary<Guid, KnotPointView>();

		

		private void Awake()
		{
			_instance = this;
			BSplineCurves.Segments = 100;
			_model = new PathModel();
		}

		private void Start()
		{
			Action deleteControlPointCallback = () =>
			{
				if (IsControlPointSelected() == false)
					return;
				DeleteControlPoint(_currentEditingControlPoint.Id);
			};

			_canvasView.OnPointerDown(position =>
			{
				if (Input.GetMouseButton(0))
					AddControlPoint(position);
				else
					deleteControlPointCallback();
			});

			_canvasView.OnChangedInputFieldRank((rankStr) =>
			{
				uint rank;
				if (uint.TryParse(rankStr, out rank))
				{
					ChangeRank(rank);
				}
			});

			_canvasView.OnClickButtonDelete(() => deleteControlPointCallback());

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

			Dictionary<Guid, KnotPoint> knotDict = new Dictionary<Guid, KnotPoint>();
			foreach(var value in knotVector)
			{
				var id = Guid.NewGuid();
				var knotPoint = CreateKnotPoint(value);
				_model.AddKnotPoint(id, knotPoint);

				knotDict.Add(id, knotPoint);
			}

			foreach(var pair in knotDict)
			{
				var id = pair.Key;
				var knotPoint = pair.Value;
				AddKnotPoint(id, knotPoint);
			}
		}

		private void AddKnotPoint(Guid id, KnotPoint knotPoint)
		{
			_model.SetKnotPointAnchor(id, knotPoint);

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
			var knotPoint = _model.GetKnotPoint(id);
			_knotPointViews[id].UpdateKnotPoint(knotPoint);
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

			ChangeRank(_model.Rank);
		}

		private void ChangeRank(uint rank)
		{
			uint newRank = _model.ClampRank(rank);
			_input.text = newRank.ToString();
			_model.ChangeRank(newRank);
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
			var points = BSplineCurves.GetPoints(_model.GetPath());
			_pathView.UpdatePath(points);
		}
	}
}