using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;

namespace AshMind.Extensions {
    /// <summary>
    /// Provides a set of extension methods for operations on <see cref="ICollection{T}"/>.
    /// </summary>
    public static class CollectionExtensions {
        /// <summary>
        /// Adds the specified elements to the end of the collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the <paramref name="collection"/>.</typeparam>
        /// <param name="collection">The collection to which the elements should be added.</param>
        /// <param name="values">The elements to add to <paramref name="collection" />.</param>
        public static void AddRange<T>([NotNull] this ICollection<T> collection, [NotNull] IEnumerable<T> values) {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (values == null)     throw new ArgumentNullException(nameof(values));
            Contract.EndContractBlock();
            
            var list = collection as List<T>;
            if (list != null) {
                list.AddRange(values);
                return;
            }
            
            var set = collection as ISet<T>;
            if (set != null) {
                set.UnionWith(values);
                return;
            }

            foreach (var item in values) {
                collection.Add(item);
            }
        }

        /// <summary>
        /// Removes all occurrences of the specified elements from <see cref="ICollection{T}" />.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the <paramref name="collection"/>.</typeparam>
        /// <param name="collection">The collection from which to remove elements.</param>
        /// <param name="values">The elements to remove from <paramref name="collection" />.</param>
        public static void RemoveAll<T>([NotNull] this ICollection<T> collection, [NotNull] IEnumerable<T> values) {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (values == null)     throw new ArgumentNullException(nameof(values));
            Contract.EndContractBlock();

            foreach (var value in values) {
                collection.Remove(value);
            }
        }

        /// <summary>
        /// Removes all elements that match the conditions defined by the specified predicate from the collection.
        /// </summary>
        /// <param name="collection">
        /// The collection from which to remove elements.
        /// </param>
        /// <param name="predicate">
        /// The Func&lt;T, bool&gt; delegate that defines the conditions of the elements to remove.
        /// </param>
        /// <returns>The number of elements that were removed from the collection.</returns>
        public static int RemoveWhere<T>([NotNull] this ICollection<T> collection, [NotNull] [InstantHandle] Func<T, bool> predicate) {
            var concreteList = collection as List<T>;
            if (concreteList != null)
                return concreteList.RemoveAll(predicate.AsPredicate());

            var list = collection as IList<T>;
            if (list != null)
                return RemoveFromListWhere(list, (item, index) => predicate(item));

            var set = collection as HashSet<T>;
            if (set != null)
                return set.RemoveWhere(predicate.AsPredicate());

            var sortedSet = collection as SortedSet<T>;
            if (sortedSet != null)
                return sortedSet.RemoveWhere(predicate.AsPredicate());

            var itemsToRemove = new List<T>();
            foreach (var item in collection) {
                if (predicate(item))
                    itemsToRemove.Add(item);
            }

            collection.RemoveAll(itemsToRemove);
            return itemsToRemove.Count;
        }

        /// <summary>
        /// Removes all elements that match the conditions defined by the specified predicate from the collection.
        /// </summary>
        /// <param name="collection">
        /// The collection from which to remove items.
        /// </param>
        /// <param name="predicate">
        /// The Func&lt;T, int, bool&gt; delegate that defines the conditions of the elements to remove;
        /// the second parameter of the delegate represents the index of the element.
        /// </param>
        /// <returns>The number of elements that were removed from the collection.</returns>
        public static int RemoveWhere<T>([NotNull] this ICollection<T> collection, [NotNull] [InstantHandle] Func<T, int, bool> predicate) {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (predicate == null)  throw new ArgumentNullException(nameof(predicate));
            Contract.EndContractBlock();

            var list = collection as IList<T>;
            if (list != null)
                return RemoveFromListWhere(list, predicate);

            var index = 0;
            var itemsToRemove = new List<T>();
            foreach (var item in collection) {
                if (predicate(item, index))
                    itemsToRemove.Add(item);

                index += 1;
            }
            collection.RemoveAll(itemsToRemove);
            return itemsToRemove.Count;
        }

        private static int RemoveFromListWhere<T>([NotNull] IList<T> list, [NotNull] [InstantHandle] Func<T, int, bool> predicate) {
            var removedCount = 0;
            for (var i = list.Count - 1; i >= 0; i--) {
                if (!predicate(list[i], i))
                    continue;

                list.RemoveAt(i);
                removedCount += 1;
            }

            return removedCount;
        }
    }
}