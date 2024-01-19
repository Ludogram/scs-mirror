using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class NetworkSceneVariablesContainer : NetworkBehaviour
    {
        #region Singleton

        private static NetworkSceneVariablesContainer instance;
        public static NetworkSceneVariablesContainer Instance
        {
            get
            {
                if (instance == null) instance = FindObjectOfType<NetworkSceneVariablesContainer>();
                if (instance == null) Debug.LogError("Can't find any NetworkSceneVariablesContainer in the scene");
                return instance;
            }
            private set => instance = value;
        }

        private void Awake()
        {
            if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            Init();
        }

        #endregion

        #region Scene Managers Registration

        public static void Register(SceneManager sceneManager)
        {
            if (Instance.sceneManagers.Contains(sceneManager))
            {
                Debug.LogError(sceneManager + " is already registered", Instance);
                return;
            }
            Instance.sceneManagers.Add(sceneManager);

            if (Instance.netIdentity.clientStarted)
            {
                sceneManager.StartNetworkScene();
            }
        }
        public static void Unregister(SceneManager sceneManager)
        {
            if (Instance.sceneManagers.Contains(sceneManager))
            {
                Instance.sceneManagers.Remove(sceneManager);

                foreach (var sv in sceneManager.SceneVariablesSO.SceneVars)
                {
                    Instance.RemoveVar(sv.uniqueID);
                }
            }
        }

        #endregion

        private List<SceneManager> sceneManagers = new();

        private VarNetworkDictionnary SceneVariables = new();
        private Dictionary<int, ComplexSceneVar> ComplexSceneVariables = new();
        private Dictionary<int, List<int>> SceneVarLinks = new();
        private Dictionary<int, object> FormerValues = new();

        private void Init()
        {
            
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            SceneVariables.Callback += OnStateChange;

            foreach (var sceneManager in sceneManagers)
                sceneManager.StartNetworkScene();
        }

        private void OnStateChange(VarNetworkDictionnary.Operation op, int key, SceneVar item)
        {
            //TODO: Idk but this Debug.Log is flooding the console
            Debug.Log("OnStateChange: op: " + op + ", key: " + key + ", item: " + (item != null ? item.RuntimeString() : "null"));
        }

        #region Scene Var Management

        private void RemoveVar(int uniqueID)
        {
            if (SceneVariables.ContainsKey(uniqueID))
            {
                SceneVariables.Remove(uniqueID);
            }
            if (SceneVarLinks.ContainsKey(uniqueID))
            {
                foreach (var complexUID in SceneVarLinks[uniqueID])
                {
                    RemoveComplexVar(complexUID);
                }
                SceneVarLinks.Remove(uniqueID);
            }
        }
        private void RemoveComplexVar(int uniqueID)
        {
            if (ComplexSceneVariables.ContainsKey(uniqueID))
            {
                ComplexSceneVariables.Remove(uniqueID);
            }
        }

        public void AddVar(SceneVar variable)
        {
            SceneVariables[variable.uniqueID] = new(variable);
        }
        public void AddComplexVar(ComplexSceneVar variable)
        {
            ComplexSceneVariables[variable.uniqueID] = new(variable);
        }

        public void SaveFormerValues()
        {
            if (SceneVariables == null) return;

            FormerValues.Clear();
            foreach (var pair in SceneVariables)
            {
                FormerValues[pair.Key] = pair.Value.Value;
            }
        }

        public void ChangedVar(int varUniqueID, BaseSceneObject sender, SceneContext context)
        {
            if (SceneVariables.ContainsKey(varUniqueID))
            {
                SceneEventManager.TriggerEvent(varUniqueID, new(SceneVariables[varUniqueID], FormerValues[varUniqueID], sender, context));
            }
            CheckChangedLink(varUniqueID, sender, context);
        }
        public void CheckChangedLink(int varUniqueID, BaseSceneObject sender, SceneContext context)
        {
            if (SceneVarLinks.ContainsKey(varUniqueID))
            {
                foreach (var complexUID in SceneVarLinks[varUniqueID])
                {
                    ChangedComplexVar(complexUID, sender, context);
                }
            }
        }
        public void ChangedComplexVar(int complexUID, BaseSceneObject sender, SceneContext context)
        {
            if (ComplexSceneVariables.ContainsKey(complexUID))
            {
                SceneEventManager.TriggerEvent(complexUID, new(SceneVariables[complexUID], FormerValues[complexUID], sender, context));
            }
        }


        public void SetSceneLinks()
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

        public void ActuBalancing(SceneVariablesSO sceneVariablesSO, int balancingIndex)
        {
            foreach (var var in sceneVariablesSO.BalancedSceneVars(balancingIndex))
            {
                if (var.IsStatic || var.IsRandom)
                {
                    SceneVariables[var.uniqueID] = var;
                }
            }
        }

        #endregion

        #region Public accessors

        public Dictionary<int, SceneVar> GetCurrentSceneVars()
        {
            return new(SceneVariables);
        }

        public object GetObjectValue(int varUniqueID)
        {
            if (SceneVariables.ContainsKey(varUniqueID))
                return SceneVariables[varUniqueID].Value;

            IncorrectID(varUniqueID);
            return null;
        }

        public SceneVar GetSceneVar(int uniqueID)
        {
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
        internal void SetSceneVar(int varUniqueID, object value)
        { 
            if (SceneVariables.ContainsKey(varUniqueID))
            {
                SceneVar sVar = SceneVariables[varUniqueID];

                switch (sVar.type)
                {
                    case SceneVarType.BOOL: sVar.BoolValue = (bool)value; break;
                    case SceneVarType.INT: sVar.IntValue = (int)value; break;
                    case SceneVarType.FLOAT: sVar.FloatValue = (float)value; break;
                    case SceneVarType.STRING: sVar.StringValue = (string)value; break;
                }
            }
        }
        public ComplexSceneVar GetComplexSceneVar(int uniqueID)
        {
            if (ComplexSceneVariables.ContainsKey(uniqueID))
            {
                return ComplexSceneVariables[uniqueID];
            }
            Debug.LogError("The ComplexSceneVar with UID : " + uniqueID + " doesn't exist");
            return null;
        }
        public object GetComplexSceneVarValue(int uniqueID)
        {
            if (ComplexSceneVariables.ContainsKey(uniqueID))
            {
                return ComplexSceneVariables[uniqueID].Value;
            }
            Debug.LogError("The ComplexSceneVar with UID : " + uniqueID + " doesn't exist");
            return null;
        }
        public bool TryGetBoolValue(int varUniqueID, out bool value)
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
        public bool TryGetIntValue(int varUniqueID, out int value)
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
        public bool TryGetFloatValue(int varUniqueID, out float value)
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
        public bool TryGetStringValue(int varUniqueID, out string value)
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

        #region Modify Vars

        public void ModifyBoolVar(int varUniqueID, BoolOperation op, bool param, BaseSceneObject sender, SceneContext context)
        {
            if (CanModifyVar(varUniqueID, SceneVarType.BOOL, out SceneVar var))
            {
                SaveFormerValues();
                if (CalculateBool(ref var, op, param))
                {
                    SceneVariables.Set(varUniqueID, var, sender);
                    //SceneVariables[varUniqueID] = var;
                    ChangedVar(varUniqueID, sender, context);
                }
                return;
            }
        }
        public void ModifyIntVar(int varUniqueID, IntOperation op, int param, BaseSceneObject sender, SceneContext context)
        {
            if (CanModifyVar(varUniqueID, SceneVarType.INT, out SceneVar var))
            {
                SaveFormerValues();
                if (CalculateInt(varUniqueID, ref var, op, param))
                {
                    SceneVariables.Set(varUniqueID, var, sender);
                    ChangedVar(varUniqueID, sender, context);
                }
                return;
            }
        }
        public void ModifyFloatVar(int varUniqueID, FloatOperation op, float param, BaseSceneObject sender, SceneContext context)
        {
            if (CanModifyVar(varUniqueID, SceneVarType.FLOAT, out SceneVar var))
            {
                SaveFormerValues();
                if (CalculateFloat(ref var, op, param))
                {
                    SceneVariables.Set(varUniqueID, var, sender);
                    ChangedVar(varUniqueID, sender, context);
                }
                return;
            }
        }
        public void ModifyStringVar(int varUniqueID, StringOperation op, string param, BaseSceneObject sender, SceneContext context)
        {
            if (CanModifyVar(varUniqueID, SceneVarType.STRING, out SceneVar var))
            {
                SaveFormerValues();
                if (CalculateString(ref var, op, param))
                {
                    SceneVariables.Set(varUniqueID, var, sender);
                    ChangedVar(varUniqueID, sender, context);
                }
                return;
            }
        }
        public void TriggerEventVar(int varUniqueID, BaseSceneObject sender, SceneContext context)
        {
            if (CanModifyVar(varUniqueID, SceneVarType.EVENT, out SceneVar var))
            {
                SaveFormerValues();
                SceneVariables.Set(varUniqueID, var, sender);
                ChangedVar(varUniqueID, sender, context);
                return;
            }
        }

        #endregion

        #region Operations

        public bool CanModifyVar(int uniqueID, SceneVarType type, out SceneVar var)
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

        private bool CalculateBool(ref SceneVar var, BoolOperation op, bool param)
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
        private bool CalculateInt(int UID, ref SceneVar var, IntOperation op, int param)
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

            //if (result) SceneVariables[UID] = var;

            return result;
        }
        private bool CalculateFloat(ref SceneVar var, FloatOperation op, float param)
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
        private bool CalculateString(ref SceneVar var, StringOperation op, string param)
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

        #region Debug

        public void DebugSceneVariables()
        {
            foreach (var kvp in SceneVariables)
            {
                Debug.Log(kvp.Value.RuntimeString());
            }
        }

        #endregion
    }
}
