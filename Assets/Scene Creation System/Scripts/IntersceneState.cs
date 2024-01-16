using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    public static class IntersceneState
    {
        #region Global SceneVars

        public static bool IsEmpty { get; private set; } = true;

        private static Dictionary<int, SceneVar> IntersceneVariables = new();

        private static Dictionary<int, object> FormerValues = new();

        public static void SetGlobalVars(IntersceneVariablesSO intersceneVariablesSO)
        {
            if (!IsEmpty || intersceneVariablesSO == null) return;

            IsEmpty = false;

            SetIntersceneVars(intersceneVariablesSO.SceneVars);
        }

        private static void SetIntersceneVars(List<SceneVar> globalVars)
        {
            if (globalVars.IsValid())
                foreach (var var in globalVars)
                    AddGlobalVar(var);
        }

        private static void AddGlobalVar(SceneVar variable)
        {
            IntersceneVariables[variable.uniqueID] = new(variable);
        }

        internal static bool IsGlobalVar(int uniqueID)
        {
            return uniqueID > 10000;
        }

        #endregion

        #region Global Vars Actions

        private static void SaveFormerValues()
        {
            if (!IntersceneVariables.IsValid()) return;

            FormerValues.Clear();
            foreach (var pair in IntersceneVariables)
            {
                FormerValues[pair.Key] = pair.Value.Value;
            }
        }

        private static void ChangedVar(int varUniqueID, BaseSceneObject sender, SceneContext context)
        {
            if (IntersceneVariables.ContainsKey(varUniqueID))
            {
                SceneEventManager.TriggerEvent(varUniqueID, new(IntersceneVariables[varUniqueID], FormerValues[varUniqueID], sender, context));
            }
            SceneState.CheckChangedLink(varUniqueID, sender, context);
        }

        internal static void ModifyBoolVar(int varUniqueID, BoolOperation op, bool param, BaseSceneObject sender, SceneContext context)
        {
            if (CanModifyVar(varUniqueID, SceneVarType.BOOL, out SceneVar var))
            {
                SaveFormerValues();
                if (SceneState.CalculateBool(ref var, op, param))
                    ChangedVar(varUniqueID, sender, context);
                return;
            }
        }
        internal static void ModifyIntVar(int varUniqueID, IntOperation op, int param, BaseSceneObject sender, SceneContext context)
        {
            if (CanModifyVar(varUniqueID, SceneVarType.INT, out SceneVar var))
            {
                SaveFormerValues();
                if (SceneState.CalculateInt(ref var, op, param))
                    ChangedVar(varUniqueID, sender, context);
                return;
            }
        }
        internal static void ModifyFloatVar(int varUniqueID, FloatOperation op, float param, BaseSceneObject sender, SceneContext context)
        {
            if (CanModifyVar(varUniqueID, SceneVarType.FLOAT, out SceneVar var))
            {
                SaveFormerValues();
                if (SceneState.CalculateFloat(ref var, op, param))
                    ChangedVar(varUniqueID, sender, context);
                return;
            }
        }
        internal static void ModifyStringVar(int varUniqueID, StringOperation op, string param, BaseSceneObject sender, SceneContext context)
        {
            if (CanModifyVar(varUniqueID, SceneVarType.STRING, out SceneVar var))
            {
                SaveFormerValues();
                if (SceneState.CalculateString(ref var, op, param))
                    ChangedVar(varUniqueID, sender, context);
                return;
            }
        }
        internal static void TriggerEventVar(int varUniqueID, BaseSceneObject sender, SceneContext context)
        {
            if (CanModifyVar(varUniqueID, SceneVarType.EVENT, out SceneVar var))
            {
                SaveFormerValues();
                ChangedVar(varUniqueID, sender, context);
                return;
            }
        }

        private static bool CanModifyVar(int uniqueID, SceneVarType type, out SceneVar var)
        {
            var = null;

            // Check if the UID is valid
            if (IntersceneVariables.ContainsKey(uniqueID))
            {
                var = IntersceneVariables[uniqueID];

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
                    SceneState.IncorrectType(uniqueID, type);
                    return false;
                }
            }
            else
            {
                SceneState.IncorrectID(uniqueID);
                return false;
            }
        }

        #endregion

        #region Global Vars Getters

        internal static SceneVar GetSceneVar(int uniqueID)
        {
            if (IsEmpty)
            {
                Debug.LogError("The IntersceneState is empty");
                return null;
            }

            if (IntersceneVariables.ContainsKey(uniqueID))
            {
                return new(IntersceneVariables[uniqueID]);
            }
            else
            {
                Debug.LogError("Unique ID " + uniqueID + " can't be found in the Global SceneVariables");
                return null;
            }
        }

        #endregion

        #region Main Variables

        /// <summary>
        /// Current level of balancing of the game
        /// </summary>
        public static int BalancingLevel { get; private set; }

        #endregion
    }
}
