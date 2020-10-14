using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Chaye
{
	public class PathModel
	{
		public bool IsDirty { get; set; }

		private uint _rank = 3;
		public uint Rank {
			get
			{
				return _rank;
			}
			set
			{
				IsDirty = true;
				_rank = value;
			}
		}

		readonly Dictionary<Guid, ControlPoint> _controlPoints = new Dictionary<Guid, ControlPoint>();
		readonly Dictionary<Guid, KnotPoint> _knotPoints = new Dictionary<Guid, KnotPoint>();

		private int _controlIndex = 0;
		private int ControlIndex {
			get { return _controlIndex++; }
			set { _controlIndex = value; }
		}

		private int ControlPointCount => _controlIndex;

		public void AddControlPoint(Guid id, ControlPoint controlPoint)
		{
			IsDirty = true;
			controlPoint.Index = ControlIndex;
			_controlPoints.Add(id, controlPoint);

			UpdateKnotVector();
		}

		private void UpdateKnotVector()
		{
			int knotCount = (int)Rank + ControlPointCount + 1;
			CanvasController.Instance.UpdateKnotVector(knotCount);
		}

		public void ClearKnotVector()
		{
			IsDirty = true;
			_knotPoints.Clear();
		}

		public void AddKnotPoint(Guid id, KnotPoint knotPoint)
		{
			IsDirty = true;
			_knotPoints.Add(id, knotPoint);
		}




		public void DragControlPoint(Guid id, Vector2 anchore)
		{
			IsDirty = true;
			var point = GetControlPoint(id);
			point.Anchor = anchore;
		}


		public ControlPoint GetControlPoint(Guid id)
		{
			ControlPoint controlPoint;
			if (_controlPoints.TryGetValue(id, out controlPoint))
			{
				return controlPoint;
			}
			return null;
		}

		public List<Vector3> GetPathPoints()
		{
			const int Segments = 60;
			var points = BSplineCurves.GetPoints(this.GetPath(), Segments);
			return points;
		}

		private Path GetPath()
		{
			return new Path
			{
				Rank = this.Rank,
				ControlPoints = _controlPoints.Values.OrderBy(x => x.Index).ToList(),
				KnotVector = _knotPoints.Values.OrderBy(x => x.Value).ToList()
			};
		}
	}
}