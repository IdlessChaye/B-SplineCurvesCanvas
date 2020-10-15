using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chaye
{
	public static class Utilities
	{
		public static void TrySetActive(GameObject go, bool isActive)
		{
			if (go == null)
				return;
			if (go.activeSelf != isActive)
				go.SetActive(isActive);
		}

		public static bool isFloatEqual(float a, float b)
		{
			const float eps = 0.000001f;
			if (Mathf.Abs(a - b) < eps)
				return true;
			return false;
		}
	}
}