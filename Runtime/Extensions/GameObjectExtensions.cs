using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreToolkit.Runtime.Extensions
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Gets <typeparamref name="T"/> component from game object. If it no exist, new one will be created.
        /// </summary>
        /// <typeparam name="T">Component type.</typeparam>
        /// <param name="gameObject">The gameobject.</param>
        /// <returns>Game object's <typeparamref name="T"/> component.</returns>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject.TryGetComponent(out T component))
                return component;

            return gameObject.AddComponent<T>();
        }
        
        /// <summary>
        /// This method is used to hide the GameObject in the Hierarchy view.
        /// </summary>
        /// <param name="gameObject"></param>
        public static void HideInHierarchy(this GameObject gameObject) 
        {
            gameObject.hideFlags = HideFlags.HideInHierarchy;
        }
        
        /// <summary>
        /// Returns the object itself if it exists, null otherwise.
        /// </summary>
        /// <remarks>
        /// This method helps differentiate between a null reference and a destroyed Unity object. Unity's "== null" check
        /// can incorrectly return true for destroyed objects, leading to misleading behaviour. The OrNull method use
        /// Unity's "null check", and if the object has been marked for destruction, it ensures an actual null reference is returned,
        /// aiding in correctly chaining operations and preventing NullReferenceExceptions.
        /// </remarks>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="obj">The object being checked.</param>
        /// <returns>The object itself if it exists and not destroyed, null otherwise.</returns>
        public static T OrNull<T>(this T obj) where T : Object => obj ? obj : null;
        
        

        /// <summary>
        /// Trying to get component in this or any it's children.
        /// </summary>
        /// <typeparam name="T">Component type.</typeparam>
        /// <param name="gameObject">Target gameobject.</param>
        /// <param name="component">Target component.</param>
        /// <param name="includeInactive">Should we find component on inactive game objects?</param>
        /// <returns><see langword="true"/> if component was found.</returns>
        public static bool TryGetComponentInChildren<T>(this GameObject gameObject, out T component, bool includeInactive = false) where T : Component
        {
            return component = gameObject.GetComponentInChildren<T>(includeInactive);
        }

        /// <summary>
        /// Trying to get component in this or any it's parent.
        /// </summary>
        /// <typeparam name="T">Component type.</typeparam>
        /// <param name="gameObject">Target gameobject.</param>
        /// <param name="component">Target component.</param>
        /// <param name="includeInactive">Should we find component on inactive game objects?</param>
        /// <returns><see langword="true"/> if component was found.</returns>
        public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T component, bool includeInactive = false) where T : Component
        {
            return component = gameObject.GetComponentInParent<T>(includeInactive);
        }

        /// <summary>
        /// Check if game object is in layer mask.
        /// </summary>
        /// <param name="gameObject">Target gameobject.</param>
        /// <param name="layerMask">Layer mask for check.</param>
        /// <returns><see langword="true"/> if layer mask contain game object's layer.</returns>
        public static bool IsInLayerMask(this GameObject gameObject, LayerMask layerMask) => ((layerMask.value & (1 << gameObject.layer)) > 0);

        /// <summary>
        /// Check if game object is in layers.
        /// </summary>
        /// <param name="gameObject">Target gameobject.</param>
        /// <param name="layerNames">Layers names for check.</param>
        /// <returns><see langword="true"/> if game object's layer is in <paramref name="layerNames"/>.</returns>
        public static bool IsInLayers(this GameObject gameObject, params string[] layerNames) => IsInLayerMask(gameObject, LayerMask.GetMask(layerNames));

        /// <summary>
        /// Sets layer to all game object hierarchy.
        /// </summary>
        /// <param name="gameObject">Target game object.</param>
        /// <param name="layer">Layer to set.</param>
        public static void SetLayerRecursive(this GameObject gameObject, string layer) => SetLayerRecursive(gameObject, LayerMask.NameToLayer(layer));

        /// <summary>
        /// Sets layer to all game object hierarchy.
        /// </summary>
        /// <param name="gameObject">Target game object.</param>
        /// <param name="layer">Layer to set.</param>
        public static void SetLayerRecursive(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            
            foreach (Transform child in gameObject.transform.GetChilds())
                SetLayerRecursive(child.gameObject, layer);
        }
        
        /// <summary>
        /// Sets all the game object's colliders to the given layer name.
        /// </summary>
        /// <param name="go"></param>
        /// <param name="layer"></param>
        public static void SetColliderLayer(this GameObject go, string layer, bool ignoreTriggers = true)
        {
            var colliders = go.GetComponentsInChildren<Collider>();

            if (colliders.Length == 0)
                return;

            foreach (var col in colliders)
            {
                if (col.isTrigger && ignoreTriggers)
                    continue;

                col.gameObject.layer = LayerMask.NameToLayer(layer);
            }
        }

        /// <summary>
        /// Sets all the game object's colliders to the given layer name.
        /// </summary>
        /// <param name="go"></param>
        /// <param name="layer"></param>
        public static void SetColliderLayer(this GameObject go, int layer, bool ignoreTriggers = true)
        {
            var colliders = go.GetComponentsInChildren<Collider>();

            if (colliders.Length == 0)
                return;

            foreach (var col in colliders)
            {
                if (col.isTrigger && ignoreTriggers)
                    continue;

                col.gameObject.layer = layer;
            }
        }
        
        /// <summary>
        /// Sets all the game object's children (ignoring colliders) to the given layer name.
        /// </summary>
        /// <param name="go"></param>
        /// <param name="layer"></param>
        public static void SetChildLayers(this GameObject go, int layer, bool ignoreColliders = true)
        {
            var children = go.GetComponentsInChildren<Transform>();

            foreach (var c in children)
            {
                if (ignoreColliders && c.GetComponent<Collider>())
                    continue;

                c.gameObject.layer = layer;
            }
        }

        /// <summary>
        /// Sets all the materials of the game object's renderers to the given material.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="material"></param>
        public static void SetMaterials(this GameObject gameObject, Material material)
        {
            var renderers = gameObject.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
                return;

            foreach (var r in renderers)
            {
                var rendererMaterials = r.materials;

                for (int i = 0; i < rendererMaterials.Length; i++)
                {
                    r.material = material;
                }
            }
        }
        
        /// <summary>
        /// Sets the active state only when required
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="active"></param>
        public static void SetActiveSafe(this GameObject gameObject, bool active)
        {
            if (gameObject.activeSelf != active)
            {
                gameObject.SetActive(active);
            }
        }

        /// <summary>
        /// Destroys all children of the game object.
        /// </summary>
        public static void DestroyAllChildren(this GameObject gameObject)
        {
            foreach (Transform child in gameObject.transform)
            {
                Object.Destroy(child.gameObject);
            }
        }
        
        /// <summary>
        /// Immediately destroys all children of the given GameObject.
        /// </summary>
        /// <param name="gameObject">GameObject whose children are to be destroyed.</param>
        public static void DestroyChildrenImmediate(this GameObject gameObject) 
        {
            gameObject.transform.DestroyChildrenImmediate();
        }
        
        /// <summary>
        /// Enables all child GameObjects associated with the given GameObject.
        /// </summary>
        /// <param name="gameObject">GameObject whose child GameObjects are to be enabled.</param>
        public static void EnableChildren(this GameObject gameObject) 
        {
            gameObject.transform.EnableChildren();
        }

        /// <summary>
        /// Disables all child GameObjects associated with the given GameObject.
        /// </summary>
        /// <param name="gameObject">GameObject whose child GameObjects are to be disabled.</param>
        public static void DisableChildren(this GameObject gameObject) 
        {
            gameObject.transform.DisableChildren();
        }

        /// <summary>
        /// Resets the GameObject's transform's position, rotation, and scale to their default values.
        /// </summary>
        /// <param name="gameObject">GameObject whose transformation is to be reset.</param>
        public static void ResetTransformation(this GameObject gameObject) 
        {
            gameObject.transform.Reset();
        }
        
        /// <summary>
        /// Returns the hierarchical path in the Unity scene hierarchy for this GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to get the path for.</param>
        /// <returns>A string representing the full hierarchical path of this GameObject in the Unity scene.
        /// This is a '/'-separated string where each part is the name of a parent, starting from the root parent and ending
        /// with the name of the specified GameObjects parent.</returns>
        public static string Path(this GameObject gameObject) 
        {
            return "/" + string.Join("/",
                gameObject.GetComponentsInParent<Transform>().Select(t => t.name).Reverse().ToArray());
        }

        /// <summary>
        /// Returns the full hierarchical path in the Unity scene hierarchy for this GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to get the path for.</param>
        /// <returns>A string representing the full hierarchical path of this GameObject in the Unity scene.
        /// This is a '/'-separated string where each part is the name of a parent, starting from the root parent and ending
        /// with the name of the specified GameObject itself.</returns>
        public static string PathFull(this GameObject gameObject) 
        {
            return gameObject.Path() + "/" + gameObject.name;
        }
        
        public static bool IsUpsideDown(this Transform transform)
        {
            return transform.localEulerAngles.z > 80 && transform.localEulerAngles.z < 280;
        }

        public static bool IsFacingTowards(this Transform transform, Transform other, float threshold = 0.7f)
        {
            var dot = Vector3.Dot(transform.forward, (other.position - transform.position).normalized);
            return dot > threshold;
        }

        public static bool IsAhead(this Transform transform, Transform other)
        {
            var dot = Vector3.Dot((transform.position - other.position), transform.forward);
            return dot >= 1;
        }

        public static bool IsBehind(this Transform transform, Transform other)
        {
            var dot = Vector3.Dot((transform.position - other.position), transform.forward);
            return dot <= 0;
        }
    }
}