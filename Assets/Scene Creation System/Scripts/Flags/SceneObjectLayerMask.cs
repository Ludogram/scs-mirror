using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    [Serializable]
    public class SceneObjectLayerMask
    {
        #region Constructor

        public SceneObjectLayerMask(int value)
        {
            _value = value;
            Clean();
        }

        #endregion

        [SerializeField] private int _value;

        public List<string> Names => MaskToNames(this);
        public string NamesAsString
        {
            get
            {
                List<string> names = Names;

                if (!names.IsValid()) return "Nothing";

                StringBuilder sb = new();
                foreach (var name in names)
                {
                    sb.Append(name);
                    sb.Append(", ");
                }
                return sb.ToString();
            }
        }
        public override string ToString()
        {
            return NamesAsString;
        }

        public void Clean()
        {
            _value = SceneObjectLayerDatabase.Instance.CleanMask(this);
        }

        #region Operation

        public static implicit operator int(SceneObjectLayerMask mask)
        {
            return mask._value;
        }

        public static implicit operator SceneObjectLayerMask(int intVal)
        {
            return new(intVal);
        }

        public static SceneObjectLayerMask operator +(SceneObjectLayerMask mask1, SceneObjectLayerMask mask2)
        {
            return mask1.Union(mask2);
        }
        public static SceneObjectLayerMask operator -(SceneObjectLayerMask mask1, SceneObjectLayerMask mask2)
        {
            return mask1.Remove(mask2);
        }

        public bool Include(SceneObjectLayer layer)
        {
            return layer >= 0 && ((1 << layer) & this) != 0;
        }
        public bool Include(string layerName)
        {
            return Include(SceneObjectLayer.NameToLayer(layerName));
        }
        public bool Exclude(SceneObjectLayer layer)
        {
            return layer < 0 || ((1 << layer) & this) == 0;
        }
        public bool Exclude(string layerName)
        {
            return Exclude(SceneObjectLayer.NameToLayer(layerName));
        }

        /// <returns>True if this contains any layer in common with <paramref name="other"/></returns>
        public bool ContainsAny(SceneObjectLayerMask other) => Intersection(other) != 0;
        /// <returns>True if this lacks any layer that <paramref name="other"/> contains</returns>
        public bool LacksAny(SceneObjectLayerMask other) => other.Remove(this) != 0;

        public SceneObjectLayerMask Intersection(SceneObjectLayerMask other)
        {
            return this & other;
        }
        
        public SceneObjectLayerMask Union(SceneObjectLayerMask other)
        {
            return this | other;
        }

        public SceneObjectLayerMask ExclusiveUnion(SceneObjectLayerMask other)
        {
            return this ^ other;
        }

        public SceneObjectLayerMask Inverse()
        {
            return ~_value;
        }
        public SceneObjectLayerMask Remove(SceneObjectLayerMask other)
        {
            //value ^= Intersection(other); 2 working versions
            return _value & (~other);
        }

        #endregion

        #region Statics

        public static List<string> Layers => SceneObjectLayerDatabase.Instance.AllNames;

        internal static int NameToIndex(string layerName) => SceneObjectLayerDatabase.Instance.IndexOfName(layerName);
        internal static SceneObjectLayerMask NameToMask(string layerName)
        {
            int index = NameToIndex(layerName);

            if (index < 0) return null;

            return 1 << index;
        }

        public static SceneObjectLayerMask NamesToMask(params string[] layerNames)
        {
            if (layerNames == null)
            {
                throw new ArgumentNullException("layerNames");
            }

            int num = 0;
            foreach (string layerName in layerNames)
            {
                int num2 = NameToIndex(layerName);
                if (num2 != -1)
                {
                    num |= 1 << num2;
                }
            }

            return num;
        }

        public static List<string> MaskToNames(SceneObjectLayerMask mask) => SceneObjectLayerDatabase.Instance.NamesOfMask(mask);

        #endregion
    }
}
