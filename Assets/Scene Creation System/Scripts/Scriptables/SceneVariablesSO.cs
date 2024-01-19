using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dhs5.SceneCreation
{
    //[CreateAssetMenu(fileName = "SceneVars", menuName = "Scene Creation/Scene Vars")]
    public class SceneVariablesSO : BaseVariablesContainer
    {
        [SerializeField] private IntersceneVariablesSO intersceneVariablesSO;
        public IntersceneVariablesSO IntersceneVariables => intersceneVariablesSO;
        
        [SerializeField] private SceneObjectSettings sceneObjectSettings;
        public SceneObjectSettings Settings => sceneObjectSettings;

        //[SerializeField] private List<SceneVar> sceneVars;
        
        [SerializeField] private List<ComplexSceneVar> complexSceneVars;

        public List<SceneVar> PureSceneVars => sceneVars.Copy();
        public List<ComplexSceneVar> ComplexSceneVars => complexSceneVars.Copy();

        public override List<SceneVar> SceneVars => GetSceneVars();//sceneVars;

        private List<SceneVar> GetSceneVars()
        {
            //if (intersceneVariablesSO != null && intersceneVariablesSO.SceneVars.IsValid())
            //{
            //    return sceneVars.IsValid() ? sceneVars.Concat(intersceneVariablesSO.SceneVars).ToList() : intersceneVariablesSO.SceneVars;
            //}
            return sceneVars;
        }


        #region Private Editor Var

        private enum Page { SceneVars = 0, ComplexSceneVars = 1, GlobalVars = 2, Balancing = 3 }
        [SerializeField] private Page currentPage;

        [SerializeField] private int currentBalancingSheetIndex;

        #endregion

        public new SceneVar this[int uniqueID] 
        {
            get
            {
                if (uniqueID >= UIDRange.y || uniqueID < UIDRange.x)
                {
                    Debug.LogError("UID " + uniqueID + " is not in the range " + UIDRange);
                    return null;
                }

                SceneVar sVar = SceneVars.Find(v => v.uniqueID == uniqueID);
                if (sVar == null) Debug.LogError("Can't find SceneVar from UID " + uniqueID + " in " + name);
                return sVar;
            }
        }

        #region Scene Vars
        /*
        public void AddSceneVarOfType(SceneVarType type)
        {
            sceneVars.Add(new(GenerateUniqueID(), type));
        }

        public void TryRemoveSceneVarAtIndex(int index)
        {
            if (sceneVars.IsIndexValid(index))
            {
                if (!sceneVars[index].IsLink)
                {
                    sceneVars.RemoveAt(index);
                }
                else
                {
                    Debug.LogError("Can't remove link");
                }
            }
            else
            {
                Debug.LogError("Invalid index");
            }
        }

        public bool CanRemoveAtIndex(int index)
        {
            return sceneVars.IsIndexValid(index) && !sceneVars[index].IsLink;
        }
        public bool IsLinkAtIndex(int index)
        {
            return sceneVars.IsIndexValid(index) && sceneVars[index].IsLink;
        }
        */
        #endregion

        #region Complex Scene Vars

        #region Getters
        public ComplexSceneVar GetComplexSceneVarWithUID(int UID)
        {
            ComplexSceneVar v = complexSceneVars.Find(x => x.uniqueID == UID);
            if (v == null)
            {
                Debug.LogError("No Complex Scene Var with UID " + UID + " found in " + name);
            }
            return v;
        }
        public bool TryGetComplexSceneVarWithUID(int UID, out ComplexSceneVar complexSceneVar)
        {
            complexSceneVar = GetComplexSceneVarWithUID(UID);

            return complexSceneVar != null;
        }
        #endregion

        #region List Management

        private void CreateLink(ComplexSceneVar complexSceneVar)
        {
            sceneVars.Add(SceneVar.CreateLink(complexSceneVar));
        }

        public void AddComplexSceneVarOfType(ComplexSceneVarType type)
        {
            ComplexSceneVar complexVar = new(GenerateUniqueID(), type);

            complexSceneVars.Add(complexVar);
            CreateLink(complexVar);
        }

        public void TryRemoveComplexSceneVarAtIndex(int index)
        {
            if (complexSceneVars.IsIndexValid(index))
            {
                ComplexSceneVar v = complexSceneVars[index];
                
                if (sceneVars.Remove(v.Link))
                {
                    complexSceneVars.Remove(v);
                }
                else
                {
                    Debug.LogError("Can't find Link of " + v + " in the SceneVar list");
                }
            }
            else
            {
                Debug.LogError("Invalid index");
            }
        }
        #endregion

        #region Set up

        private void SetupComplexSceneVars()
        {
            complexSceneVars.SetUp(this);
            complexSceneVars.SetForbiddenUID(0);
        }

        #endregion

        #region Links Management
        private void UpdateSceneVarLinks()
        {
            // Browse on Complex Scene Vars
            foreach (var cVar in complexSceneVars)
            {
                // If link is null, create one
                if (cVar.Link == null)
                {
                    Debug.LogError("No Scene Var Link found for Complex Scene Var" + cVar.uniqueID + " , creating one");
                    CreateLink(cVar);
                }
            }
            
            // Browse on Scene Vars
            foreach (var v in sceneVars.Copy())
            {
                if (v.IsLink && (!TryGetComplexSceneVarWithUID(v.uniqueID, out ComplexSceneVar cVar) || cVar.Link != v))
                {
                    RemoveSceneVar(v);
                }
            }
        }

        #endregion
        
        #endregion

        #region IDs
        /*
        public int GetUniqueIDByIndex(int index)
        {
            if (index < 0 || index >= SceneVars.Count) return 0;
            return SceneVars[index].uniqueID;
        }
        public int GetIndexByUniqueID(int uniqueID)
        {
            if (uniqueID == 0) return -1;
            return SceneVars.FindIndex(v => v.uniqueID == uniqueID);
        }
        
        private List<int> UniqueIDs
        {
            get
            {
                List<int> list = new();
                foreach (var var in SceneVars)
                {
                    list.Add(var.uniqueID);
                }
                return list;
            }
        }
        public List<string> IDs
        {
            get
            {
                List<string> list = new();
                foreach (var var in SceneVars)
                {
                    if (var.uniqueID != 0)
                        list.Add(var.ID);
                    else
                        list.Add("No unique ID");
                }
                return list;
            }
        }
        public List<string> SceneVarStrings
        {
            get
            {
                List<string> list = new();
                foreach (var var in SceneVars)
                {
                    if (var.uniqueID != 0)
                        list.Add(var.PopupString());
                    else
                        list.Add("No unique ID");
                }
                return list;
            }
        }
        
        public int GenerateUniqueID()
        {
            int uniqueID;
            List<int> uniqueIDs = UniqueIDs;
            do
            {
                uniqueID = Random.Range(1, 10000);
            } while (uniqueIDs.Contains(uniqueID));
            
            return uniqueID;
        }
        */
        #endregion

        [SerializeField] private Vector2Int uidRange = new Vector2Int(1, 10000);
        protected override Vector2Int UIDRange => uidRange;

        
        private void OnValidate()
        {
            SetupComplexSceneVars();

            UpdateSceneVarLinks();

#if UNITY_EDITOR
            GetSceneCreationSettings();
#endif
        }
        

#if UNITY_EDITOR
        internal void OnEditorEnable()
        {
            GetSceneCreationSettings();
        }

        internal void FixUIDsNotInRange()
        {
            if (!Application.isPlaying)
            {
                int uid;
                SceneVar sVar;
                ComplexSceneVar cVar;

                for (int i = 0; i < complexSceneVars.Count; i++)
                {
                    cVar = complexSceneVars[i];
                    uid = cVar.uniqueID;
                    if (uid >= UIDRange.y || uid < UIDRange.x)
                    {
                        cVar.uniqueID = GenerateUniqueID();
                        sVar = sceneVars.Find(x => x.uniqueID == uid);
                        if (sVar != null) sVar.uniqueID = cVar.uniqueID;
                    }
                }

                for (int i = 0; i < sceneVars.Count; i++)
                {
                    sVar = sceneVars[i];
                    uid = sVar.uniqueID;
                    if (!sVar.IsLink && (uid >= UIDRange.y || uid < UIDRange.x))
                    {
                        sVar.uniqueID = GenerateUniqueID();
                    }
                }
            }
        }
#endif

        

        #region Scene Creation Settings

        /// <summary>
        /// Editor only function to get the interscene variables from the settings
        /// </summary>
        private void GetSceneCreationSettings()
        {
            intersceneVariablesSO = SceneCreationSettings.instance.IntersceneVars;
            sceneObjectSettings = SceneCreationSettings.instance.SceneObjectSettings;
        }

        #endregion

        #region Lists
        /*
        public List<string> VarStrings(List<SceneVar> vars)
        {
            List<string> list = new();
            foreach (var var in vars)
            {
                if (var.uniqueID != 0)
                    list.Add(var.PopupString());
                else
                    list.Add("No unique ID");
            }
            return list;
        }
        public int GetUniqueIDByIndex(List<SceneVar> vars, int index)
        {
            if (index < 0 || index >= vars.Count) return 0;
            return vars[index].uniqueID;
        }
        public int GetIndexByUniqueID(List<SceneVar> vars, int uniqueID)
        {
            if (uniqueID == 0) return -1;
            return vars.FindIndex(v => v.uniqueID == uniqueID);
        }
        */
        public List<SceneVar> GetListByType(SceneVarType type, bool precisely = false)
        {
            switch (type)
            {
                case SceneVarType.BOOL: return Booleans;
                case SceneVarType.INT: return precisely ? Integers : Numbers;
                case SceneVarType.FLOAT: return precisely ? Floats : Numbers;
                case SceneVarType.STRING: return Strings;
                case SceneVarType.EVENT: return Events;
                default: return SceneVars;
            }
        }

        public List<SceneVar> Statics
        {
            get => sceneVars != null ? sceneVars.FindAll(v => v.IsStatic) : null;
        }
        public List<SceneVar> Modifyables
        {
            get => SceneVars != null ? SceneVars.FindAll(v => !v.IsStatic && !v.IsRandom && !v.IsLink) : null;
        }
        public List<SceneVar> Listenables
        {
            get => SceneVars != null ? SceneVars.FindAll(v => !v.IsStatic && !v.IsRandom) : null;
        }
        public List<SceneVar> Conditionable
        {
            get => SceneVars != null ? SceneVars.FindAll(v => !v.IsStatic && v.type != SceneVarType.EVENT) : null;
        }
        public List<SceneVar> Booleans
        {
            get => SceneVars != null ? SceneVars.FindAll(v => v.type == SceneVarType.BOOL) : null;
        }
        public List<SceneVar> Numbers
        {
            get => SceneVars != null ? SceneVars.FindAll(v => v.type == SceneVarType.INT || v.type == SceneVarType.FLOAT) : null;
        }
        public List<SceneVar> Integers
        {
            get => SceneVars != null ? SceneVars.FindAll(v => v.type == SceneVarType.INT) : null;
        }
        public List<SceneVar> Floats
        {
            get => SceneVars != null ? SceneVars.FindAll(v => v.type == SceneVarType.FLOAT) : null;
        }
        public List<SceneVar> Strings
        {
            get => SceneVars != null ? SceneVars.FindAll(v => v.type == SceneVarType.STRING) : null;
        }
        public List<SceneVar> Events
        {
            get => sceneVars != null ? sceneVars.FindAll(v => v.type == SceneVarType.EVENT) : null;
        }
        public List<SceneVar> NonEvents
        {
            get => SceneVars != null ? SceneVars.FindAll(v => v.type != SceneVarType.EVENT) : null;
        }
        #endregion

        #region Dependency
        public List<SceneVar> CleanListOfCycleDependencies(List<SceneVar> list, int UID)
        {
            List<SceneVar> sceneVars = new();
            foreach (SceneVar v in list)
            {
                if (!v.IsLink || !complexSceneVars.Find(x => x.uniqueID == v.uniqueID).DependOn(UID))
                {
                    sceneVars.Add(v);
                }
            }
            return sceneVars;
        }
        #endregion

        #region Balancing Sheets
        [SerializeField] private List<SceneBalancingSheetSO> sceneBalancingSheets;

        readonly string baseName = "BalancingSheet";

#if UNITY_EDITOR
        internal void CreateNewBalancingSheet()
        {
            //string path = AssetDatabase.GetAssetPath(this);
            //path = path.Substring(0, path.LastIndexOf('/') + 1);

            SceneBalancingSheetSO balancingSheet = ScriptableObject.CreateInstance<SceneBalancingSheetSO>();
            balancingSheet.sceneVariablesSO = this;
            balancingSheet.name = GetUniqueName();
            balancingSheet.hideFlags = HideFlags.None;
            sceneBalancingSheets.Add(balancingSheet);

            AssetDatabase.AddObjectToAsset(balancingSheet, this);
            AssetDatabase.SaveAssets();

            //if (!Directory.Exists(path))
            //{
            //    Directory.CreateDirectory(path);
            //}
            //
            //AssetDatabase.CreateAsset(balancingSheet, path + GetUniqueName(path) + ".asset");
        }
        internal void TryRemoveBalancingSheetAtIndex(int index)
        {
            if (!sceneBalancingSheets.IsValid() || !sceneBalancingSheets.IsIndexValid(index)) return;

            SceneBalancingSheetSO bs = sceneBalancingSheets[index];
            sceneBalancingSheets.RemoveAt(index);
            AssetDatabase.RemoveObjectFromAsset(bs);
            DestroyImmediate(bs, true);
            AssetDatabase.SaveAssets();
        }

        private string GetUniqueName()
        {
            List<string> names = new();
            string current;

            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this)))
            {
                if (o != this)
                {
                    current = o.name;
                    names.Add(current);
                }
            }

            if (names.Count < 1)
            {
                return baseName + "1";
            }

            int index = 1;

            while (names.Contains(baseName + index))
            {
                index++;
            }
            return baseName + index;
        }

        internal SceneBalancingSheetSO GetCurrentlySelectedBalancingSheet()
        {
            if (!sceneBalancingSheets.IsValid() || !sceneBalancingSheets.IsIndexValid(currentBalancingSheetIndex))
            {
                return null;
            }
            return sceneBalancingSheets[currentBalancingSheetIndex];
        }
#endif
        public List<SceneVar> BalancedSceneVars(int balancingIndex)
        {
            List<SceneVar> vars;

            if (!sceneBalancingSheets.IsValid()
                || balancingIndex <= 0
                || balancingIndex > sceneBalancingSheets.Count)
            {
                vars = PureSceneVars;
            }
            else
            {
                vars = sceneBalancingSheets[balancingIndex - 1].SceneVars;
            }

            return vars;
        }
        #endregion


        #region Helper Functions

        protected override bool CallBaseCleanUp() => false;
        protected override void ChildCleanUp()
        {
            foreach (var var in sceneVars.Copy())
            {
                if (var.uniqueID == 0)
                    RemoveSceneVar(var);
                else if (var.IsLink)
                {
                    bool isLink = false;
                    foreach (var cVar in complexSceneVars)
                    {
                        if (cVar.Link == var) isLink = true;
                    }
                    if (!isLink)
                        RemoveSceneVar(var);
                }
            }
        }

        #endregion
    }
}
