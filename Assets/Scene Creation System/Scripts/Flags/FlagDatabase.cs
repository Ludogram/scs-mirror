using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    public abstract class FlagDatabase : ScriptableObject
    {
        [SerializeField] protected string flag0;
        [SerializeField] protected string flag1;
        [SerializeField] protected string flag2;
        [SerializeField] protected string flag3;
        [SerializeField] protected string flag4;
        [SerializeField] protected string flag5;
        [SerializeField] protected string flag6;
        [SerializeField] protected string flag7;
        [SerializeField] protected string flag8;
        [SerializeField] protected string flag9;
        [SerializeField] protected string flag10;
        [SerializeField] protected string flag11;
        [SerializeField] protected string flag12;
        [SerializeField] protected string flag13;
        [SerializeField] protected string flag14;
        [SerializeField] protected string flag15;
        [SerializeField] protected string flag16;
        [SerializeField] protected string flag17;
        [SerializeField] protected string flag18;
        [SerializeField] protected string flag19;
        [SerializeField] protected string flag20;
        [SerializeField] protected string flag21;
        [SerializeField] protected string flag22;
        [SerializeField] protected string flag23;
        [SerializeField] protected string flag24;
        [SerializeField] protected string flag25;
        [SerializeField] protected string flag26;
        [SerializeField] protected string flag27;
        [SerializeField] protected string flag28;
        [SerializeField] protected string flag29;
        [SerializeField] protected string flag30;
        [SerializeField] protected string flag31;

        #region Core Behaviour

        private void OnValidate()
        {
            (Names, AllNames) = GetAllNames();
            Values = GetAllValues();
        }

        #endregion

        #region Helpers

        private string GetFlagAtIndex(int index)
        {
            if (index < 0 || index >= 32)
            {
                Debug.LogError("Flag index out of range");
                return null;
            }

            return index switch
            {
                0 => flag0,
                1 => flag1,
                2 => flag2,
                3 => flag3,
                4 => flag4,
                5 => flag5,
                6 => flag6,
                7 => flag7,
                8 => flag8,
                9 => flag9,
                10 => flag10,
                11 => flag11,
                12 => flag12,
                13 => flag13,
                14 => flag14,
                15 => flag15,
                16 => flag16,
                17 => flag17,
                18 => flag18,
                19 => flag19,
                20 => flag20,
                21 => flag21,
                22 => flag22,
                23 => flag23,
                24 => flag24,
                25 => flag25,
                26 => flag26,
                27 => flag27,
                28 => flag28,
                29 => flag29,
                30 => flag30,
                31 => flag31,
                _ => null
            };
        }
        
        private int GetIndexOfFlag(string flag)
        {
            if (string.IsNullOrWhiteSpace(flag))
            {
                Debug.LogError("Flag is null or white spaces");
                return -1;
            }

            for (int i = 0; i < 32; i++)
            {
                if (flag == GetFlagAtIndex(i)) return i;
            }

            return -1;
        }

        private bool IsValid(int index) => !string.IsNullOrWhiteSpace(GetFlagAtIndex(index));

        private (List<string> names, List<string> allNames) GetAllNames()
        {
            int lastValid = -1;

            for (int i = 31; i >= 0; i--)
            {
                if (IsValid(i))
                {
                    lastValid = i;
                    break;
                }
            }

            if (lastValid == -1) return (null, null);

            List<string> names = new();
            List<string> allNames = new();

            string n;
            string flag;

            for (int i = 0; i <= lastValid; i++)
            {
                n = i + ": ";
                if (!IsValid(i))
                {
                    n += "--- Empty ---";
                }
                else
                {
                    flag = GetFlagAtIndex(i);
                    n += flag;
                    names.Add(n);
                }
                allNames.Add(n);
            }

            return (names, allNames);
        }

        private List<int> GetAllValues()
        {
            List<int> list = new();

            for (int i = 0; i < 32; i++)
            {
                if (IsValid(i))
                {
                    list.Add(i);
                }
            }

            return list;
        }

        #endregion

        #region Public Accessors

        internal List<string> Names { get => _names; private set => _names = value; }
        [SerializeField, HideInInspector] private List<string> _names;
        internal List<string> AllNames { get => _allNames; private set => _allNames = value; }
        [SerializeField, HideInInspector] private List<string> _allNames;
        internal List<int> Values { get => _values; private set => _values = value; }
        [SerializeField, HideInInspector] private List<int> _values;

        internal string NameAtIndex(int index) => GetFlagAtIndex(index);
        internal List<string> NamesAtIndexes(ICollection<int> indexes)
        {
            List<string> names = new();
            foreach (int index in indexes)
            {
                names.Add(GetFlagAtIndex(index));
            }

            return names;
        }
        internal List<string> NamesOfTag(SceneObjectTag tag)
        {
            List<string> names = new();
            for (int i = 0; i < 32; i++)
            {
                if (tag.Include(i) && IsValid(i)) names.Add(GetFlagAtIndex(i));
            }

            return names;
        }
        internal List<string> NamesOfMask(SceneObjectLayerMask mask)
        {
            List<string> names = new();
            for (int i = 0; i < 32; i++)
            {
                if (mask.Include(i) && IsValid(i)) names.Add(GetFlagAtIndex(i));
            }

            return names;
        }

        internal int IndexOfName(string name) => GetIndexOfFlag(name);
        internal List<int> IndexesOfNames(ICollection<string> names)
        {
            List<int> indexes = new();
            foreach (string name in names)
            {
                indexes.Add(GetIndexOfFlag(name));
            }

            return indexes;
        }

        internal int CleanTag(SceneObjectTag tag)
        {
            int cleaner = 0;
            for (int i = 0; i < 32; i++)
            {
                if (tag.Include(i) && !IsValid(i)) cleaner |= 1 << i;
            }

            return tag & (~cleaner);
        }
        internal int CleanMask(SceneObjectLayerMask mask)
        {
            int cleaner = 0;
            for (int i = 0; i < 32; i++)
            {
                if (mask.Include(i) && !IsValid(i)) cleaner |= 1 << i;
            }

            return mask & (~cleaner);
        }
        internal int CleanMask(FlagMask mask)
        {
            int cleaner = 0;
            for (int i = 0; i < 32; i++)
            {
                if (mask.Include(i) && !IsValid(i)) cleaner |= 1 << i;
            }

            return mask & (~cleaner);
        }
        #endregion
    }
}
