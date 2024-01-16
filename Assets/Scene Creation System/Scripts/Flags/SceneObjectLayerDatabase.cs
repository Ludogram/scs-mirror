using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    //[CreateAssetMenu(fileName = "SceneObject Layer Database", menuName = "Scene Creation/Database/Layers")]
    public class SceneObjectLayerDatabase : FlagDatabase
    {
        public static SceneObjectLayerDatabase Instance => SceneObjectSettings.Instance.LayerDatabase;
    }
}
