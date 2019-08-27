using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace TeamfightTactics
{
    public static class Extensions
    {
        // Reference: https://answers.unity.com/questions/168084/change-layer-of-child.html
        public static void ChangeLayersRecursively(this Transform trans, string name)
        {
            trans.gameObject.layer = LayerMask.NameToLayer(name);

            foreach (Transform child in trans)
                child.ChangeLayersRecursively(name);
        }

        public static void ChangeLayersRecursively(this Transform trans, int layer)
        {
            trans.gameObject.layer = layer;

            foreach (Transform child in trans)
                child.ChangeLayersRecursively(layer);
        }

        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }
    }
}
