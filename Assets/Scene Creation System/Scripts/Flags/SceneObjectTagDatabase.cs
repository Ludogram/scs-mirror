using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    //[CreateAssetMenu(fileName = "SceneObject Tag Database", menuName = "Scene Creation/Database/Tags")]
    public class SceneObjectTagDatabase : FlagDatabase
    {
        public static SceneObjectTagDatabase Instance => SceneObjectSettings.Instance.TagDatabase;
    }
}
