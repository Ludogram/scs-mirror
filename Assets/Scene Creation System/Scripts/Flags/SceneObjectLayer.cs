using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    [Serializable]
    public class SceneObjectLayer
    {
        #region Constructor

        public SceneObjectLayer(int value)
        {
            _value = value;
        }

        #endregion

        [SerializeField] private int _value;

        public string Name => LayerToName(this);
        public override string ToString()
        {
            return _value + ": " + Name;
        }

        #region Operation

        public static implicit operator int(SceneObjectLayer layer)
        {
            return layer._value;
        }

        public static implicit operator SceneObjectLayer(int intVal)
        {
            return new(intVal);
        }

        public bool IsContainedIn(SceneObjectLayerMask mask)
        {
            return mask.Include(this);
        }


        #endregion

        #region Statics

        public static List<string> Layers => SceneObjectLayerDatabase.Instance.Names;
        internal static List<int> LayerValues => SceneObjectLayerDatabase.Instance.Values;

        internal static int NameToIndex(string layerName) => SceneObjectLayerDatabase.Instance.IndexOfName(layerName);
        internal static SceneObjectLayer NameToLayer(string layerName)
        {
            int index = NameToIndex(layerName);

            if (index < 0) return null;

            return index;
        }
        internal static string LayerToName(SceneObjectLayer layer) => SceneObjectLayerDatabase.Instance.NameAtIndex(layer._value);

        #endregion
    }
}
