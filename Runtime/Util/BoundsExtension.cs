using UnityEngine;

namespace CoreToolkit.Runtime.Util
{
    public static class BoundsExtension
    {
        public static bool ContainBounds(Transform t, Bounds bounds, Bounds target)
        {
            if (bounds.Contains(target.ClosestPoint(t.position)))
            {
                return true;
            }
            return false;
        }
    }
}
