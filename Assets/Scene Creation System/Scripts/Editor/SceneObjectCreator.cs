using UnityEngine;
using UnityEditor;

using MenuCommand = UnityEditor.MenuCommand;

namespace Dhs5.SceneCreation
{
    public class SceneObjectCreator
    {
        public const string menuPath = "GameObject/SceneObjects/";

        protected static BaseSceneObject CreateSceneObject(GameObject prefab, MenuCommand menuCommand)
        {
            GameObject obj = PrefabUtility.InstantiatePrefab(prefab, Selection.activeTransform) as GameObject;
            BaseSceneObject sceneObject = obj.GetComponent<BaseSceneObject>();
            if (sceneObject != null && sceneObject is not SceneManager) sceneObject.Refresh();
            GameObjectUtility.SetParentAndAlign(obj, menuCommand?.context as GameObject);
            PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
            Selection.activeGameObject = obj;
            return sceneObject;
        }

        [MenuItem(menuPath + "SceneObject", priority = 10, secondaryPriority = 3)]
        public static SceneObject CreateSimpleSceneObject(MenuCommand menuCommand)
        {
            return CreateSceneObject(SceneCreationSettings.instance.Prefabs.sceneObjectPrefab, menuCommand) as SceneObject;
        }
        
        [MenuItem(menuPath + "SceneManager", priority = 10, secondaryPriority = 1)]
        public static SceneManager CreateSceneManager(MenuCommand menuCommand)
        {
            return CreateSceneObject(SceneCreationSettings.instance.Prefabs.sceneManagerPrefab, menuCommand) as SceneManager;
        }
        
        [MenuItem(menuPath + "SceneClock", priority = 10, secondaryPriority = 2)]
        public static SceneClock CreateSceneClock(MenuCommand menuCommand)
        {
            return CreateSceneObject(SceneCreationSettings.instance.Prefabs.sceneClockPrefab, menuCommand) as SceneClock;
        }
        
        [MenuItem(menuPath + "Helpers/Collider SceneObject", priority = 10, secondaryPriority = 10)]
        public static Collider_SObj CreateColliderSceneObject(MenuCommand menuCommand)
        {
            return CreateSceneObject(SceneCreationSettings.instance.Prefabs.colliderSceneObjectPrefab, menuCommand) as Collider_SObj;
        }
        
        [MenuItem(menuPath + "SceneSpawner", priority = 10, secondaryPriority = 4)]
        public static SceneSpawner CreateSceneSpawner(MenuCommand menuCommand)
        {
            return CreateSceneObject(SceneCreationSettings.instance.Prefabs.sceneSpawnerPrefab, menuCommand) as SceneSpawner;
        }
    }
}
