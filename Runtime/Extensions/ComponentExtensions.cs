using System.Collections.Generic;
using UnityEngine;

namespace CoreToolkit.Runtime.Extensions
{
    public static class ComponentExtensions
    {
        /// <summary>
        /// Gets <typeparamref name="T"/> component from <paramref name="component"/>'s game object. If it no exist, new one will be created.
        /// </summary>
        /// <typeparam name="T">Component type.</typeparam>
        /// <param name="component">The component.</param>
        /// <returns>Game object's <typeparamref name="T"/> component.</returns>
        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            return component.gameObject.GetOrAddComponent<T>();
        }

        /// <summary>
        /// Trying to get component in this or any it's children.
        /// </summary>
        /// <typeparam name="T">Component type.</typeparam>
        /// <param name="sourceComponent">The component.</param>
        /// <param name="component">Target component.</param>
        /// <param name="includeInactive">Should we find component on inactive game objects?</param>
        /// <returns><see langword="true"/> if component was found.</returns>
        public static bool TryGetComponentInChildren<T>(this Component sourceComponent, out T component, bool includeInactive = false) where T : Component
        {
            return component = sourceComponent.gameObject.GetComponentInChildren<T>(includeInactive);
        }

        /// <summary>
        /// Trying to get component in this or any it's parent.
        /// </summary>
        /// <typeparam name="T">Component type.</typeparam>
        /// <param name="sourceComponent">The component.</param>
        /// <param name="component">Target component.</param>
        /// <param name="includeInactive">Should we find component on inactive game objects?</param>
        /// <returns><see langword="true"/> if component was found.</returns>
        public static bool TryGetComponentInParent<T>(this Component sourceComponent, out T component, bool includeInactive = false) where T : Component
        {
            return component = sourceComponent.gameObject.GetComponentInParent<T>(includeInactive);
        }
        
        /// <summary>
        /// Gets all components of type <typeparamref name="T"/> in the GameObject or any of its children.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] GetComponentsInChildrenWithoutParent<T>(this GameObject gameObject) where T : Component
        {
            var result = new List<T>(gameObject.GetComponentsInChildren<T>());

            if (result.Count > 0)
            {
                if (result[0].gameObject == gameObject)
                {
                    result.RemoveAt(0);
                }
            }

            return result.ToArray();
        }
        
        /// <summary>
        /// Sets the enabled state only when required
        /// </summary>
        /// <param name="behaviour"></param>
        /// <param name="active"></param>
        public static void SetActiveSafe(this Behaviour behaviour, bool active)
        {
            if (behaviour.enabled != active)
            {
                behaviour.enabled = active;
            }
        }
    }
}
