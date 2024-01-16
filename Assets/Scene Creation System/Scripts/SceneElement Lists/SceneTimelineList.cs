using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    [Serializable]
    public class SceneTimelineList : IList<SceneTimeline>
    {
        [SerializeField] private List<SceneTimeline> list;

        [SerializeField] private int currentTimeline;
        [SerializeField] private int currentStep;

        [SerializeField] private float propertyHeight;

        #region List
        public SceneTimeline this[int index] { get => list[index]; set => list[index] = value; }

        public int Count => list.Count;

        public bool IsReadOnly => false;

        public void Add(SceneTimeline item)
        {
            list.Add(item);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(SceneTimeline item)
        {
            return list.Contains(item);
        }

        public void CopyTo(SceneTimeline[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<SceneTimeline> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public int IndexOf(SceneTimeline item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, SceneTimeline item)
        {
            list.Insert(index, item);
        }

        public bool Remove(SceneTimeline item)
        {
            return list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public SceneTimeline Find(Predicate<SceneTimeline> match)
        {
            return list.Find(match);
        }
        public List<SceneTimeline> FindAll(Predicate<SceneTimeline> match)
        {
            return list.FindAll(match);
        }
        #endregion
    }
}
