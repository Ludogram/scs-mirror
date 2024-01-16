using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    //[CreateAssetMenu(fileName = "IntersceneVars", menuName = "Scene Creation/Interscene Vars")]
    public class IntersceneVariablesSO : BaseVariablesContainer
    {
        public override List<SceneVar> SceneVars => sceneVars;


        private Vector2Int uidRange = new Vector2Int(10001, 11000);
        protected override Vector2Int UIDRange => uidRange;

        public override void AddSceneVarOfType(SceneVarType type)
        {
            sceneVars.Add(new(GenerateUniqueID(), type, true));
        }
    }
}
