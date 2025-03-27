using UnityEngine;

namespace CoreToolkit.Runtime.Extensions
{
    public static class LayerMaskExtensions
    {
        public static bool Contains(this LayerMask layerMask, GameObject gameObject)
        {
            return layerMask == (layerMask | (1 << gameObject.layer));
        }
    }
}
