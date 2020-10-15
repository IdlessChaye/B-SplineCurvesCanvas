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
	}
}