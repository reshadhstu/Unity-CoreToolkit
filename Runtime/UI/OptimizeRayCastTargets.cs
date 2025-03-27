using UnityEngine;
using UnityEngine.UI;

namespace CoreToolkit.Runtime.UI
{
	public class OptimizeRayCastTargets : MonoBehaviour
	{
		private void EnableRayCastTargets()
		{
			Image[] images = GetComponentsInChildren<Image>();

			for (int i = 0; i < images.Length; i++)
			{
				images[i].raycastTarget = true;
			}
		}
		
		private void DisableRayCastTargets()
		{
			Image[] images = GetComponentsInChildren<Image>();

			for (int i = 0; i < images.Length; i++)
			{
				images[i].raycastTarget = false;
			}
		}
	}
}