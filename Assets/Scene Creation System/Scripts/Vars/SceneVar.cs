using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Dhs5.SceneCreation
{
    [Serializable]
    public class SceneVar
    {
        #region Constructors

        public SceneVar(int _uniqueID, SceneVarType _type, bool global = false)
        {
            uniqueID = _uniqueID;
            ID = "New Scene Var";
            type = _type;

            isLink = false;
            isStatic = false;
            isRandom = false;
            isGlobal = global;
        }

        public SceneVar(SceneVar var)
        {
            uniqueID = var.uniqueID;
            ID = var.ID;
            type = var.type;
            boolValue = var.boolValue;
            intValue = var.intValue;
            floatValue = var.floatValue;
            stringValue = var.stringValue;
            isStatic = var.isStatic;
            isLink = var.isLink;
            isRandom = var.isRandom;
            isGlobal = var.isGlobal;

            hasMin = var.hasMin;
            hasMax = var.hasMax;
            minInt = var.minInt;
            maxInt = var.maxInt;
            minFloat = var.minFloat;
            maxFloat = var.maxFloat;
        }
        public SceneVar(SceneVar sceneVar, BalancingVar bVar)
        {
            if (sceneVar.uniqueID != bVar.uniqueID || sceneVar.type != bVar.type)
            {
                Debug.LogError("Wrong var balancing : " +
                    "Scene Var " + sceneVar.uniqueID + " getting balanced by Balancing Var " + bVar.uniqueID);
                return;
            }

            uniqueID = sceneVar.uniqueID;
            ID = sceneVar.ID;
            type = sceneVar.type;
            boolValue = bVar.boolValue;
            intValue = bVar.intValue;
            floatValue = bVar.floatValue;
            stringValue = bVar.stringValue;
            isStatic = sceneVar.IsStatic;
            isLink = sceneVar.isLink;
            isRandom = sceneVar.IsRandom;
            isGlobal = sceneVar.isGlobal;

            hasMin = sceneVar.hasMin;
            hasMax = sceneVar.hasMax;
            minInt = bVar.minInt;
            maxInt = bVar.maxInt;
            minFloat = bVar.minFloat;
            maxFloat = bVar.maxFloat;
        }
        private SceneVar(int UID, string id, SceneVarType _type, bool _isStatic, bool _isLink, bool _isRandom)
        {
            uniqueID = UID;
            ID = id;
            type = _type;
            isStatic = _isStatic;
            isLink = _isLink;
            isRandom = _isRandom;
            isGlobal = false;

            switch (type)
            {
                case SceneVarType.BOOL: 
                    boolValue = false; 
                    break;
                case SceneVarType.INT: 
                    intValue = 0;
                    break;
                case SceneVarType.FLOAT: 
                    floatValue = 0f;
                    break;
                case SceneVarType.STRING: 
                    stringValue = "";
                    break;
            }
        }
        public static SceneVar CreateLink(ComplexSceneVar var)
        {
            return new(var.uniqueID, var.ID, var.BaseType, false, true, false);
        }
        
        #endregion

        public int uniqueID = 0;
        
        public string ID;
        public SceneVarType type;

        [SerializeField] private bool boolValue;
        [SerializeField] private int intValue;
        [SerializeField] private float floatValue;
        [SerializeField] private string stringValue;

        public bool hasMin;
        public bool hasMax;
        public int minInt;
        public int maxInt;
        public float minFloat;
        public float maxFloat;

        [SerializeField] private bool isStatic = false;
        public bool IsStatic => isStatic;

        [SerializeField] private bool isRandom = false;
        public bool IsRandom => isRandom;

        [SerializeField] private bool isLink = false;
        public bool IsLink => isLink;
        
        [SerializeField] private bool isGlobal = false;
        public bool IsGlobal => isGlobal;

        [SerializeField] private float propertyHeight;

        public bool CanModify
        {
            get
            {
                switch (type)
                {
                    case SceneVarType.EVENT: 
                        return true;
                    case SceneVarType.BOOL:
                    case SceneVarType.STRING:
                        return !IsStatic && !IsLink;
                    case SceneVarType.INT:
                    case SceneVarType.FLOAT:
                        return !IsStatic && !IsLink && !IsRandom;
                    default: 
                        return false;
                }
            }
        }
        /// <summary>
        /// Whether it is a Link to a random ComplexSceneVar
        /// </summary>
        /// <remarks>Runtime only !</remarks>
        public bool IsLinkRandom
        {
            get
            {
                if (!IsLink) return false;

                return SceneState.GetComplexSceneVar(uniqueID).IsRandom;
            }
        }

        #region Values
        public bool BoolValue
        {
            get
            {
                if (isLink) return (bool)LinkValue;
                return boolValue;
            }
            set
            {
                if (isLink)
                {
                    CantSetLinkVar();
                    return;
                }
                boolValue = value;
            }
        }
        public int IntValue
        {
            get
            {
                if (isLink)
                {
                    return (int)LinkValue;
                }
                if (isRandom)
                {
                    //Debug.Log("Rand between " + );
                    return UnityEngine.Random.Range(minInt, maxInt + 1);
                }
                return intValue;
            }
            set
            {
                if (isLink)
                {
                    CantSetLinkVar();
                    return;
                }
                if (isRandom)
                {
                    CantSetRandomVar();
                    return;
                }
                intValue = value;
            }
        }
        public float FloatValue
        {
            get
            {
                if (isLink)
                {
                    return (float)LinkValue;
                }
                if (isRandom)
                {
                    return UnityEngine.Random.Range(minFloat, maxFloat);
                }
                return floatValue;
            }
            set
            {
                if (isLink)
                {
                    CantSetLinkVar();
                    return;
                }
                if (isRandom)
                {
                    CantSetRandomVar();
                    return;
                }
                floatValue = value;
            }
        }
        public string StringValue
        {
            get
            {
                if (isLink) return (string)LinkValue;
                return stringValue;
            }
            set
            {
                if (isLink)
                {
                    CantSetLinkVar();
                    return;
                }
                stringValue = value;
            }
        }
        /// <summary>
        /// Value of this <see cref="SceneVar"/>
        /// </summary>
        /// <remarks>Runtime ONLY</remarks>
        public object Value
        {
            get
            {
                if (isLink) return LinkValue;
                switch (type)
                {
                    case SceneVarType.BOOL: return BoolValue;
                    case SceneVarType.INT: return IntValue;
                    case SceneVarType.FLOAT: return FloatValue;
                    case SceneVarType.STRING: return StringValue;
                    case SceneVarType.EVENT: return null;
                }
                return null;
            }
        }
        /// <summary>
        /// Value of this <see cref="SceneVar"/>
        /// </summary>
        /// <remarks>Editor-safe value</remarks>
        public object EditorValue
        {
            get
            {
                switch (type)
                {
                    case SceneVarType.BOOL: return boolValue;
                    case SceneVarType.INT: return intValue;
                    case SceneVarType.FLOAT: return floatValue;
                    case SceneVarType.STRING: return stringValue;
                    case SceneVarType.EVENT: return null;
                }
                return null;
            }
        }
        /// <summary>
        /// Value of the <see cref="ComplexSceneVar"/> this var is a Link for
        /// </summary>
        /// <remarks>Runtime ONLY</remarks>
        public object LinkValue => SceneState.GetComplexSceneVarValue(uniqueID);

        public void GetStaticValues(out bool boolVal, out int intVal, out float floatVal, out string stringVal)
        {
            boolVal = boolValue;
            intVal = intValue;
            floatVal = floatValue;
            stringVal = stringValue;
        }
        #endregion

        #region Log
        private string GetRange()
        {
            switch (type)
            {
                case SceneVarType.INT:
                    return minInt + " - " + maxInt;
                case SceneVarType.FLOAT: 
                    return minFloat + " - " + maxFloat;
                default: return "";
            }
        }

        public override string ToString()
        {
            if (type == SceneVarType.EVENT) return ID + " (EVENT)";
            if (IsLink) return ID + " (" + type.ToString() + " LINK)";
            return (IsGlobal ? "(GLOBAL) " : "") + ID + " (" + type.ToString() + ") = " + (isRandom ? " (random " + GetRange() + ")" : Value) + (isStatic ? " (static)" : "");
        }
        public string PopupString()
        {
            if (type == SceneVarType.EVENT) return ID + " (EVENT)";
            if (IsLink) return ID + " (" + type.ToString() + " LINK)";
            return (IsGlobal ? "(GLOBAL) " : "") + ID + " (" + type.ToString() + ")" + (isStatic ? " = " + Value : "") + (isRandom ? " (random " + GetRange() + ")" : "");
        }
        public string RuntimeString()
        {
            if (type == SceneVarType.EVENT) return "[" + uniqueID + "] " + ID + " (EVENT)";
            return "[" + uniqueID + "] " + ID + " (" + type.ToString() + ") = " + Value;
        }
        public string RuntimeCompleteString()
        {
            if (type == SceneVarType.EVENT) return "[" + uniqueID + "] " + ID + " (EVENT)";
            if (IsRandom || IsLinkRandom) return "[" + uniqueID + "] " + ID + " (RANDOM " + type.ToString() + ")";
            return "[" + uniqueID + "] " + ID + " (" + (IsStatic ? "STATIC " : "") + type.ToString() + ") = " + Value;
        }
        public string LogString()
        {
            return "[" + uniqueID + "] " + ID + " (" + type + ")" + (IsStatic ? " = " +  EditorValue : null);
        }
        #endregion

        #region Debug
        private void CantSetLinkVar()
        {
            Debug.LogError("This SceneVar is a link to a ComplexSceneVar, you can't set its value");
        }
        private void CantSetRandomVar()
        {
            Debug.LogError("This SceneVar is a random " + type + ", you can't set its value");
        }
        #endregion
    }
}
