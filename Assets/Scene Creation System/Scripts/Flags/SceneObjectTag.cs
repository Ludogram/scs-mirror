using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    [Serializable]
    public class SceneObjectTag
    {
        #region Constructor

        public SceneObjectTag(int value)
        {
            _value = value;
            Clean();
        }

        #endregion

        [SerializeField] private int _value;

        public List<string> Names => TagToNames(this);
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

        public void Clean()
        {
            _value = SceneObjectTagDatabase.Instance.CleanTag(this);
        }

        #region Operation

        public static implicit operator int(SceneObjectTag tag)
        {
            return tag._value;
        }

        public static implicit operator SceneObjectTag(int intVal)
        {
            return new(intVal);
        }

        public static SceneObjectTag operator +(SceneObjectTag tag1, SceneObjectTag tag2)
        {
            return tag1.Union(tag2);
        }
        public static SceneObjectTag operator -(SceneObjectTag tag1, SceneObjectTag tag2)
        {
            return tag1.Remove(tag2);
        }

        internal bool Include(int index)
        {
            return index != -1 && ((1 << index) & this) != 0;
        }
        internal bool Exclude(int index)
        {
            return index == -1 || ((1 << index) & this) == 0;
        }

        /// <returns>True if this contains any tag in common with <paramref name="other"/></returns>
        public bool ContainsAny(SceneObjectTag other) => Intersection(other) != 0;
        /// <returns>True if this lacks any tag that <paramref name="other"/> contains</returns>
        public bool LacksAny(SceneObjectTag other) => other.Remove(this) != 0;

        public SceneObjectTag Intersection(SceneObjectTag other)
        {
            return this & other;
        }
        
        public SceneObjectTag Union(SceneObjectTag other)
        {
            return this | other;
        }

        public SceneObjectTag ExclusiveUnion(SceneObjectTag other)
        {
            return this ^ other;
        }

        public SceneObjectTag Inverse()
        {
            return ~_value;
        }
        public SceneObjectTag Remove(SceneObjectTag other)
        {
            //value ^= Intersection(other); 2 working versions
            return _value & (~other);
        }

        public bool HasTag(string tag)
        {
            return Include(NameToIndex(tag));
        }
        public bool HasTagByIndex(int index)
        {
            return Include(index);
        }

        #endregion

        #region Statics

        public static List<string> Tags => SceneObjectTagDatabase.Instance.AllNames;

        internal static int NameToIndex(string tagName) => SceneObjectTagDatabase.Instance.IndexOfName(tagName);
        internal static SceneObjectTag NameToTag(string tagName)
        {
            int index = NameToIndex(tagName);

            if (index < 0) return null;

            return 1 << index;
        }

        public static SceneObjectTag NamesToTag(params string[] tagNames)
        {
            if (tagNames == null)
            {
                throw new ArgumentNullException("tagNames");
            }

            int num = 0;
            foreach (string tagName in tagNames)
            {
                int num2 = NameToIndex(tagName);
                if (num2 != -1)
                {
                    num |= 1 << num2;
                }
            }

            return num;
        }

        public static List<string> TagToNames(SceneObjectTag tag) => SceneObjectTagDatabase.Instance.NamesOfTag(tag);

        #endregion
    }
}
