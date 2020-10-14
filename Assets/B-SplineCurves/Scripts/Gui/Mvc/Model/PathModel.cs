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
		private int ControlPointCount => _controlPoints.Count;

		public void ChangeRank(uint newRank)
		{
			IsDirty = true;
			Rank = newRank;

			UpdateKnotVector();
		}

		public void AddControlPoint(Guid id, ControlPoint controlPoint)
		{
			IsDirty = true;
			controlPoint.Index = _controlIndex++;
			_controlPoints.Add(id, controlPoint);

			UpdateKnotVector();
		}

		public Guid? DeleteControlPoint(Guid id)
		{
			IsDirty = true;
			var point = GetControlPoint(id);
			if (point == null)
				return null;

			_controlPoints.Remove(id);
			Guid? previousId = null;
			int indexPrevious = point.Index - 1;
			foreach(var pair in _controlPoints)
			{
				if (pair.Value.Index == indexPrevious)
				{
					previousId = pair.Key;
					break;
				}
			}

			var points = _controlPoints.Values.OrderBy(x => x.Index).ToList();
			for (_controlIndex = 0; _controlIndex < ControlPointCount; _controlIndex++)
				points[_controlIndex].Index = _controlIndex;

			UpdateKnotVector();
			return previousId;
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

		public Path GetPath()
		{
			return new Path
			{
				Rank = this.Rank,
				ControlPoints = _controlPoints.Values.OrderBy(x => x.Index).ToList(),
				KnotVector = _knotPoints.Values.OrderBy(x => x.Value).ToList()
			};
		}

		public void Clear()
		{
			IsDirty = true;
			_controlPoints.Clear();
			_knotPoints.Clear();
			_controlIndex = 0;
		}
	}
}