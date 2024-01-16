using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace Dhs5.SceneCreation
{
    #region Enums
    [Serializable]
    public enum SceneVarType
    {
        BOOL, INT, FLOAT, STRING, EVENT
    }

    [Serializable]
    public enum BoolOperation
    {
        SET, INVERSE, TO_TRUE, TO_FALSE
    }
    [Serializable]
    public enum BoolComparison
    {
        EQUAL, DIFF, IS_TRUE, IS_FALSE
    }
    [Serializable]
    public enum IntOperation
    {
        SET, ADD, SUBSTRACT, MULTIPLY, DIVIDE, POWER, TO_MIN, TO_MAX, TO_NULL, INCREMENT, DECREMENT
    }
    [Serializable]
    public enum IntComparison
    {
        EQUAL, DIFF, SUP, INF, SUP_EQUAL, INF_EQUAL, IS_MIN, IS_MAX, IS_NULL, IS_POSITIVE, IS_NEGATIVE
    }
    [Serializable]
    public enum FloatOperation
    {
        SET, ADD, SUBSTRACT, MULTIPLY, DIVIDE, POWER, TO_MIN, TO_MAX, TO_NULL, INCREMENT, DECREMENT
    }
    [Serializable]
    public enum FloatComparison
    {
        EQUAL, DIFF, SUP, INF, SUP_EQUAL, INF_EQUAL, IS_MIN, IS_MAX, IS_NULL, IS_POSITIVE, IS_NEGATIVE
    }
    [Serializable]
    public enum StringOperation
    {
        SET, APPEND, REMOVE
    }
    [Serializable]
    public enum StringComparison
    {
        EQUAL, DIFF, CONTAINS, CONTAINED, NULL_EMPTY
    }
    [Serializable]
    public enum LogicOperator
    {
        AND, OR, NAND, NOR, XOR, XNOR
    }
    #endregion

    public static class SceneState
    {
        private static List<BaseSceneObject> sceneObjects = new();

        //private static event Action onStartScene;
        //private static event Action onChangeScene;
        //private static event Action onCompleteScene;
        //private static event Action onGameOver;
        //
        //private static event Action<int> onSceneUpdate;

        private static List<IOnStartScene> onStartSceneObjects = new();
        private static List<IOnChangeScene> onChangeSceneObjects = new();
        private static List<IOnCompleteScene> onCompleteSceneObjects = new();
        private static List<IOnUpdateScene> onUpdateSceneObjects = new();
        private static List<IOnGameOver> onGameOverObjects = new();

        private static Dictionary<int, SceneVar> SceneVariables = new();
        private static Dictionary<int, ComplexSceneVar> ComplexSceneVariables = new();
        private static Dictionary<int, List<int>> SceneVarLinks = new();
        private static Dictionary<int, object> FormerValues = new();

        #region Scene Object Registration
        public static void Register(BaseSceneObject sceneObject)
        {
            if (sceneObjects.Contains(sceneObject)) return;

            sceneObjects.Add(sceneObject);

            if (sceneObject is IOnStartScene onStartSO) onStartSceneObjects.Add(onStartSO);
            if (sceneObject is IOnChangeScene onChangeSO) onChangeSceneObjects.Add(onChangeSO);
            if (sceneObject is IOnCompleteScene onCompleteSO) onCompleteSceneObjects.Add(onCompleteSO);
            if (sceneObject is IOnUpdateScene onUpdateSO) onUpdateSceneObjects.Add(onUpdateSO);
            if (sceneObject is IOnGameOver onGameOverSO) onGameOverObjects.Add(onGameOverSO);

            //if (sceneObject.DoStartScene)
            //    onStartScene += sceneObject.OnStartScene;
            //if (sceneObject.DoChangeScene)
            //    onChangeScene += sceneObject.OnChangeScene;
            //if (sceneObject.DoCompleteScene)
            //    onCompleteScene += sceneObject.OnCompleteScene;
            //if (sceneObject.DoGameOver)
            //    onGameOver += sceneObject.OnGameOver;
            //
            //if (sceneObject.DoUpdateScene)
            //    onSceneUpdate += sceneObject.OnUpdateScene;
        }
        public static void Unregister(BaseSceneObject sceneObject)
        {
            if (!sceneObjects.Contains(sceneObject)) return;

            sceneObjects.Remove(sceneObject);

            if (sceneObject is IOnStartScene onStartSO) onStartSceneObjects.Remove(onStartSO);
            if (sceneObject is IOnChangeScene onChangeSO) onChangeSceneObjects.Remove(onChangeSO);
            if (sceneObject is IOnCompleteScene onCompleteSO) onCompleteSceneObjects.Remove(onCompleteSO);
            if (sceneObject is IOnUpdateScene onUpdateSO) onUpdateSceneObjects.Remove(onUpdateSO);
            if (sceneObject is IOnGameOver onGameOverSO) onGameOverObjects.Remove(onGameOverSO);

            //if (sceneObject.DoStartScene)
            //    onStartScene -= sceneObject.OnStartScene;
            //if (sceneObject.DoChangeScene)
            //    onChangeScene -= sceneObject.OnChangeScene;
            //if (sceneObject.DoCompleteScene)
            //    onCompleteScene -= sceneObject.OnCompleteScene;
            //if (sceneObject.DoGameOver)
            //    onGameOver -= sceneObject.OnGameOver;
            //
            //if (sceneObject.DoUpdateScene)
            //    onSceneUpdate -= sceneObject.OnUpdateScene;
        }
        public static void StartScene()
        {
            foreach (var o in onStartSceneObjects)
                o.OnStartScene();

            //onStartScene?.Invoke();
        }
        public static void ChangeScene()
        {
            foreach (var o in onChangeSceneObjects)
                o.OnChangeScene();

            //onChangeScene?.Invoke();
        }
        public static void CompleteScene()
        {
            foreach (var o in onCompleteSceneObjects)
                o.OnCompleteScene();

            //onCompleteScene?.Invoke();
        }
        public static void GameOver()
        {
            foreach (var o in onGameOverObjects)
                o.OnGameOver();

            //onGameOver?.Invoke();
        }

        public static void UpdateScene(int frameIndex)
        {
            foreach (var o in onUpdateSceneObjects)
                o.OnUpdateScene(frameIndex);

            //onSceneUpdate?.Invoke(frameIndex);
        }
        #endregion

        #region Private Utility functions
        private static void Clear()
        {
            SceneVariables.Clear();
            ComplexSceneVariables.Clear();
            SceneVarLinks.Clear();
        }
        private static void AddVar(SceneVar variable)
        {
            SceneVariables[variable.uniqueID] = new(variable);
        }
        private static void AddComplexVar(ComplexSceneVar variable)
        {
            //SceneVar link = GetSceneVar(variable.uniqueID);
            ComplexSceneVariables[variable.uniqueID] = new(variable);
        }

        private static void SaveFormerValues()
        {
            if (SceneVariables == null) return;

            FormerValues.Clear();
            foreach (var pair in SceneVariables)
            {
                FormerValues[pair.Key] = pair.Value.Value;
            }
        }

        private static void ChangedVar(int varUniqueID, BaseSceneObject sender, SceneContext context)
        {
            if (SceneVariables.ContainsKey(varUniqueID))
            {
                SceneEventManager.TriggerEvent(varUniqueID, new(SceneVariables[varUniqueID], FormerValues[varUniqueID], sender, context));
            }
            CheckChangedLink(varUniqueID, sender, context);
        }
        internal static void CheckChangedLink(int varUniqueID, BaseSceneObject sender, SceneContext context)
        {
            if (SceneVarLinks.ContainsKey(varUniqueID))
            {
                foreach (var complexUID in SceneVarLinks[varUniqueID])
                {
                    ChangedComplexVar(complexUID, sender, context);
                }
            }
        }
        private static void ChangedComplexVar(int complexUID, BaseSceneObject sender, SceneContext context)
        {
            if (ComplexSceneVariables.ContainsKey(complexUID))
            {
                SceneEventManager.TriggerEvent(complexUID, new(SceneVariables[complexUID], FormerValues[complexUID], sender, context));
            }
        }
        #endregion

        #region Public accessors

        internal static Dictionary<int, SceneVar> GetCurrentSceneVars()
        {
            return new(SceneVariables);
        }

        internal static object GetObjectValue(int varUniqueID)
        {
            if (SceneVariables.ContainsKey(varUniqueID))
                return SceneVariables[varUniqueID].Value;

            IncorrectID(varUniqueID);
            return null;
        }

        internal static SceneVar GetSceneVar(int uniqueID)
        {
            if (IntersceneState.IsGlobalVar(uniqueID))
            {
                return IntersceneState.GetSceneVar(uniqueID);
            }

            if (SceneVariables.ContainsKey(uniqueID))
            {
                return new SceneVar(SceneVariables[uniqueID]);
            }
            else
            {
                Debug.LogError("Unique ID " + uniqueID + " can't be found in this scene SceneVariables");
                return null;
            }
        }
        internal static ComplexSceneVar GetComplexSceneVar(int uniqueID)
        {
            if (ComplexSceneVariables.ContainsKey(uniqueID))
            {
                return ComplexSceneVariables[uniqueID];
            }
            Debug.LogError("The ComplexSceneVar with UID : " + uniqueID + " doesn't exist");
            return null;
        }
        internal static object GetComplexSceneVarValue(int uniqueID)
        {
            if (ComplexSceneVariables.ContainsKey(uniqueID))
            {
                return ComplexSceneVariables[uniqueID].Value;
            }
            Debug.LogError("The ComplexSceneVar with UID : " + uniqueID + " doesn't exist");
            return null;
        }
        public static bool TryGetBoolValue(int varUniqueID, out bool value)
        {
            value = false;
            if (SceneVariables.ContainsKey(varUniqueID))
            {
                SceneVar sceneVar = SceneVariables[varUniqueID];
                if (sceneVar.type == SceneVarType.BOOL)
                {
                    value = sceneVar.BoolValue;
                    return true;
                }
                IncorrectType(varUniqueID, SceneVarType.BOOL);
                return false;
            }
            IncorrectID(varUniqueID);
            return false;
        }
        public static bool TryGetIntValue(int varUniqueID, out int value)
        {
            value = 0;
            if (SceneVariables.ContainsKey(varUniqueID))
            {
                SceneVar sceneVar = SceneVariables[varUniqueID];
                if (sceneVar.type == SceneVarType.INT)
                {
                    value = sceneVar.IntValue;
                    return true;
                }
                IncorrectType(varUniqueID, SceneVarType.INT);
                return false;
            }
            IncorrectID(varUniqueID);
            return false;
        }
        public static bool TryGetFloatValue(int varUniqueID, out float value)
        {
            value = 0f;
            if (SceneVariables.ContainsKey(varUniqueID))
            {
                SceneVar sceneVar = SceneVariables[varUniqueID];
                if (sceneVar.type == SceneVarType.FLOAT)
                {
                    value = sceneVar.FloatValue;
                    return true;
                }
                IncorrectType(varUniqueID, SceneVarType.FLOAT);
                return false;
            }
            IncorrectID(varUniqueID);
            return false;
        }
        public static bool TryGetStringValue(int varUniqueID, out string value)
        {
            value = null;
            if (SceneVariables.ContainsKey(varUniqueID))
            {
                SceneVar sceneVar = SceneVariables[varUniqueID];
                if (sceneVar.type == SceneVarType.STRING)
                {
                    value = sceneVar.StringValue;
                    return true;
                }
                IncorrectType(varUniqueID, SceneVarType.STRING);
                return false;
            }
            IncorrectID(varUniqueID);
            return false;
        }
        #endregion

        #region Public setters
        internal static void SetSceneVars(SceneVariablesSO sceneVariablesSO, int balancingIndex = 0)
        {
            Clear();
            if (sceneVariablesSO == null) return;

            List<SceneVar> sceneVars = sceneVariablesSO.BalancedSceneVars(balancingIndex);
            List<ComplexSceneVar> complexSceneVars = sceneVariablesSO.ComplexSceneVars;
            SetSceneVars(sceneVars);
            SetComplexSceneVars(complexSceneVars);
            SetSceneLinks();
            IntersceneState.SetGlobalVars(sceneVariablesSO.IntersceneVariables);
        }
        private static void SetSceneVars(List<SceneVar> sceneVars)
        {
            foreach (SceneVar sceneVar in sceneVars)
                AddVar(sceneVar);
        }
        private static void SetComplexSceneVars(List<ComplexSceneVar> complexSceneVars)
        {
            foreach (ComplexSceneVar var in complexSceneVars)
            {
                AddComplexVar(var);
            }
        }
        private static void SetSceneLinks()
        {
            // Browse on Complex Scene Vars
            foreach (var pair in ComplexSceneVariables)
            {
                // Get all dependencies of the Complex Scene Var
                foreach (var depUID in pair.Value.Dependencies)
                {
                    // Add a link from the dependency (SceneVar UID) to the dependant (Complex Scene Var)
                    if (!SceneVarLinks.ContainsKey(depUID))
                    {
                        SceneVarLinks[depUID] = new();
                    }
                    SceneVarLinks[depUID].Add(pair.Key);
                }
            }
        }

        internal static void ActuBalancing(SceneVariablesSO sceneVariablesSO, int balancingIndex)
        {
            foreach (var var in sceneVariablesSO.BalancedSceneVars(balancingIndex))
            {
                if (var.IsStatic || var.IsRandom)
                {
                    SceneVariables[var.uniqueID] = var;
                }
            }
        }

        internal static void ModifyBoolVar(int varUniqueID, BoolOperation op, bool param, BaseSceneObject sender, SceneContext context)
        {
            if (IntersceneState.IsGlobalVar(varUniqueID))
            {
                IntersceneState.ModifyBoolVar(varUniqueID, op, param, sender, context);
                return;
            }

            if (CanModifyVar(varUniqueID, SceneVarType.BOOL, out SceneVar var))
            {
                SaveFormerValues();
                if (CalculateBool(ref var, op, param))
                    ChangedVar(varUniqueID, sender, context);
                return;
            }
        }
        internal static void ModifyIntVar(int varUniqueID, IntOperation op, int param, BaseSceneObject sender, SceneContext context)
        {
            if (IntersceneState.IsGlobalVar(varUniqueID))
            {
                IntersceneState.ModifyIntVar(varUniqueID, op, param, sender, context);
                return;
            }

            if (CanModifyVar(varUniqueID, SceneVarType.INT, out SceneVar var))
            {
                SaveFormerValues();
                if (CalculateInt(ref var, op, param))
                    ChangedVar(varUniqueID, sender, context);
                return;
            }
        }
        internal static void ModifyFloatVar(int varUniqueID, FloatOperation op, float param, BaseSceneObject sender, SceneContext context)
        {
            if (IntersceneState.IsGlobalVar(varUniqueID))
            {
                IntersceneState.ModifyFloatVar(varUniqueID, op, param, sender, context);
                return;
            }

            if (CanModifyVar(varUniqueID, SceneVarType.FLOAT, out SceneVar var))
            {
                SaveFormerValues();
                if (CalculateFloat(ref var, op, param))
                    ChangedVar(varUniqueID, sender, context);
                return;
            }
        }
        internal static void ModifyStringVar(int varUniqueID, StringOperation op, string param, BaseSceneObject sender, SceneContext context)
        {
            if (IntersceneState.IsGlobalVar(varUniqueID))
            {
                IntersceneState.ModifyStringVar(varUniqueID, op, param, sender, context);
                return;
            }

            if (CanModifyVar(varUniqueID, SceneVarType.STRING, out SceneVar var))
            {
                SaveFormerValues();
                if (CalculateString(ref var, op, param))
                    ChangedVar(varUniqueID, sender, context);
                return;
            }
        }
        internal static void TriggerEventVar(int varUniqueID, BaseSceneObject sender, SceneContext context)
        {
            if (IntersceneState.IsGlobalVar(varUniqueID))
            {
                IntersceneState.TriggerEventVar(varUniqueID, sender, context);
                return;
            }

            if (CanModifyVar(varUniqueID, SceneVarType.EVENT, out SceneVar var))
            {
                SaveFormerValues();
                ChangedVar(varUniqueID, sender, context);
                return;
            }
        }
        #endregion

        #region Var Operations

        private static bool CanModifyVar(int uniqueID, SceneVarType type, out SceneVar var)
        {
            var = null;

            // Check if the UID is valid
            if (SceneVariables.ContainsKey(uniqueID))
            {
                var = SceneVariables[uniqueID];

                // Check if the type is valid
                if (var.type == type)
                {
                    // Check if the SceneVar is modifiable
                    bool canModify = var.CanModify;

                    if (!canModify)
                    {
                        Debug.LogError("Can't modify this SceneVar");
                    }

                    return canModify;
                }
                else
                {
                    IncorrectType(uniqueID, type);
                    return false;
                }
            }
            else
            {
                IncorrectID(uniqueID);
                return false;
            }
        }

        internal static bool CalculateBool(ref SceneVar var, BoolOperation op, bool param)
        {
            bool baseValue = var.BoolValue;
            switch (op)
            {
                case BoolOperation.SET:
                    var.BoolValue = param;
                    return baseValue != param;
                case BoolOperation.INVERSE:
                    var.BoolValue = !baseValue;
                    return true;
                case BoolOperation.TO_TRUE:
                    var.BoolValue = true;
                    return !baseValue;
                case BoolOperation.TO_FALSE:
                    var.BoolValue = false;
                    return baseValue;
                default:
                    var.BoolValue = param;
                    return baseValue != param;
            }
        }
        internal static bool CalculateInt(ref SceneVar var, IntOperation op, int param)
        {
            int baseValue = var.IntValue;
            bool result;

            switch (op)
            {
                case IntOperation.SET:
                    var.IntValue = param;
                    result = baseValue != param;
                    break;
                case IntOperation.ADD:
                    var.IntValue += param;
                    result = param != 0;
                    break;
                case IntOperation.SUBSTRACT:
                    var.IntValue -= param;
                    result = param != 0;
                    break;
                case IntOperation.MULTIPLY:
                    var.IntValue *= param;
                    result = param != 1;
                    break;
                case IntOperation.DIVIDE:
                    var.IntValue /= param;
                    result = param != 1;
                    break;
                case IntOperation.POWER:
                    var.IntValue = (int)Mathf.Pow(var.IntValue, param);
                    result = param != 1;
                    break;
                case IntOperation.TO_MIN:
                    if (!var.hasMin) return false;
                    var.IntValue = var.minInt;
                    result = baseValue != var.minInt;
                    break;
                case IntOperation.TO_MAX:
                    if (!var.hasMax) return false;
                    var.IntValue = var.maxInt;
                    result = baseValue != var.maxInt;
                    break;
                case IntOperation.TO_NULL:
                    var.IntValue = 0;
                    result = baseValue != 0;
                    break;
                case IntOperation.INCREMENT:
                    var.IntValue++;
                    result = true;
                    break;
                case IntOperation.DECREMENT:
                    var.IntValue--;
                    result = true;
                    break;

                default:
                    var.IntValue = param;
                    result = baseValue != param;
                    break;
            }

            if (var.hasMin || var.hasMax)
            {
                var.IntValue = (int)Mathf.Clamp(var.IntValue,
                    var.hasMin ? var.minInt : -Mathf.Infinity,
                    var.hasMax ? var.maxInt : Mathf.Infinity);
                result = var.IntValue != baseValue;
            }

            return result;
        }
        internal static bool CalculateFloat(ref SceneVar var, FloatOperation op, float param)
        {
            float baseValue = var.FloatValue;
            bool result;

            switch (op)
            {
                case FloatOperation.SET:
                    var.FloatValue = param;
                    result = baseValue != param;
                    break;
                case FloatOperation.ADD:
                    var.FloatValue += param;
                    result = param != 0;
                    break;
                case FloatOperation.SUBSTRACT:
                    var.FloatValue -= param;
                    result = param != 0;
                    break;
                case FloatOperation.MULTIPLY:
                    var.FloatValue *= param;
                    result = param != 1;
                    break;
                case FloatOperation.DIVIDE:
                    var.FloatValue /= param;
                    result = param != 1;
                    break;
                case FloatOperation.POWER:
                    var.FloatValue = Mathf.Pow(var.FloatValue, param);
                    result = param != 1;
                    break;
                case FloatOperation.TO_MIN:
                    if (!var.hasMin) return false;
                    var.FloatValue = var.minFloat;
                    result = baseValue != var.minFloat;
                    break;
                case FloatOperation.TO_MAX:
                    if (!var.hasMax) return false;
                    var.FloatValue = var.maxFloat;
                    result = baseValue != var.maxFloat;
                    break;
                case FloatOperation.TO_NULL:
                    var.FloatValue = 0;
                    result = baseValue != 0;
                    break;
                case FloatOperation.INCREMENT:
                    var.FloatValue++;
                    result = true;
                    break;
                case FloatOperation.DECREMENT:
                    var.FloatValue--;
                    result = true;
                    break;

                default:
                    var.FloatValue = param;
                    result = baseValue != param;
                    break;
            }

            if (var.hasMin || var.hasMax)
            {
                var.FloatValue = Mathf.Clamp(var.FloatValue,
                    var.hasMin ? var.minFloat : -Mathf.Infinity,
                    var.hasMax ? var.maxFloat : Mathf.Infinity);
                result = baseValue != var.FloatValue;
            }

            return result;
        }
        internal static bool CalculateString(ref SceneVar var, StringOperation op, string param)
        {
            string baseValue = var.StringValue;

            switch (op)
            {
                case StringOperation.SET:
                    var.StringValue = param;
                    return baseValue != param;
                case StringOperation.APPEND:
                    var.StringValue += param;
                    return !string.IsNullOrEmpty(param);
                case StringOperation.REMOVE:
                    var.StringValue.Replace(param, "");
                    return !string.IsNullOrEmpty(param);

                default:
                    var.StringValue = param;
                    return baseValue != param;
            }
        }

        #endregion

        #region Log
        internal static void IncorrectID(int UID)
        {
            Debug.LogError("Variable UID : '" + UID + "' doesn't exist in the current scene.");
        }
        internal static void IncorrectType(int UID, SceneVarType type)
        {
            Debug.LogError("Variable UID : '" + UID + "' is not of type : '" + type.ToString() + "'.");
        }
        #endregion

        #region Casts
        #region Cast To Bool
        public static bool CastToBool(SceneVar var)
        {
            switch (var.type)
            {
                case SceneVarType.BOOL:
                    return var.BoolValue;
                case SceneVarType.INT:
                    return var.IntValue != 0;
                case SceneVarType.FLOAT:
                    return var.FloatValue != 0;
                case SceneVarType.STRING:
                    return (var.StringValue.ToLower() == "true");
                default:
                    return false;
            }
        }
        #endregion

        #region Cast To Int
        public static int CastToInt(SceneVar var)
        {
            int i;
            switch (var.type)
            {
                case SceneVarType.INT:
                    return var.IntValue;
                case SceneVarType.FLOAT:
                    return (int)var.FloatValue;
                case SceneVarType.BOOL:
                    return var.BoolValue ? 1 : 0;
                case SceneVarType.STRING:
                    int.TryParse(var.StringValue, out i);
                    return i;
                default:
                    return 0;
            }
        }
        #endregion

        #region Cast To Float
        public static float CastToFloat(SceneVar var)
        {
            float f;
            switch (var.type)
            {
                case SceneVarType.FLOAT:
                    return var.FloatValue;
                case SceneVarType.INT:
                    return var.IntValue;
                case SceneVarType.BOOL:
                    return var.BoolValue ? 1f : 0f;
                case SceneVarType.STRING:
                    float.TryParse(var.StringValue, out f);
                    return f;
                default:
                    return 0f;
            }
        }
        #endregion

        #region Cast To String
        public static string CastToString(SceneVar var)
        {
            switch (var.type)
            {
                case SceneVarType.STRING:
                    return var.StringValue;
                case SceneVarType.BOOL:
                    return var.BoolValue.ToString();
                case SceneVarType.INT:
                    return var.IntValue.ToString();
                case SceneVarType.FLOAT:
                    return var.FloatValue.ToString();
                default:
                    return "";
            }
        }
        #endregion
        #endregion

        #region Extension Methods
        #region Utility
        public static bool IsValid<T>(this IList<T> list)
        {
            return list != null && list.Count > 0;
        }
        public static bool IsValid<T>(this T[] array)
        {
            return array != null && array.Length > 0;
        }
        public static bool IsValid<T>(this Stack<T> stack)
        {
            return stack != null && stack.Count > 0;
        }
        public static bool IsValid<T>(this Queue<T> queue)
        {
            return queue != null && queue.Count > 0;
        }
        public static bool IsValid<T, U>(this Dictionary<T, U> dico)
        {
            return dico != null && dico.Count > 0;
        }
        public static bool IsReallyValid<T, U>(this Dictionary<T, List<U>> dico)
        {
            if (dico == null || dico.Count <= 0) return false;
            foreach (var pair in dico)
            {
                if (pair.Value.IsValid()) return true;
            }
            return false;
        }

        public static bool IsIndexValid<T>(this List<T> list, int index)
        {
            return index >= 0 && index < list.Count;
        }

        public static List<T> Copy<T>(this List<T> list)
        {
            if (list == null) return null;
            return new(list);
        }
        #endregion

        #region Set Ups
        public interface ISceneVarSetupable
        {
            public void SetUp(SceneVariablesSO sceneVariablesSO);
        }
        public interface ISceneVarTypedSetupable
        {
            public void SetUp(SceneVariablesSO sceneVariablesSO, SceneVarType type);
        }
        
        public static void SetUp<T>(this IList<T> setupables, SceneVariablesSO sceneVariablesSO) where T : ISceneVarSetupable
        {
            if (setupables == null || setupables.Count < 1) return;

            foreach (var setupable in setupables)
            {
                setupable?.SetUp(sceneVariablesSO);
            }
        }
        public static void SetUp<T>(this IList<T> setupables, SceneVariablesSO sceneVariablesSO, SceneVarType type) where T : ISceneVarTypedSetupable
        {
            if (setupables == null || setupables.Count < 1) return;

            foreach (var setupable in setupables)
            {
                setupable?.SetUp(sceneVariablesSO, type);
            }
        }
        #endregion
        
        #region Belongs
        public interface ISceneObjectBelongable
        {
            public void BelongTo(BaseSceneObject _sceneObject);
        }
        
        public static void BelongTo<T>(this IList<T> belongables, BaseSceneObject sceneObject) where T : ISceneObjectBelongable
        {
            if (belongables == null || belongables.Count < 1) return;

            foreach (var belongable in belongables)
            {
                belongable.BelongTo(sceneObject);
            }
        }
        #endregion

        #region Inits
        public interface IInitializable
        {
            public void Init();
        }
        public static void Init<T>(this IList<T> initializables) where T : IInitializable
        {
            if (initializables == null || initializables.Count < 1) return;

            foreach (var initializable in initializables)
            {
                initializable.Init();
            }
        }
        #endregion

        #region Dependencies
        public interface ISceneVarDependant
        {
            public List<int> Dependencies { get; }
            public bool DependOn(int UID);
        }
        public interface ISceneVarDependantWithChild : ISceneVarDependant
        {
            public List<int> ChildDependencies();
        }
        public interface ISceneVarDependantWithProhibition : ISceneVarDependant
        {
            public void SetForbiddenUID(int UID);
        }
        public static List<int> Dependencies<T>(this IList<T> list) where T : ISceneVarDependant
        {
            if (!list.IsValid())
            {
                return new();
            }
            List<int> dependencies = new();
            List<int> temp;
            foreach (var dependant in list)
            {
                temp = dependant.Dependencies;
                if (temp.IsValid())
                {
                    foreach (var dep in temp)
                    {
                        if (!dependencies.Contains(dep))
                            dependencies.Add(dep);
                    }
                }
            }
            return dependencies;
        }
        public static bool DependOn<T>(this IList<T> list, int UID) where T : ISceneVarDependant
        {
            foreach (var dependant in list)
            {
                if (dependant.DependOn(UID))
                    return true;
            }
            return false;
        }
        public static void SetForbiddenUID<T>(this IList<T> list, int UID) where T : ISceneVarDependantWithProhibition
        {
            foreach (var dependant in list)
            {
                dependant.SetForbiddenUID(UID);
            }
        }
        #endregion

        #region Randoms

        public interface IPotentialRandom
        {
            public bool IsRandom { get; }
        }

        public static bool HasRandom<T>(this IList<T> randoms) where T : IPotentialRandom
        {
            if (!randoms.IsValid()) return false;

            foreach (var random in randoms)
            {
                if (random != null && random.IsRandom)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Log
        public interface ISceneLogable
        {
            public string Log(bool detailed = false, bool showEmpty = false)
            {
                StringBuilder sb = new();

                foreach (var l in LogLines(detailed, showEmpty))
                {
                    sb.Append(l);
                }

                return sb.ToString();
            }
            public List<string> LogLines(bool detailed = false, bool showEmpty = false, string alinea = null);

            /// <returns>Whether the <see cref="ISceneLogable"/> is empty</returns>
            public bool IsEmpty();
        }
        public interface ISceneLogableWithChild : ISceneLogable
        {
            public void ChildLog(List<string> lines, StringBuilder sb, bool detailed, bool showEmpty, string alinea = null);
            public bool IsChildEmpty();
        }

        public static bool IsEmpty<T>(this List<T> list) where T : ISceneLogable
        {
            if (!list.IsValid()) return true;

            foreach (var l in list)
            {
                if (!l.IsEmpty()) return false;
            }

            return true;
        }

        #endregion

        #region Scene Condition list verification (Extension Method)
        public static bool VerifyConditions(this IList<SceneCondition> conditions)
        {
            if (conditions == null || conditions.Count < 1) return true;
        
            bool result = conditions[0].VerifyCondition();
        
            for (int i = 1; i < conditions.Count; i++)
            {
                result = ApplyLogicOperator(result, conditions[i - 1].logicOperator, conditions[i].VerifyCondition());
            }
        
            return result;
        }
        private static bool ApplyLogicOperator(bool bool1, LogicOperator op, bool bool2)
        {
            switch (op)
            {
                case LogicOperator.AND: return bool1 & bool2;
                case LogicOperator.OR: return bool1 | bool2;
                case LogicOperator.NAND: return !(bool1 & bool2);
                case LogicOperator.NOR: return !(bool1 | bool2);
                case LogicOperator.XOR: return bool1 ^ bool2;
                case LogicOperator.XNOR: return !(bool1 ^ bool2);
                default: return true;
            }
        }
        #endregion
        
        #region Scene Total list evaluation (Extension Method)
        public static int EvaluateIntTotal(this List<SceneTotal> totals)
        {
            if (totals == null || totals.Count < 1) return 0;
        
            int result = (int)totals[0].Value;
        
            for (int i = 1; i < totals.Count; i++)
            {
                result = ApplyMathOperator(result, totals[i - 1].Op, (int)totals[i].Value);
            }
        
            return result;
        }
        public static float EvaluateFloatTotal(this List<SceneTotal> totals)
        {
            if (totals == null || totals.Count < 1) return 0f;
        
            float result = (float)totals[0].Value;
        
            for (int i = 1; i < totals.Count; i++)
            {
                result = ApplyMathOperator(result, totals[i - 1].Op, (float)totals[i].Value);
            }
        
            return result;
        }
        public static string EvaluateSentence(this List<SceneTotal> totals)
        {
            if (totals == null || totals.Count < 1) return "";

            StringBuilder sb = new StringBuilder();
            foreach (SceneTotal total in totals)
            {
                sb.Append(total.Value);
            }
            return sb.ToString();
        }
        private static int ApplyMathOperator(int int1, SceneTotal.Operator op, int int2)
        {
            switch (op)
            {
                case SceneTotal.Operator.ADD: return int1 + int2;
                case SceneTotal.Operator.SUBTRACT: return int1 - int2;
                case SceneTotal.Operator.MULTIPLY: return int1 * int2;
                case SceneTotal.Operator.DIVIDE: return int1 / int2;
                case SceneTotal.Operator.POWER: return (int)Mathf.Pow(int1, int2);
                default: return 0;
            }
        }
        private static float ApplyMathOperator(float float1, SceneTotal.Operator op, float float2)
        {
            switch (op)
            {
                case SceneTotal.Operator.ADD: return float1 + float2;
                case SceneTotal.Operator.SUBTRACT: return float1 - float2;
                case SceneTotal.Operator.MULTIPLY: return float1 * float2;
                case SceneTotal.Operator.DIVIDE: return float1 / float2;
                case SceneTotal.Operator.POWER: return Mathf.Pow(float1, float2);
                default: return 0;
            }
        }
        #endregion

        #region Random Scene Event triggering (Extension Method)
        public static bool TriggerRandom<T>(this List<T> sceneEvents, string filter = null, bool removeAfterTrigger = false) where T : BaseSceneEvent
        {
            if (sceneEvents == null || sceneEvents.Count < 1) return false;

            List<T> events = new();

            if (filter != null)
            {
                foreach (var sceneEvent in sceneEvents)
                    if (sceneEvent.eventID.Contains(filter))
                        events.Add(sceneEvent);
            }
            else
            {
                events = new(sceneEvents);
            }

            T ev;
            for (; events.Count > 0;)
            {
                ev = events[UnityEngine.Random.Range(0, events.Count)];
                if (ev.Trigger())
                {
                    if (removeAfterTrigger) sceneEvents.Remove(ev);
                    return true;
                }
                events.Remove(ev);
            }

            return false;
        }
        /// <summary>
        /// Triggers a random SceneEvent in the list
        /// </summary>
        /// <param name="sceneEvents"></param>
        /// <param name="filter">Trigger a random SceneEvent among ones which eventID contains filter</param>
        /// <returns>Whether an event was triggered</returns>
        public static bool TriggerRandom(this List<SceneEvent> sceneEvents, string filter = null, bool removeAfterTrigger = false)
        {
            if (sceneEvents == null || sceneEvents.Count < 1) return false;

            List<SceneEvent> events = new();

            if (filter != null)
            {
                foreach (SceneEvent sceneEvent in sceneEvents)
                    if (sceneEvent.eventID.Contains(filter))
                        events.Add(sceneEvent);
            }
            else
            {
                events = new(sceneEvents);
            }

            SceneEvent ev;
            for (;events.Count > 0;)
            {
                ev = events[UnityEngine.Random.Range(0, events.Count)];
                if (ev.Trigger())
                {
                    if (removeAfterTrigger) sceneEvents.Remove(ev);
                    return true;
                }
                events.Remove(ev);
            }

            return false;
        }
        /// <summary>
        /// Triggers a random SceneEvent in the list
        /// </summary>
        /// <param name="sceneEvents"></param>
        /// <param name="filter">Trigger a random SceneEvent among ones which eventID contains filter</param>
        /// <returns>Whether an event was triggered</returns>
        public static bool TriggerRandom<T>(this List<SceneEvent<T>> sceneEvents, T value = default, string filter = null, bool removeAfterTrigger = false)
        {
            if (sceneEvents == null || sceneEvents.Count < 1) return false;

            List<SceneEvent<T>> events = new();

            if (filter != null)
            {
                foreach (SceneEvent<T> sceneEvent in sceneEvents)
                    if (sceneEvent.eventID.Contains(filter))
                        events.Add(sceneEvent);
            }
            else
            {
                events = new(sceneEvents);
            }

            SceneEvent<T> ev;
            for (;events.Count > 0;)
            {
                ev = events[UnityEngine.Random.Range(0, events.Count)];
                if (ev.Trigger(value))
                {
                    if (removeAfterTrigger) sceneEvents.Remove(ev);
                    return true;
                }
                events.Remove(ev);
            }

            return false;
        }
        #endregion

        #region Trigger a list of SceneEvents (Extension Method)
        /// <summary>
        /// Triggers every <see cref="BaseSceneEvent"/> in <paramref name="sceneEvents"/>
        /// </summary>
        /// <typeparam name="T"><see cref="BaseSceneEvent"/></typeparam>
        /// <param name="sceneEvents">List of <see cref="BaseSceneEvent"/>s to trigger</param>
        public static void Trigger<T>(this List<T> sceneEvents) where T : BaseSceneEvent
        {
            if (!sceneEvents.IsValid()) return;

            foreach (var sceneEvent in sceneEvents)
            {
                sceneEvent?.Trigger();
                if (sceneEvent == null || sceneEvent.ReachedTriggerLimit)
                {
                    sceneEvents.Remove(sceneEvent);
                }
            }
        }
        /// <summary>
        /// Triggers every <see cref="BaseSceneEvent"/> with <see cref="BaseSceneEvent.eventID"/> = <paramref name="ID"/> in <paramref name="sceneEvents"/>
        /// </summary>
        /// <typeparam name="T"><see cref="BaseSceneEvent"/></typeparam>
        /// <param name="sceneEvents">List of <see cref="BaseSceneEvent"/>s to trigger</param>
        /// <param name="ID">ID of the <see cref="BaseSceneEvent"/>s to trigger</param>
        public static void TriggerWithID<T>(this List<T> sceneEvents, string ID) where T : BaseSceneEvent
        {
            if (!sceneEvents.IsValid()) return;

            ID ??= "";
            List<T> events = sceneEvents.FindAll(e => e.eventID == ID);

            if (!events.IsValid()) return;

            foreach (var sceneEvent in events)
            {
                sceneEvent?.Trigger();
                if (sceneEvent == null || sceneEvent.ReachedTriggerLimit)
                {
                    sceneEvents.Remove(sceneEvent);
                }
            }
        }
        /// <summary>
        /// Triggers every <see cref="SceneEvent{T}"/> in <paramref name="sceneEvents"/>
        /// </summary>
        /// <typeparam name="T"><see cref="SceneEvent{T}"/></typeparam>
        /// <param name="sceneEvents">List of <see cref="SceneEvent{T}"/>s to trigger</param>
        /// <param name="value">Value of the trigger paramater</param>
        public static void Trigger<T>(this List<SceneEvent<T>> sceneEvents, T value)
        {
            if (!sceneEvents.IsValid()) return;

            foreach (var sceneEvent in sceneEvents)
            {
                sceneEvent?.Trigger(value);
                if (sceneEvent == null || sceneEvent.ReachedTriggerLimit)
                {
                    sceneEvents.Remove(sceneEvent);
                }
            }
        }
        /// <summary>
        /// Triggers every <see cref="SceneEvent{T}"/> with <see cref="BaseSceneEvent.eventID"/> = <paramref name="ID"/> in <paramref name="sceneEvents"/>
        /// </summary>
        /// <typeparam name="T"><see cref="SceneEvent{T}"/></typeparam>
        /// <param name="sceneEvents">List of <see cref="SceneEvent{T}"/>s to trigger</param>
        /// <param name="value">Value of the trigger paramater</param>
        /// <param name="ID">ID of the <see cref="SceneEvent{T}"/>s to trigger</param>
        public static void TriggerWithID<T>(this List<SceneEvent<T>> sceneEvents, T value, string ID)
        {
            if (!sceneEvents.IsValid()) return;

            ID ??= "";
            List<SceneEvent<T>> events = sceneEvents.FindAll(e => e.eventID == ID);

            if (!events.IsValid()) return;

            foreach (var sceneEvent in events)
            {
                sceneEvent?.Trigger(value);
                if (sceneEvent == null || sceneEvent.ReachedTriggerLimit)
                {
                    sceneEvents.Remove(sceneEvent);
                }
            }
        }

        /// <summary>
        /// Triggers and remove every <see cref="BaseSceneEvent"/> in <paramref name="sceneEvents"/>
        /// </summary>
        /// <typeparam name="T"><see cref="BaseSceneEvent"/></typeparam>
        /// <param name="sceneEvents">List of <see cref="BaseSceneEvent"/>s to trigger</param>
        /// <param name="onlyIfTriggered">Whether to remove only triggered events or all of them</param>
        public static void TriggerAndRemove<T>(this List<T> sceneEvents, bool onlyIfTriggered) where T : BaseSceneEvent
        {
            if (!sceneEvents.IsValid()) return;

            foreach (var sceneEvent in sceneEvents.Copy())
            {
                if (sceneEvent == null || (sceneEvent.Trigger() && onlyIfTriggered))
                {
                    sceneEvents.Remove(sceneEvent);
                }
            }

            if (!onlyIfTriggered) sceneEvents.Clear();
        }
        /// <summary>
        /// Triggers and remove every <see cref="BaseSceneEvent"/> with <see cref="BaseSceneEvent.eventID"/> = <paramref name="ID"/> in <paramref name="sceneEvents"/>
        /// </summary>
        /// <typeparam name="T"><see cref="BaseSceneEvent"/></typeparam>
        /// <param name="sceneEvents">List of <see cref="BaseSceneEvent"/>s to trigger</param>
        /// <param name="ID">ID of the <see cref="BaseSceneEvent"/>s to trigger</param>
        /// <param name="onlyIfTriggered">Whether to remove only triggered events or all of them</param>
        public static void TriggerAndRemoveWithID<T>(this List<T> sceneEvents, string ID, bool onlyIfTriggered) where T : BaseSceneEvent
        {
            if (!sceneEvents.IsValid()) return;

            ID ??= "";
            List<T> events = sceneEvents.FindAll(e => e.eventID == ID);

            if (!events.IsValid()) return;

            foreach (var sceneEvent in events)
            {
                if (sceneEvent == null || sceneEvent.Trigger() || !onlyIfTriggered)
                {
                    sceneEvents.Remove(sceneEvent);
                }
            }
        }
        /// <summary>
        /// Triggers and remove every <see cref="SceneEvent{T}"/> in <paramref name="sceneEvents"/>
        /// </summary>
        /// <typeparam name="T"><see cref="SceneEvent{T}"/></typeparam>
        /// <param name="sceneEvents">List of <see cref="SceneEvent{T}"/>s to trigger</param>
        /// <param name="value">Value of the trigger paramater</param>
        /// <param name="onlyIfTriggered">Whether to remove only triggered events or all of them</param>
        public static void TriggerAndRemove<T>(this List<SceneEvent<T>> sceneEvents, T value, bool onlyIfTriggered)
        {
            if (!sceneEvents.IsValid()) return;

            foreach (var sceneEvent in sceneEvents)
            {
                if (sceneEvent == null || (sceneEvent.Trigger(value) && onlyIfTriggered))
                {
                    sceneEvents.Remove(sceneEvent);
                }
            }

            if (!onlyIfTriggered) sceneEvents.Clear();
        }
        /// <summary>
        /// Triggers and remove every <see cref="SceneEvent{T}"/> with <see cref="BaseSceneEvent.eventID"/> = <paramref name="ID"/> in <paramref name="sceneEvents"/>
        /// </summary>
        /// <typeparam name="T"><see cref="SceneEvent{T}"/></typeparam>
        /// <param name="sceneEvents">List of <see cref="SceneEvent{T}"/>s to trigger</param>
        /// <param name="value">Value of the trigger paramater</param>
        /// <param name="ID">ID of the <see cref="SceneEvent{T}"/>s to trigger</param>
        /// <param name="onlyIfTriggered">Whether to remove only triggered events or all of them</param>
        public static void TriggerAndRemoveWithID<T>(this List<SceneEvent<T>> sceneEvents, T value, string ID, bool onlyIfTriggered)
        {
            if (!sceneEvents.IsValid()) return;

            ID ??= "";
            List<SceneEvent<T>> events = sceneEvents.FindAll(e => e.eventID == ID);

            if (!events.IsValid()) return;

            foreach (var sceneEvent in events)
            {
                if (sceneEvent == null || sceneEvent.Trigger(value) || !onlyIfTriggered)
                {
                    sceneEvents.Remove(sceneEvent);
                }
            }
        }
        
        
        #endregion

        #region Trigger a list of SceneActions or SceneParameteredEvents (Extension Method)
        public static void Trigger(this IList<SceneAction> sceneActions, SceneContext context)
        {
            if (sceneActions == null || sceneActions.Count < 1) return;
            
            foreach (var action in sceneActions)
            {
                action.Trigger(context);
            }
        }
        public static void Trigger(this IList<SceneParameteredEvent> sceneEvents)
        {
            if (sceneEvents == null || sceneEvents.Count < 1) return;
            
            foreach (var action in sceneEvents)
            {
                action.Trigger();
            }
        }
        #endregion

        #region SceneListeners Registration
        public interface ISceneSubscribable
        {
            public void Subscribe();
            public void Unsubscribe();
        }
        public static void Subscribe<T>(this List<T> subscribables) where T : ISceneSubscribable
        {
            if (subscribables == null || subscribables.Count <= 0) return;

            foreach (var subscribable in subscribables)
            {
                subscribable.Subscribe();
            }
        }
        public static void Unsubscribe<T>(this List<T> subscribables) where T : ISceneSubscribable
        {
            if (subscribables == null || subscribables.Count <= 0) return;

            foreach (var subscribable in subscribables)
            {
                subscribable.Unsubscribe();
            }
        }
        #endregion

        #region SceneSpecificListeners Set Events
        /// <inheritdoc cref="SceneSpecificListener.SetEvents(Action{SceneEventParam})"/>
        public static void SetEvents(this List<SceneSpecificListener> listeners, Action<SceneEventParam> _events)
        {
            if (listeners == null || listeners.Count <= 0) return;

            foreach (var listener in listeners)
            {
                listener.SetEvents(_events);
            }
        }
        #endregion

        #region SceneObject Trigger
        public static void Trigger(this BaseSceneObject sceneObject, List<SceneListener.SceneEventTrigger> triggers, SceneEventParam param)
        {
            if (!triggers.IsValid()) return;

            foreach (var trigger in triggers)
            {
                sceneObject.Trigger(trigger, param);
            }
        }
        #endregion

        #region Profiles Management
        public static void Attach<T>(this List<T> list, SceneObject _sceneObject) where T : SceneProfile
        {
            if (list == null || list.Count <= 0) return;

            foreach (var item in list)
            {
                item.Attach(_sceneObject);
            }
        }
        public static void Detach<T>(this List<T> list) where T : SceneProfile
        {
            if (list == null || list.Count <= 0) return;

            foreach (var item in list)
            {
                item.Detach();
            }
        }
        #endregion

        #region SceneScriptableObjects management
        public static void Link<T>(this List<T> scriptables, BaseSceneObject sceneObject) where T : SceneScriptableObject
        {
            if (scriptables.IsValid())
            {
                foreach (var item in scriptables)
                    item.Link(sceneObject);
            }
        }
        public static void OnSceneObjectEnable<T>(this List<T> scriptables) where T : SceneScriptableObject
        {
            if (scriptables.IsValid())
            {
                foreach (var item in scriptables)
                    item.OnSceneObjectEnable();
            }
        }
        public static void OnSceneObjectDisable<T>(this List<T> scriptables) where T : SceneScriptableObject
        {
            if (scriptables.IsValid())
            {
                foreach (var item in scriptables)
                    item.OnSceneObjectDisable();
            }
        }
        #endregion

        #region Enum Second Parameter
        #region Operations
        public static bool HasSecondParameter(this BoolOperation boolOp)
        {
            return boolOp switch
            {
                BoolOperation.SET => true,
                BoolOperation.INVERSE => false,
                BoolOperation.TO_TRUE => false,
                BoolOperation.TO_FALSE => false,
                _ => false,
            };
        }
        public static bool HasSecondParameter(this IntOperation intOp)
        {
            return intOp switch
            {
                IntOperation.SET => true,
                IntOperation.ADD => true,
                IntOperation.SUBSTRACT => true,
                IntOperation.MULTIPLY => true,
                IntOperation.DIVIDE => true,
                IntOperation.POWER => true,
                IntOperation.TO_MIN => false,
                IntOperation.TO_MAX => false,
                IntOperation.TO_NULL => false,
                IntOperation.INCREMENT => false,
                IntOperation.DECREMENT => false,
                _ => false,
            };
        }
        public static bool HasSecondParameter(this FloatOperation floatOp)
        {
            return floatOp switch
            {
                FloatOperation.SET => true,
                FloatOperation.ADD => true,
                FloatOperation.SUBSTRACT => true,
                FloatOperation.MULTIPLY => true,
                FloatOperation.DIVIDE => true,
                FloatOperation.POWER => true,
                FloatOperation.TO_MIN => false,
                FloatOperation.TO_MAX => false,
                FloatOperation.TO_NULL => false,
                FloatOperation.INCREMENT => false,
                FloatOperation.DECREMENT => false,
                _ => false,
            };
        }
        #endregion

        #region Comparisons
        public static bool HasSecondParameter(this BoolComparison boolComp)
        {
            return boolComp switch
            { 
                BoolComparison.EQUAL => true,
                BoolComparison.DIFF => true,
                BoolComparison.IS_TRUE => false,
                BoolComparison.IS_FALSE => false,
                _ => false,
            };
        }
        public static bool HasSecondParameter(this IntComparison intComp)
        {
            return intComp switch
            { 
                IntComparison.EQUAL => true,
                IntComparison.DIFF => true,
                IntComparison.SUP => true,
                IntComparison.INF => true,
                IntComparison.SUP_EQUAL => true,
                IntComparison.INF_EQUAL => true,
                IntComparison.IS_MIN => false,
                IntComparison.IS_MAX => false,
                IntComparison.IS_NULL => false,
                IntComparison.IS_POSITIVE => false,
                IntComparison.IS_NEGATIVE => false,
                _ => false,
            };
        }
        public static bool HasSecondParameter(this FloatComparison floatComp)
        {
            return floatComp switch
            { 
                FloatComparison.EQUAL => true,
                FloatComparison.DIFF => true,
                FloatComparison.SUP => true,
                FloatComparison.INF => true,
                FloatComparison.SUP_EQUAL => true,
                FloatComparison.INF_EQUAL => true,
                FloatComparison.IS_MIN => false,
                FloatComparison.IS_MAX => false,
                FloatComparison.IS_NULL => false,
                FloatComparison.IS_POSITIVE => false,
                FloatComparison.IS_NEGATIVE => false,
                _ => false,
            };
        }
        public static bool HasSecondParameter(this StringComparison stringComp)
        {
            return stringComp switch
            { 
                StringComparison.EQUAL => true,
                StringComparison.DIFF => true,
                StringComparison.CONTAINS => true,
                StringComparison.CONTAINED => true,
                StringComparison.NULL_EMPTY => false,
                _ => false,
            };
        }
        #endregion
        #endregion

        #region Enum Description
        #region Comparisons
        public static string Description(this BoolComparison comp)
        {
            switch (comp)
            {
                case BoolComparison.EQUAL: return " == ";
                case BoolComparison.DIFF: return " != ";
                case BoolComparison.IS_TRUE: return " == True";
                case BoolComparison.IS_FALSE: return " == False";
                default: return "";
            }
        }
        public static string Description(this IntComparison comp)
        {
            switch (comp)
            {
                case IntComparison.EQUAL: return " == ";
                case IntComparison.DIFF: return " != ";
                case IntComparison.SUP: return " > ";
                case IntComparison.INF: return " < ";
                case IntComparison.SUP_EQUAL: return " >= ";
                case IntComparison.INF_EQUAL: return " <= ";
                case IntComparison.IS_MIN: return " == min";
                case IntComparison.IS_MAX: return " == max";
                case IntComparison.IS_NULL: return " == 0";
                case IntComparison.IS_POSITIVE: return " > 0";
                case IntComparison.IS_NEGATIVE: return " < 0";
                default: return "";
            }
        }
        public static string Description(this FloatComparison comp)
        {
            switch (comp)
            {
                case FloatComparison.EQUAL: return " == ";
                case FloatComparison.DIFF: return " != ";
                case FloatComparison.SUP: return " > ";
                case FloatComparison.INF: return " < ";
                case FloatComparison.SUP_EQUAL: return " >= ";
                case FloatComparison.INF_EQUAL: return " <= ";
                case FloatComparison.IS_MIN: return " == min ";
                case FloatComparison.IS_MAX: return " == max ";
                case FloatComparison.IS_NULL: return " == 0";
                case FloatComparison.IS_POSITIVE: return " > 0";
                case FloatComparison.IS_NEGATIVE: return " < 0";
                default: return "";
            }
        }
        public static string Description(this StringComparison comp)
        {
            switch (comp)
            {
                case StringComparison.EQUAL: return " == ";
                case StringComparison.DIFF: return " != ";
                case StringComparison.CONTAINS: return " Contains : ";
                case StringComparison.CONTAINED: return " Contained in : ";
                case StringComparison.NULL_EMPTY: return " is null or empty. ";
                default: return "";
            }
        }
        #endregion

        #region Operations
        public static string Description(this BoolOperation op)
        {
            switch (op)
            {
                case BoolOperation.SET: return " = ";
                case BoolOperation.INVERSE: return " Inverse.";
                case BoolOperation.TO_TRUE: return " = True";
                case BoolOperation.TO_FALSE: return " = False";
                default: return "";
            }
        }
        public static string Description(this IntOperation op)
        {
            switch (op)
            {
                case IntOperation.SET: return " = ";
                case IntOperation.ADD: return " += ";
                case IntOperation.SUBSTRACT: return " -= ";
                case IntOperation.MULTIPLY: return " *= ";
                case IntOperation.DIVIDE: return " /= ";
                case IntOperation.POWER: return " = power ";
                case IntOperation.TO_MIN: return " = min ";
                case IntOperation.TO_MAX: return " = max ";
                case IntOperation.TO_NULL: return " = 0 ";
                case IntOperation.INCREMENT: return " ++";
                case IntOperation.DECREMENT: return " --";
                default: return "";
            }
        }
        public static string Description(this FloatOperation op)
        {
            switch (op)
            {
                case FloatOperation.SET: return " = ";
                case FloatOperation.ADD: return " += ";
                case FloatOperation.SUBSTRACT: return " -= ";
                case FloatOperation.MULTIPLY: return " *= ";
                case FloatOperation.DIVIDE: return " /= ";
                case FloatOperation.POWER: return " = power ";
                case FloatOperation.TO_MIN: return " = min ";
                case FloatOperation.TO_MAX: return " = max ";
                case FloatOperation.TO_NULL: return " = 0 ";
                case FloatOperation.INCREMENT: return " ++";
                case FloatOperation.DECREMENT: return " --";
                default: return "";
            }
        }
        public static string Description(this StringOperation op)
        {
            switch (op)
            {
                case StringOperation.SET: return " = ";
                case StringOperation.APPEND: return " .Append ";
                case StringOperation.REMOVE: return " .Replace(param,'') ";
                default: return "";
            }
        }
        #endregion
        #endregion

        #region SceneVar List Methods

        public static List<string> VarStrings(this List<SceneVar> vars)
        {
            return BaseVariablesContainer.VarStrings(vars);
        }
        public static int GetUniqueIDByIndex(this List<SceneVar> vars, int index)
        {
            return BaseVariablesContainer.GetUniqueIDByIndex(vars, index);
        }
        public static int GetIndexByUniqueID(this List<SceneVar> vars, int uniqueID)
        {
            return BaseVariablesContainer.GetIndexByUniqueID(vars, uniqueID);
        }

        #endregion
        #endregion
    }
}
