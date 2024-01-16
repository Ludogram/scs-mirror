using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    public class FlagMask
    {
        #region Constructor

        internal FlagMask(int value)
        {
            _value = value;
            Clean();
        }

        #endregion

        [SerializeField] protected int _value;

        #region Operations

        public static implicit operator int(FlagMask mask)
        {
            return mask._value;
        }

        public static implicit operator FlagMask(int intVal)
        {
            return new(intVal);
        }

        public static FlagMask operator +(FlagMask mask1, FlagMask mask2)
        {
            return mask1.Union(mask2);
        }
        public static FlagMask operator -(FlagMask mask1, FlagMask mask2)
        {
            return mask1.Remove(mask2);
        }

        public bool Include(int index)
        {
            return index >= 0 && ((1 << index) & this) != 0;
        }
        public bool Exclude(int index)
        {
            return index < 0 || ((1 << index) & this) == 0;
        }

        /// <returns>True if this contains any flag in common with <paramref name="other"/></returns>
        public bool ContainsAny(FlagMask other) => Intersection(other) != 0;
        /// <returns>True if this lacks any flag that <paramref name="other"/> contains</returns>
        public bool LacksAny(FlagMask other) => other.Remove(this) != 0;

        public FlagMask Intersection(FlagMask other)
        {
            return this & other;
        }

        public FlagMask Union(FlagMask other)
        {
            return this | other;
        }

        public FlagMask ExclusiveUnion(FlagMask other)
        {
            return this ^ other;
        }

        public FlagMask Inverse()
        {
            return ~_value;
        }
        public FlagMask Remove(FlagMask other)
        {
            //value ^= Intersection(other); 2 working versions
            return _value & (~other);
        }

        #endregion

        #region Database

        protected virtual FlagDatabase Database { get; }
        public void Clean()
        {
            _value = Database.CleanMask(this);
        }

        #endregion
    }
}
