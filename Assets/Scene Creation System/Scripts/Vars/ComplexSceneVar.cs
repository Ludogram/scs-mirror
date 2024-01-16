using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    [System.Serializable]
    public enum ComplexSceneVarType
    {
        CONDITION = 0,
        TOTAL_INT = 1,
        TOTAL_FLOAT = 2,
        SENTENCE = 3,
    }

    [System.Serializable]
    public class ComplexSceneVar : SceneState.ISceneVarSetupable, SceneState.ISceneVarDependantWithProhibition, SceneState.IPotentialRandom
    {
        [SerializeField] private SceneVariablesSO sceneVariablesSO;

        #region SetUp
        public void SetUp(SceneVariablesSO _sceneVariablesSO)
        {
            sceneVariablesSO = _sceneVariablesSO;

            conditions.SetUp(sceneVariablesSO);
            intTotals.SetUp(sceneVariablesSO, SceneVarType.INT);
            floatTotals.SetUp(sceneVariablesSO, SceneVarType.FLOAT);
            sentences.SetUp(sceneVariablesSO, SceneVarType.STRING);

            UpdateLinkInfo();
        }
        private void UpdateLinkInfo()
        {
            if (Link != null)
            {
                Link.uniqueID = uniqueID;
                Link.ID = ID;
                Link.type = BaseType;
            }
        }
        #endregion

        public ComplexSceneVar(int _uniqueID, ComplexSceneVarType _type)
        {
            uniqueID = _uniqueID;
            ID = "New Complex Var";
            type = _type;

            conditions = new();
            intTotals = new();
            floatTotals = new();
            sentences = new();

            //Link = SceneVar.CreateLink(this);
        }

        public ComplexSceneVar(ComplexSceneVar var)
        {
            uniqueID = var.uniqueID;
            ID = var.ID;
            type = var.type;

            conditions = new(var.conditions);
            intTotals = new(var.intTotals);
            floatTotals = new(var.floatTotals);
            sentences = new(var.sentences);

            //Link = var.Link;
        }

        public int uniqueID = 0;

        public string ID;
        public ComplexSceneVarType type;
        public SceneVarType BaseType
        {
            get
            {
                switch (type)
                {
                    case ComplexSceneVarType.CONDITION: return SceneVarType.BOOL;
                    case ComplexSceneVarType.TOTAL_INT: return SceneVarType.INT;
                    case ComplexSceneVarType.TOTAL_FLOAT: return SceneVarType.FLOAT;
                    case ComplexSceneVarType.SENTENCE: return SceneVarType.STRING;
                    default: return SceneVarType.BOOL;
                }
            }
        }

        public List<SceneCondition> conditions;
        public List<SceneTotal> intTotals;
        public List<SceneTotal> floatTotals;
        public List<SceneTotal> sentences;

        public object Value
        {
            get
            {
                switch (type)
                {
                    case ComplexSceneVarType.CONDITION: return conditions.VerifyConditions();
                    case ComplexSceneVarType.TOTAL_INT: return intTotals.EvaluateIntTotal();
                    case ComplexSceneVarType.TOTAL_FLOAT: return floatTotals.EvaluateFloatTotal();
                    case ComplexSceneVarType.SENTENCE: return sentences.EvaluateSentence();
                    default: return null;
                }
            }
        }

        public bool IsRandom
        {
            get
            {
                switch (type)
                {
                    case ComplexSceneVarType.CONDITION: return conditions.HasRandom();
                    case ComplexSceneVarType.TOTAL_INT: return intTotals.HasRandom();
                    case ComplexSceneVarType.TOTAL_FLOAT: return floatTotals.HasRandom();
                    case ComplexSceneVarType.SENTENCE:
                    default: return false;
                }
            }
        }

        public List<int> Dependencies
        {
            get
            {
                List<int> dependencies = type switch
                {
                    ComplexSceneVarType.CONDITION => new(conditions.Dependencies()),
                    ComplexSceneVarType.TOTAL_INT => new(intTotals.Dependencies()),
                    ComplexSceneVarType.TOTAL_FLOAT => new(floatTotals.Dependencies()),
                    ComplexSceneVarType.SENTENCE => new(sentences.Dependencies()),
                    _ => new(),
                };
                dependencies.Insert(0, uniqueID);
                return dependencies;
            }
        }
        public bool DependOn(int UID)
        {
            if (UID == uniqueID) return true;
            return type switch
            {
                ComplexSceneVarType.CONDITION => conditions.DependOn(UID),
                ComplexSceneVarType.TOTAL_INT => intTotals.DependOn(UID),
                ComplexSceneVarType.TOTAL_FLOAT => floatTotals.DependOn(UID),
                ComplexSceneVarType.SENTENCE => sentences.DependOn(UID),
                _ => new(),
            };
        }
        public void SetForbiddenUID(int UID = 0)
        {
            conditions.SetForbiddenUID(uniqueID);
            intTotals.SetForbiddenUID(uniqueID);
            floatTotals.SetForbiddenUID(uniqueID);
            sentences.SetForbiddenUID(uniqueID);
        }

        /// <summary>
        /// The Link is only used in the Editor for <see cref="SceneVar"/>-picking <br></br>
        /// At runtime, only the <see cref="uniqueID"/> will matter
        /// </summary>
        //public SceneVar Link;
        /// <summary>
        /// The Link is only used in the Editor for <see cref="SceneVar"/>-picking <br></br>
        /// At runtime, only the <see cref="uniqueID"/> will matter
        /// </summary>
        public SceneVar Link => sceneVariablesSO[uniqueID];

        [SerializeField] private float propertyHeight;


        #region Log
        public override string ToString()
        {
            return ID + " (" + type.ToString() + ")";
        }
        #endregion
    }
}
