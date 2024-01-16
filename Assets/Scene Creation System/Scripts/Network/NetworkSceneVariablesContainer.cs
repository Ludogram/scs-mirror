using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    public class NetworkSceneVariablesContainer : NetworkBehaviour
    {
        private Dictionary<int, SceneVar> SceneVariables = new();
        private Dictionary<int, ComplexSceneVar> ComplexSceneVariables = new();
        private Dictionary<int, List<int>> SceneVarLinks = new();
        private Dictionary<int, object> FormerValues = new();

        #region Scene Var Management

        public void Clear()
        {
            SceneVariables.Clear();
            ComplexSceneVariables.Clear();
            SceneVarLinks.Clear();
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
    }
}
