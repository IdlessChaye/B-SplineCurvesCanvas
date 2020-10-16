using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chaye
{
	[RequireComponent(typeof(LineRenderer))]
	public class PathView : MonoBehaviour
	{
		private LineRenderer _lineRenderer;

		private void Awake()
		{
			_lineRenderer = GetComponent<LineRenderer>();
		}

		public void UpdatePath(List<Vector3> points)
		{
			_lineRenderer.positionCount = points.Count;
			_lineRenderer.SetPositions(points.ToArray());
		}

		public void SetColor(Color color)
		{
			_lineRenderer.startColor = color;
			_lineRenderer.endColor = color;
		}

	}
}

