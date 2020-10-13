using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Chaye
{

	public class CanvasController : MonoBehaviour
	{
		[SerializeField]
		private CanvasView _canvasView = default;
		[SerializeField]
		private RectTransform _curveContainer = default;
		[SerializeField]
		private PathView _pathView = default;
		[SerializeField]
		private GameObject _controlPointPrefab = default;
		private PathModel _model = default;

		private ControlPointView _currentEditingControlPoint;

		private readonly Dictionary<Guid, ControlPointView> _controlPointViews = new Dictionary<Guid, ControlPointView>();

		private void Awake()
		{
			_model = new PathModel();
		}

		private void Start()
		{

		}
	}
}