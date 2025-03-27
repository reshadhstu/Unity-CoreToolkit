using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoreToolkit.Runtime.Extensions
{
    public static class UIExtensions
    {
        #region Event Trigger
        /// <summary>
        /// Add a listener to an Event Trigger
        /// </summary>
        /// <param name="go"></param>
        /// <param name="layer"></param>
        public static void AddListener(this EventTrigger eventTrigger, EventTriggerType eventType,
                                       Action<PointerEventData> callback)
        {
            var entry = new EventTrigger.Entry();
            entry.eventID = eventType;
            entry.callback.AddListener((data) => callback.Invoke((PointerEventData)data));
            eventTrigger.triggers.Add(entry);
        }
        #endregion

        #region Image
        public static void SetSprite(this Image image, Sprite sprite)
        {
            image.sprite = sprite;
        }

        public static void DisableIfNullSprite(this Image image)
        {
            image.enabled = image.sprite != null;
        }
        #endregion

        #region Canvas Group
        public static void SetAlpha(this CanvasGroup group, float alpha)
        {
            if (group == null || group.alpha == alpha)
                return;

            group.alpha = alpha;
            group.interactable = group.blocksRaycasts = alpha > 0;
        }

        public static IEnumerator Fade(this CanvasGroup group, float duration, float targetAlpha)
        {
            if (duration <= 0)
            {
                group.SetAlpha(targetAlpha);
                yield break;
            }

            var time = 0f;
            var start = group.alpha;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                group.SetAlpha(Mathf.Lerp(start, targetAlpha, time / duration));
                yield return null;
            }

            yield break;
        }
        #endregion

        #region RectTransform
        public static void SetPivot(this RectTransform rectTransform, Vector2 pivot)
        {
            Vector3 deltaPosition = rectTransform.pivot - pivot;
            
            deltaPosition.Scale(rectTransform.rect.size);
            deltaPosition.Scale(rectTransform.localScale);
            deltaPosition = rectTransform.localRotation * deltaPosition;
    
            rectTransform.pivot = pivot;
            rectTransform.localPosition -= deltaPosition;
        }
        #endregion

        #region ScrollView
        public static Vector2 CalculateFocusedScrollPosition(this ScrollRect scrollView, Vector2 focusPoint)
        {
            Vector2 contentSize = scrollView.content.rect.size;
            Vector2 viewportSize = ((RectTransform)scrollView.content.parent).rect.size;
            Vector2 contentScale = scrollView.content.localScale;

            contentSize.Scale(contentScale);
            focusPoint.Scale(contentScale);

            Vector2 scrollPosition = scrollView.normalizedPosition;
            if (scrollView.horizontal && contentSize.x > viewportSize.x)
                scrollPosition.x = Mathf.Clamp01((focusPoint.x - viewportSize.x * 0.5f) / (contentSize.x - viewportSize.x));
            if (scrollView.vertical && contentSize.y > viewportSize.y)
                scrollPosition.y = Mathf.Clamp01((focusPoint.y - viewportSize.y * 0.5f) / (contentSize.y - viewportSize.y));

            return scrollPosition;
        }

        public static Vector2 CalculateFocusedScrollPosition(this ScrollRect scrollView, RectTransform item)
        {
            Vector2 itemCenterPoint = scrollView.content.InverseTransformPoint(item.transform.TransformPoint(item.rect.center));

            Vector2 contentSizeOffset = scrollView.content.rect.size;
            contentSizeOffset.Scale(scrollView.content.pivot);

            return scrollView.CalculateFocusedScrollPosition(itemCenterPoint + contentSizeOffset);
        }

        public static void FocusAtPoint(this ScrollRect scrollView, Vector2 focusPoint)
        {
            scrollView.normalizedPosition = scrollView.CalculateFocusedScrollPosition(focusPoint);
        }

        public static void FocusOnItem(this ScrollRect scrollView, RectTransform item)
        {
            scrollView.normalizedPosition = scrollView.CalculateFocusedScrollPosition(item);
        }

        static IEnumerator LerpToScrollPositionCoroutine(this ScrollRect scrollView, Vector2 targetNormalizedPos, float speed)
        {
            Vector2 initialNormalizedPos = scrollView.normalizedPosition;

            float t = 0f;
            while (t < 1f)
            {
                scrollView.normalizedPosition = Vector2.LerpUnclamped(initialNormalizedPos, targetNormalizedPos, 1f - (1f - t) * (1f - t));

                yield return null;
                t += speed * Time.unscaledDeltaTime;
            }

            scrollView.normalizedPosition = targetNormalizedPos;
        }

        public static IEnumerator FocusAtPointCoroutine(this ScrollRect scrollView, Vector2 focusPoint, float speed)
        {
            yield return scrollView.LerpToScrollPositionCoroutine(scrollView.CalculateFocusedScrollPosition(focusPoint), speed);
        }

        public static IEnumerator FocusOnItemCoroutine(this ScrollRect scrollView, RectTransform item, float speed)
        {
            yield return scrollView.LerpToScrollPositionCoroutine(scrollView.CalculateFocusedScrollPosition(item), speed);
        }

        public static IEnumerator SelectChild(this ScrollRect scrollView, int index, bool focus = true)
        {
            if (scrollView.content.childCount == 0)
                yield break;

            yield return null;

            if (scrollView.content.childCount >= index)
            {
                var child = scrollView.content.GetChild(index);

                if (child.TryGetComponent<Selectable>(out var selectable))
                {
                    selectable.Select();
                }

                if (child.TryGetComponent<RectTransform>(out var rect))
                {
                    scrollView.FocusOnItem(rect);
                }
            }
        }
        #endregion
    }
}