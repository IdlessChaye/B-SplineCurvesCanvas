﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Chaye
{
	public class PathModel
	{
		public bool IsDirty { get; set; }

		public bool IsAutoAdjustRank { get; set; }

		// is knot vector uniformed
		private bool _isUniform = false;
		public bool IsUniform {
			get { return _isUniform; }
			set
			{
				IsDirty = true;
				_isUniform = value;
			}
		}

		// rank of B-Spline Curves
		private uint _rank = 0;
		public uint Rank {
			get { return _rank; }
			set
			{
				IsDirty = true;
				_rank = value;
			}
		}

		private readonly Dictionary<Guid, ControlPoint> _controlPoints = new Dictionary<Guid, ControlPoint>();
		private readonly Dictionary<Guid, KnotPoint> _knotPoints = new Dictionary<Guid, KnotPoint>();
		public Dictionary<Guid, KnotPoint> KnotPoints => _knotPoints;

		public int ControlPointCount => _controlPoints.Count;
		private int _controlIndex = 0;

		public uint ClampRank(uint rank)
		{
			if (ControlPointCount == 0)
				rank = 0;
			else if (rank > ControlPointCount - 1)
				rank = (uint)ControlPointCount - 1;
			return rank;
		}

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

			if (IsAutoAdjustRank == true)
			{
				if (ControlPointCount == 1)
					Rank = ClampRank(0);
				else if (ControlPointCount == 2)
					Rank = ClampRank(1);
				else if (ControlPointCount == 3)
					Rank = ClampRank(2);
			}

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

		public void UpdateKnotVector()
		{
			int knotCount = 0;
			if (ControlPointCount != 0)
				knotCount = (int)Rank + ControlPointCount + 1;
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

		public void SetKnotPointAnchor(Guid id)
		{
			KnotPoint knotPoint;
			if (_knotPoints.TryGetValue(id, out knotPoint))
				knotPoint.Anchor = BSplineCurves.GetPoint(GetPath(), knotPoint.Value);
		}




		public void DragControlPoint(Guid id, Vector2 anchore)
		{
			IsDirty = true;
			var point = GetControlPoint(id);
			if (point == null)
				return;
			point.Anchor = anchore;
		}

		public void DragKnotPoint(Guid id, Vector2 anchore)
		{
			IsDirty = true;
			var point = GetKnotPoint(id);
			if (point == null)
				return;
			//point.Anchor = anchore;
			const float speed = 0.5f;
			var mouseMove = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
			var moveLength = Vector2.Dot(mouseMove, Vector2.up + Vector2.right);
			if (moveLength > 0)
			{
				point.Value += Time.deltaTime * speed;
			}
			else
			{
				point.Value -= Time.deltaTime * speed;
			}
			point.Value = Mathf.Clamp(point.Value, 0.0f, 1.0f);
			point.Anchor = BSplineCurves.GetPoint(GetPath(), point.Value);
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

		public KnotPoint GetKnotPoint(Guid id)
		{
			KnotPoint knotPoint;
			if (_knotPoints.TryGetValue(id, out knotPoint))
			{
				return knotPoint;
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
			Rank = 0;
			_controlPoints.Clear();
			_knotPoints.Clear();
			_controlIndex = 0;
			UpdateKnotVector();
		}
	}
}