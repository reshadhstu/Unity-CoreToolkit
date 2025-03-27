using System;
using System.Collections.Generic;

namespace CoreToolkit.Runtime.Extensions
{
    public static class ListExtensions
    {
        public static void RemoveNullElements<T>(this IList<T> list)
        {
            //Note: This method does not identify missing elements (.asset files) in the list as null!

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == null)
                {
                    list.Remove(list[i]);
                }
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;

            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        public static TElement GetRandom<TElement>(this List<TElement> list)
        {
            if (list.Count == 0)
                return default;

            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static TElement GetNext<TElement>(this List<TElement> list, TElement element)
        {
            var index = list.IndexOf(element);

            index = (index + 1) % list.Count;

            return list[index];
        }

        public static TElement GetPrevious<TElement>(this List<TElement> list, TElement element)
        {
            var index = list.IndexOf(element);

            index -= 1;
            if (index < 0)
            {
                index = list.Count - 1;
            }

            return list[index];
        }

        public static void Move<T>(this List<T> list, int oldIndex, int newIndex)
        {
            T item = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, item);
        }
    }
}