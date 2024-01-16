using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace Dhs5.SceneCreation
{
    [Serializable]
    public class SceneAction : SceneState.ISceneVarSetupable, SceneState.ISceneObjectBelongable, SceneState.ISceneVarDependant
    {
        public SceneVariablesSO sceneVariablesSO;
        private BaseSceneObject sceneObject;

        [SerializeField] private int var1UniqueID;
        public SceneVar SceneVar1 { get => SceneState.GetSceneVar(var1UniqueID); }
        private SceneVar EditorSceneVar1 { get => sceneVariablesSO[var1UniqueID]; }

        [SerializeField] private SceneVarTween SceneVar2;
        [SerializeField] private SceneVarType var2Type;

        // Operations        
        public BoolOperation boolOP;
        
        public IntOperation intOP;
        
        public FloatOperation floatOP;
        
        public StringOperation stringOP;

        public void SetUp(SceneVariablesSO sceneVariablesSO)
        {
            this.sceneVariablesSO = sceneVariablesSO;

            SceneVar2.SetUp(sceneVariablesSO, var2Type, true);
        }
        public void BelongTo(BaseSceneObject _sceneObject)
        {
            sceneObject = _sceneObject;
        }
        
        public void Trigger(SceneContext context)
        {
            if (SceneVar1 == null)
            {
                Debug.LogError("Can't trigger a SceneAction on a null SceneVar");
                return;
            }

            switch (SceneVar1.type)
            {
                case SceneVarType.BOOL:
                    SceneState.ModifyBoolVar(var1UniqueID, boolOP, SceneVar2.BoolValue, sceneObject, context.Add(SceneVar1.RuntimeString(), boolOP.Description(), SceneVar2.BoolValue.ToString()));
                    break;
                case SceneVarType.INT:
                    SceneState.ModifyIntVar(var1UniqueID, intOP, SceneVar2.IntValue, sceneObject, context.Add(SceneVar1.RuntimeString(), intOP.Description(), SceneVar2.IntValue.ToString()));
                    break;
                case SceneVarType.FLOAT:
                    SceneState.ModifyFloatVar(var1UniqueID, floatOP, SceneVar2.FloatValue, sceneObject, context.Add(SceneVar1.RuntimeString(), floatOP.Description(), SceneVar2.FloatValue.ToString()));
                    break;
                case SceneVarType.STRING:
                    SceneState.ModifyStringVar(var1UniqueID, stringOP, SceneVar2.StringValue, sceneObject, context.Add(SceneVar1.RuntimeString(), stringOP.Description(), SceneVar2.StringValue));
                    break;
                case SceneVarType.EVENT:
                    SceneState.TriggerEventVar(var1UniqueID, sceneObject, context.Add(SceneVar1.RuntimeString(), " Trigger"));
                    break;

                default:
                    break;
            }
        }

        #region Operation Description
        
        private string GetOpDescription()
        {
            switch (EditorSceneVar1.type)
            {
                case SceneVarType.BOOL: return boolOP.Description();
                case SceneVarType.INT: return intOP.Description();
                case SceneVarType.FLOAT: return floatOP.Description();
                case SceneVarType.STRING: return stringOP.Description();
                case SceneVarType.EVENT: return "Trigger";
                default: return null;
            }
        }
        private bool HasSecondParameter()
        {
            switch (EditorSceneVar1.type)
            {
                case SceneVarType.BOOL: return boolOP.HasSecondParameter();
                case SceneVarType.INT: return intOP.HasSecondParameter();
                case SceneVarType.FLOAT: return floatOP.HasSecondParameter();
                case SceneVarType.STRING: return true;
                case SceneVarType.EVENT: return false;
                default: return false;
            }
        }
        #endregion

        #region Log
        public override string ToString()
        {
            StringBuilder sb = new();

            sb.Append(EditorSceneVar1.LogString());
            sb.Append(" ");
            sb.Append(GetOpDescription());
            if (HasSecondParameter())
            {
                sb.Append(" ");
                sb.Append(SceneVar2.LogString());
            }

            return sb.ToString();
        }
        #endregion

        #region Dependencies
        public List<int> Dependencies 
        {
            get
            {
                List<int> dependencies = new() { var1UniqueID };
                if (HasSecondParameter())
                {
                    dependencies.AddRange(SceneVar2.Dependencies);
                }
                return dependencies;
            }
        }
        public bool DependOn(int UID) { return Dependencies.Contains(UID); }
        public void SetForbiddenUID(int UID) { }
        #endregion
    }
}
