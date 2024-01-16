using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Scripting;
using System.Text;

namespace Dhs5.SceneCreation
{
    public interface IOnStartScene
    {
        public void OnStartScene();
    }
    public interface IOnChangeScene
    {
        public void OnChangeScene();
    }
    public interface IOnCompleteScene
    {
        public void OnCompleteScene();
    }
    public interface IOnUpdateScene
    {
        public void OnUpdateScene(int frameIndex);
    }
    public interface IOnGameOver
    {
        public void OnGameOver();
    }

    public abstract class BaseSceneObject : MonoBehaviour, SceneState.ISceneLogableWithChild, SceneState.ISceneVarDependantWithChild
    {
        [SerializeField, HideInInspector] protected SceneVariablesSO sceneVariablesSO;
        public SceneVariablesSO SceneVariablesSO => sceneVariablesSO;

        [SerializeField, HideInInspector] protected SceneObjectTag sceneObjectTag;
        public SceneObjectTag Tag => sceneObjectTag;
        
        [SerializeField, HideInInspector] protected SceneObjectLayer sceneObjectLayer;
        public SceneObjectLayer Layer => sceneObjectLayer;

        public virtual string DisplayName => "BaseSceneObject";

        protected SceneObjectSettings Settings => sceneVariablesSO != null ? sceneVariablesSO.Settings : null;

        #region Private Editor References

#if UNITY_EDITOR
        /// <summary>
        /// Editor only reference to the <see cref="SceneManager"/> of the Scene
        /// </summary>
        [SerializeField, HideInInspector] private SceneManager sceneManager;

        [SerializeField, HideInInspector] private int currentPage;
#endif

        #endregion

        #region Base
        private void Awake()
        {
            RegisterSceneElements();

            Init();
            SetBelongings();

            OnSceneObjectAwake();
        }

        private void OnEnable()
        {
            SceneState.Register(this);

            StartSubscription();
            EnableScriptables();

            OnSceneObjectEnable();
        }
        private void OnDisable()
        {
            SceneState.Unregister(this);

            EndSubscription();
            DisableScriptables();

            OnSceneObjectDisable();
        }

        private void OnValidate()
        {
            if (GetSceneVariablesSO())
            {
                UpdateSceneVariables();
            }

            OnSceneObjectValidate();
        }
        #endregion

        #region Automatisation
        /// <summary>
        /// Private function getting the <see cref="SceneManager"/> of the Scene and the <see cref="SceneVariablesSO"/>
        /// </summary>
        /// <returns>True if <see cref="SceneVariablesSO"/> is valid</returns>
        private bool GetSceneVariablesSO()
        {
            if (this is SceneManager) return sceneVariablesSO != null;

            if (sceneManager == null)
            {
                sceneManager = FindObjectOfType<SceneManager>();
            }

            if (sceneManager != null)
            {
                sceneVariablesSO = sceneManager.SceneVariablesSO;
                return sceneVariablesSO != null;
            }
            else
            {
                sceneVariablesSO = null;
            }

            return false;
        }
        /// <summary>
        /// Called on <see cref="Awake"/>.<br></br><br></br>
        /// If overriden :<br></br>
        /// ALWAYS call <see cref="Init"/><br></br>
        /// Init UNREGISTERED <see cref="SceneState.IInitializable"/>s elements HERE.
        /// </summary>
        protected virtual void Init()
        {
            LinkScriptables();
            InitEvents();
            InitProfiles();
        }
        /// <summary>
        /// Called on <see cref="Awake"/>.<br></br><br></br>
        /// If overriden :<br></br>
        /// ALWAYS call <see cref="SetBelongings"/><br></br>
        /// Set the belongings of UNREGISTERED <see cref="SceneState.ISceneObjectBelongable"/>s elements to this object HERE.
        /// </summary>
        protected virtual void SetBelongings()
        {
            SetEventsBelongings();
            SetListenersBelongings();
            SetTweensBelongings();
            SetProfileBelongings();
        }
        /// <summary>
        /// Called on <see cref="OnEnable"/>.<br></br><br></br>
        /// If overriden :<br></br>
        /// ALWAYS call <see cref="StartSubscription"/><br></br>
        /// Start subscription of UNREGISTERED <see cref="SceneState.ISceneSubscribable"/>s elements HERE.
        /// </summary>
        protected virtual void StartSubscription()
        {
            StartListenersSubscription();
            StartProfileSubscription();
        }
        /// <summary>
        /// Called on <see cref="OnDisable"/>.<br></br><br></br>
        /// If overriden :<br></br>
        /// ALWAYS call <see cref="EndSubscription"/><br></br>
        /// End subscription of UNREGISTERED <see cref="SceneState.ISceneSubscribable"/>s elements HERE.
        /// </summary>
        protected virtual void EndSubscription()
        {
            EndListenersSubscription();
            EndProfileSubscription();
        }
        /// <summary>
        /// Called on <see cref="OnEnable"/>.<br></br><br></br>
        /// If overriden :<br></br>
        /// ALWAYS call <see cref="EnableScriptables"/><br></br>
        /// Enable only UNREGISTERED <see cref="SceneScriptableObject"/>s HERE.
        /// </summary>
        protected virtual void EnableScriptables()
        {
            EnableScriptablesList();
        }
        /// <summary>
        /// Called on <see cref="OnDisable"/>.<br></br><br></br>
        /// If overriden :<br></br>
        /// ALWAYS call <see cref="DisableScriptables"/><br></br>
        /// Disable only UNREGISTERED <see cref="SceneScriptableObject"/>s HERE.
        /// </summary>
        protected virtual void DisableScriptables()
        {
            DisableScriptablesList();
        }

        #region Editor
        /// <summary>
        /// Editor only function called when the Editor of this object is enabled
        /// </summary>
        internal virtual void OnEditorEnable()
        {
            Refresh();
        }

        [ContextMenu("Refresh")]
        internal void Refresh()
        {
            if (GetSceneVariablesSO())
            {
                UpdateSceneVariables();
            }
        }

        #endregion

        #endregion

        #region Abstracts
        /// <summary>
        /// Called on <see cref="OnValidate"/>.<br></br>
        /// Update in this function the <see cref="Dhs5.SceneCreation.SceneVariablesSO"/> of :
        /// <list type="bullet">
        /// <item>
        /// <see cref="SceneState.ISceneVarSetupable"/> elements
        /// </item>
        /// <item>
        /// <see cref="SceneState.ISceneVarTypedSetupable"/> elements
        /// </item>
        /// </list>
        /// </summary>
        protected abstract void UpdateSceneVariables();
        /// <summary>
        /// Called on <see cref="Awake"/>.<br></br>
        /// Register in this function :
        /// <list type="bullet">
        /// <item>
        /// <see cref="BaseSceneEvent"/> lists with <see cref="RegisterEvent{T}(string, List{T})"/>
        /// </item>
        /// <item>
        /// <see cref="BaseSceneListener"/> lists with <see cref="RegisterListener{T}(string, List{T})"/>
        /// </item>
        /// <item>
        /// <see cref="SceneVarTween"/>s with <see cref="RegisterTween(string, SceneVarTween)"/>
        /// </item>
        /// <item>
        /// <see cref="SceneScriptableObject"/>s with <see cref="RegisterScriptable{T}(T)"/>
        /// </item>
        /// <item>
        /// <see cref="SceneProfile"/>s with <see cref="RegisterProfile{T}(T)"/>
        /// </item>
        /// </list>
        /// </summary>
        protected abstract void RegisterSceneElements();
        #endregion

        #region Extensions
        /// <summary>
        /// <see cref="Awake"/> extension.
        /// </summary>
        protected virtual void OnSceneObjectAwake() { }
        /// <summary>
        /// <see cref="OnValidate"/> extension.
        /// </summary>
        protected virtual void OnSceneObjectValidate() { }
        /// <summary>
        /// <see cref="OnEnable"/> extension.
        /// </summary>
        protected virtual void OnSceneObjectEnable() { }
        /// <summary>
        /// <see cref="OnDisable"/> extension.
        /// </summary>
        protected virtual void OnSceneObjectDisable() { }
        #endregion

        #region Registration
        protected Dictionary<string, List<BaseSceneEvent>> SceneEventsDico { get; private set; } = new();
        protected Dictionary<string, List<BaseSceneListener>> SceneListenersDico { get; private set; } = new();
        protected Dictionary<string, SceneVarTween> TweensDico { get; private set; } = new();
        protected List<SceneScriptableObject> SceneScriptablesList { get; private set; } = new();
        protected List<SceneProfile> SceneRegisteredProfiles { get; private set; } = new();

        #region Events
        protected void RegisterEvent<T>(string name, List<T> list) where T : BaseSceneEvent
        {
            if (list.IsValid())
                SceneEventsDico[name] = list.Cast<BaseSceneEvent>().ToList();
        }
        protected void RegisterEvents<T>(params (string name, List<T> list)[] vars) where T : BaseSceneEvent
        {
            if (vars.IsValid())
                foreach (var v in vars)
                    RegisterEvent(v.name, v.list);
        }
        #endregion

        #region Listeners
        protected void RegisterListener<T>(string name, List<T> list) where T : BaseSceneListener
        {
            if (list.IsValid())
                SceneListenersDico[name] = list.Cast<BaseSceneListener>().ToList();
        }
        protected void RegisterListeners<T>(params (string name, List<T> list)[] vars) where T : BaseSceneListener
        {
            if (vars.IsValid())
                foreach (var v in vars)
                    RegisterListener(v.name, v.list);
        }
        #endregion

        #region Tweens
        protected void RegisterTween(string name, SceneVarTween tween)
        {
            if (tween != null)
                TweensDico[name] = tween;
        }
        protected void RegisterTweens(params (string name, SceneVarTween tween)[] vars)
        {
            if (vars.IsValid())
                foreach (var v in vars)
                    RegisterTween(v.name, v.tween);
        }
        #endregion

        #region Scriptables
        protected void RegisterScriptable<T>(T scriptable) where T : SceneScriptableObject
        {
            if (scriptable == null || SceneScriptablesList.Contains(scriptable)) return;

            SceneScriptablesList.Add(scriptable);
        }
        protected void RegisterScriptables<T>(List<T> scriptables) where T : SceneScriptableObject
        {
            if (scriptables.IsValid())
                foreach (var scriptable in scriptables)
                    RegisterScriptable(scriptable);
        }
        protected void RegisterScriptables<T>(params T[] scriptables) where T : SceneScriptableObject
        {
            if (scriptables.IsValid())
                foreach (var scriptable in scriptables)
                    RegisterScriptable(scriptable);
        }
        #endregion

        #region Profiles

        protected void RegisterProfile<T>(T profile) where T : SceneProfile
        {
            if (profile != null)
            {
                if (HasRegisteredProfileOfType<T>())
                {
                    SceneDebugger.Log("Can't register 2 profiles of the same type : " + profile, this);
                    return;
                }

                SceneRegisteredProfiles.Add(profile);
            }
        }
        protected void RegisterProfiles(params SceneProfile[] profiles)
        {
            if (profiles.IsValid())
                foreach (var profile in profiles)
                    RegisterProfile(profile);
        }

        #endregion


        #region Utility

        #region Events
        private void InitEvents()
        {
            if (SceneEventsDico.IsValid())
            {
                foreach (var pair in SceneEventsDico)
                {
                    pair.Value.Init();
                }
            }
        }
        private void SetEventsBelongings()
        {
            if (SceneEventsDico.IsValid())
            {
                foreach (var pair in SceneEventsDico)
                {
                    Belong(pair.Value);
                }
            }
        }
        #endregion

        #region Listeners
        private void SetListenersBelongings()
        {
            if (SceneListenersDico.IsValid())
            {
                foreach (var pair in SceneListenersDico)
                {
                    Belong(pair.Value);
                }
            }
        }
        private void StartListenersSubscription()
        {
            if (SceneListenersDico.IsValid())
            {
                foreach (var pair in SceneListenersDico)
                {
                    pair.Value.Subscribe();
                }
            }
        }
        private void EndListenersSubscription()
        {
            if (SceneListenersDico.IsValid())
            {
                foreach (var pair in SceneListenersDico)
                {
                    pair.Value.Unsubscribe();
                }
            }
        }
        #endregion

        #region Tweens
        private void SetTweensBelongings()
        {
            if (TweensDico.IsValid())
            {
                foreach (var pair in TweensDico)
                {
                    Belong(pair.Value);
                }
            }
        }
        #endregion

        #region Scriptables
        private void LinkScriptables()
        {
            if (SceneScriptablesList.IsValid())
            {
                Link(SceneScriptablesList);
            }
        }
        private void EnableScriptablesList()
        {
            if (SceneScriptablesList.IsValid())
            {
                SceneScriptablesList.OnSceneObjectEnable();
            }
        }
        private void DisableScriptablesList()
        {
            if (SceneScriptablesList.IsValid())
            {
                SceneScriptablesList.OnSceneObjectDisable();
            }
        }
        #endregion

        #region Profiles

        private void InitProfiles()
        {
            if (SceneRegisteredProfiles.IsValid())
            {
                foreach (var profile in SceneRegisteredProfiles)
                {
                    profile.Init();
                }
            }
        }
        private void SetProfileBelongings()
        {
            if (SceneRegisteredProfiles.IsValid())
            {
                foreach (var profile in SceneRegisteredProfiles)
                {
                    Belong(profile);
                }
            }
        }
        private void StartProfileSubscription()
        {
            if (SceneRegisteredProfiles.IsValid())
            {
                foreach (var profile in SceneRegisteredProfiles)
                {
                    profile.Subscribe();
                }
            }
        }
        private void EndProfileSubscription()
        {
            if (SceneRegisteredProfiles.IsValid())
            {
                foreach (var profile in SceneRegisteredProfiles)
                {
                    profile.Unsubscribe();
                }
            }
        }

        #endregion

        #endregion

        #endregion


        #region SceneObject's specifics
        /*
        public virtual bool DoStartScene => false;
        /// <summary>
        /// Called on <see cref="SceneManager.StartScene"/> once the <see cref="SceneState"/> has been set up.
        /// </summary>
        /// <remarks>Must set <see cref="DoStartScene"/> to TRUE to be called.</remarks>
        internal virtual void OnStartScene() { }
        
        public virtual bool DoUpdateScene => false;
        /// <summary>
        /// Called on <see cref="SceneManager.UpdateScene"/> once per frame.
        /// </summary>
        /// <remarks>Must set <see cref="DoUpdateScene"/> to TRUE to be called.</remarks>
        internal virtual void OnUpdateScene(int frameIndex) { }

        public virtual bool DoChangeScene => false;
        /// <summary>
        /// Called on <see cref="SceneManager.ChangeScene"/> when the Scene is going to change.
        /// </summary>
        /// <remarks>Must set <see cref="DoChangeScene"/> to TRUE to be called.</remarks>
        internal virtual void OnChangeScene() { }
        
        public virtual bool DoCompleteScene => false;
        /// <summary>
        /// Called on <see cref="SceneManager.CompleteScene"/> when the Scene is completed.
        /// </summary>
        /// <remarks>Must set <see cref="DoCompleteScene"/> to TRUE to be called.</remarks>
        internal virtual void OnCompleteScene() { }

        public virtual bool DoGameOver => false;
        /// <summary>
        /// Called on <see cref="SceneManager.GameOver"/> when the game is over.
        /// </summary>
        /// <remarks>Must set <see cref="DoGameOver"/> to TRUE to be called.</remarks>
        internal virtual void OnGameOver() { }
        */
        #endregion

        #region Trigger

        /// <summary>
        /// Function called by a <see cref="SceneListener"/> when it has been instructed to Trigger its <see cref="BaseSceneObject"/>
        /// </summary>
        /// <param name="trigger"><see cref="SceneListener.SceneEventTrigger"/> informations on the Trigger</param>
        /// <param name="param"><see cref="SceneEventParam"/> passed during the triggering</param>
        public virtual void Trigger(SceneListener.SceneEventTrigger trigger, SceneEventParam param) { }

        #endregion

        #region Added Profiles
        protected List<SceneProfile> SceneAddedProfiles { get; private set; } = new();
        public int ProfileCount => SceneAddedProfiles.Count + SceneRegisteredProfiles.Count;

        #region Registered Profiles

        private T GetRegisteredProfileOfType<T>() where T : SceneProfile
        {
            if (!SceneRegisteredProfiles.IsValid()) return null;

            return SceneRegisteredProfiles.Find(p => p is T) as T;
        }
        protected bool HasRegisteredProfileOfType<T>() where T : SceneProfile
        {
            return GetRegisteredProfileOfType<T>() != null;
        }

        public bool OverrideProfile<T>(T overridingProfile) where T : SceneProfile
        {
            for (int i = 0; i < SceneRegisteredProfiles.Count; i++)
            {
                if (SceneRegisteredProfiles[i] is T)
                {
                    if (SceneRegisteredProfiles[i].Override(overridingProfile))
                    {
                        SceneRegisteredProfiles[i].Attach(this);
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

        #endregion

        #region List Handling
        /// <summary>
        /// Apply a list of <see cref="SceneProfile"/>s to this <see cref="BaseSceneObject"/> after clearing its profiles.
        /// </summary>
        /// <param name="_sceneVariablesSO"></param>
        /// <param name="profiles"></param>
        public void ApplyProfiles(SceneVariablesSO _sceneVariablesSO, List<SceneProfile> profiles)
        {
            if (sceneVariablesSO == null) sceneVariablesSO = _sceneVariablesSO;

            if (!profiles.IsValid()) return;

            Profiles_Clear();

            foreach (var profile in profiles)
            {
                ApplyProfile(profile);
            }
        }
        public void ApplyProfile(SceneProfile profile)
        {
            if (profile == null) return;

            if (!OverrideProfile(profile))
            {
                AddProfile(profile);
            }
        }
        private void AddProfile(SceneProfile profile)
        {
            SceneAddedProfiles.Add(profile);

            profile.Attach(this);
        }
        
        public bool RemoveProfileOfType<T>() where T : SceneProfile
        {
            if (!SceneAddedProfiles.IsValid()) return false;

            foreach (var profile in SceneAddedProfiles)
            {
                if (profile is T)
                {
                    return RemoveProfile(profile);
                }
            }

            return false;
        }
        private bool RemoveProfile(SceneProfile profile)
        {
            profile.Detach();
            return SceneAddedProfiles.Remove(profile);
        }

        public void Profiles_Clear()
        {
            if (!SceneAddedProfiles.IsValid())
            {
                SceneAddedProfiles = new();
                return;
            }

            SceneAddedProfiles.Detach();
            SceneAddedProfiles.Clear();
        }
        #endregion

        #region Get Profile
        public T GetProfileOfType<T>() where T : SceneProfile
        {
            T registeredProfile = GetRegisteredProfileOfType<T>();

            if (registeredProfile != null) return registeredProfile;

            if (!SceneAddedProfiles.IsValid()) return null;

            return SceneAddedProfiles.Find(p => p is T) as T;
        }
        public bool HasProfileOfType<T>() where T : SceneProfile
        {
            return GetProfileOfType<T>() != null;
        }
        #endregion

        #region Actions

        // ----- CODE ONLY -----
        public bool TriggerProfileOfType<T>() where T : SceneProfile
        {
            if (!SceneAddedProfiles.IsValid()) return false;

            T profile = GetProfileOfType<T>();
            if (profile != null)
            {
                profile.Trigger();
                return true;
            }
            return false;
        }
        // ----- EDITOR -----
        public void Profiles_TriggerAll()
        {
            if (!SceneAddedProfiles.IsValid()) return;

            foreach (var profile in SceneAddedProfiles)
                profile.Trigger();
        }
        public void Profiles_TriggerWithID(string eventID)
        {
            if (!SceneAddedProfiles.IsValid()) return;

            foreach (var profile in SceneAddedProfiles)
                profile.TriggerWithID(eventID);
        }

        #region Blabla
        /// <summary>
        /// For every <see cref="SceneProfile"/> in <see cref="SceneProfiles"/> :<br></br>
        /// ""<inheritdoc cref="SceneProfile.TriggerAndRemove(bool)"/> ""
        /// </summary>
        /// <param name="onlyIfTriggered">Whether to remove only triggered events or all of them</param>
        public void Profiles_TriggerAndRemoveAll(bool onlyIfTriggered)
        {
            if (!SceneAddedProfiles.IsValid()) return;

            foreach (var profile in SceneAddedProfiles)
                profile.TriggerAndRemove(onlyIfTriggered);
        }
        // ----- COMPLEXE -----
        /// <summary>
        /// For every <see cref="SceneProfile"/> in <see cref="SceneProfiles"/> :<br></br>
        /// ""<inheritdoc cref="SceneProfile.TriggerAndRemoveWithID(string, bool)"/> ""
        /// </summary>
        /// <param name="eventID">ID of the <see cref="BaseSceneEvent"/>s to trigger</param>
        /// <param name="onlyIfTriggered">Whether to remove only triggered events or all of them</param>
        [Preserve] public void Profiles_TriggerAndRemoveWithID(string eventID, bool onlyIfTriggered)
        {
            if (!SceneAddedProfiles.IsValid()) return;

            foreach (var profile in SceneAddedProfiles)
                profile.TriggerAndRemoveWithID(eventID, onlyIfTriggered);
        }
        // ----- RANDOM -----
        public bool TriggerRandomInProfileOfType<T>(bool remove = false, string filter = null) where T : SceneProfile
        {
            if (!SceneAddedProfiles.IsValid()) return false;

            foreach (var profile in SceneAddedProfiles)
                if (profile is T p)
                {
                    return p.TriggerRandom(filter, remove);
                }
            return false;
        }
        #endregion

        #endregion

        #endregion

        #region SceneLog
        [ContextMenu("Display Log")]
        private void DisplayLog()
        {
            Debug.Log(Log());
        }

        public string Log()
        {
            return ((SceneState.ISceneLogableWithChild)this).Log(true, true);
        }
        
        public List<string> LogLines(bool detailed = false, bool showEmpty = false, string alinea = null)
        {
            string passToLine = "Line()";
            List<string> lines = new();
            StringBuilder sb = new();

            // First Alinea
            if (alinea != null) sb.Append(alinea);

            // Register elements
            RegisterSceneElements();

            // Scene Object compartiment
            AppendColor(SceneLogger.SceneObjectColor, "--------------------------------------------------");
            Line();

            // LISTENERS
            if (showEmpty || SceneListenersDico.IsReallyValid())
            {
                AppendColor(SceneLogger.ListenerColor, "Listeners :", passToLine, "----------------------------------------");
                Line();

                foreach (var pair in SceneListenersDico)
                {
                    AppendColor(SceneLogger.ListenerColor, pair.Key, " :");
                    Line();

                    if (!pair.Value.IsValid()) continue;

                    foreach (var listener in pair.Value)
                    {
                        if (showEmpty || !listener.IsEmpty())
                            lines.AddRange(listener.LogLines(detailed, showEmpty, alinea + "   "));
                    }
                }

                AppendColor(SceneLogger.ListenerColor, "----------------------------------------");
                Line();
            }

            // EVENTS
            if (showEmpty || SceneEventsDico.IsReallyValid())
            {
                AppendColor(SceneLogger.EventColor, "Events :", passToLine, "----------------------------------------");
                Line();

                foreach (var pair in SceneEventsDico)
                {
                    AppendColor(SceneLogger.EventColor, pair.Key, " :");
                    Line();

                    if (!pair.Value.IsValid()) continue;

                    foreach (var events in pair.Value)
                    {
                        if (showEmpty || !events.IsEmpty())
                            lines.AddRange(events.LogLines(detailed, showEmpty, alinea + "   "));
                    }
                }

                AppendColor(SceneLogger.EventColor, "----------------------------------------");
                Line();
            }

            // CHILD LOG
            ChildLog(lines, sb, detailed, showEmpty);
            sb.Clear();

            // TWEENS
            if (showEmpty || TweensDico.IsValid())
            {
                AppendColor(SceneLogger.TweenColor, "Tweens :", passToLine, "----------------------------------------");
                Line();

                foreach (var pair in TweensDico)
                {
                    AppendColor(SceneLogger.TweenColor, "   * ");
                    AppendBold(pair.Key);
                    sb.Append(" : ");
                    sb.Append(pair.Value.LogString());
                    Line();
                }

                AppendColor(SceneLogger.TweenColor, "----------------------------------------");
                Line();
            }

            // SCRIPTABLES
            if (showEmpty || SceneScriptablesList.IsValid())
            {
                AppendColor(SceneLogger.TweenColor, "Scriptables :", passToLine, "----------------------------------------");
                Line();

                foreach (var scriptable in SceneScriptablesList)
                {
                    AppendColor(SceneLogger.TweenColor, "   * ");
                    AppendBold(scriptable.name);
                    sb.Append(" :");
                    Line();

                    //lines.AddRange(scriptable.LogLines(detailed, showEmpty, alinea + "   "));
                }

                AppendColor(SceneLogger.TweenColor, "----------------------------------------");
                Line();
            }

            AppendColor(SceneLogger.SceneObjectColor, "--------------------------------------------------");
            lines.Add(sb.ToString());

            return lines;

            #region Local
            void Line()
            {
                sb.Append('\n');
                lines.Add(sb.ToString());
                sb.Clear();
                if (alinea != null) sb.Append(alinea);
            }
            void AppendColor(string color, params string[] strings)
            {
                sb.Append(color);
                foreach (var s in strings)
                {
                    if (s == passToLine) Line();
                    else sb.Append(s);
                }
                sb.Append(SceneLogger.ColorEnd);
            }
            void AppendBold(params string[] strings)
            {
                sb.Append(SceneLogger.Bold);
                foreach (var s in strings)
                {
                    if (s == passToLine) Line();
                    else sb.Append(s);
                }
                sb.Append(SceneLogger.BoldEnd);
            }
            #endregion
        }
        public bool IsEmpty()
        {
            RegisterSceneElements();

            if (SceneListenersDico.IsReallyValid()) return false;
            if (SceneEventsDico.IsReallyValid()) return false;
            if (!IsChildEmpty()) return false;
            if (TweensDico.IsValid()) return false;
            if (SceneScriptablesList.IsValid()) return false;

            return true;
        }

        public virtual void ChildLog(List<string> lines, StringBuilder sb, bool detailed, bool showEmpty, string alinea = null) { }
        public virtual bool IsChildEmpty() { return true; }
        #endregion

        #region Dependencies

        #region Editor
        internal List<string> GetDisplayDependencies()
        {
            List<string> dependencies = new();
            Dictionary<int, int> deps = new();

            foreach (var d in Dependencies)
            {
                if (deps.ContainsKey(d))
                {
                    deps[d]++;
                }
                else
                {
                    deps[d] = 1;
                }
            }

            foreach (var d in deps)
            {
                dependencies.Add(sceneVariablesSO[d.Key]?.LogString() + "   x" + d.Value);
            }

            return dependencies;
        }
        #endregion

        public List<int> Dependencies
        {
            get
            {
                List<int> dependencies = new List<int>();

                RegisterSceneElements();

                if (SceneListenersDico.IsReallyValid())
                {
                    foreach (var pair in SceneListenersDico)
                    {
                        if (pair.Value.IsValid())
                            dependencies.AddRange(pair.Value.Dependencies());
                    }
                }
                if (SceneEventsDico.IsReallyValid())
                {
                    foreach (var pair in SceneEventsDico)
                    {
                        if (pair.Value.IsValid())
                            dependencies.AddRange(pair.Value.Dependencies());
                    }
                }
                if (TweensDico.IsValid())
                {
                    foreach (var pair in TweensDico)
                    {
                        dependencies.AddRange(pair.Value.Dependencies);
                    }
                }
                if (SceneScriptablesList.IsValid())
                {
                    foreach (var scriptable in SceneScriptablesList)
                    {
                        //if (scriptable != null)
                        //    dependencies.AddRange(scriptable.Dependencies);
                    }
                }

                dependencies.AddRange(ChildDependencies());

                return dependencies;
            }
        }
        public bool DependOn(int UID) { return Dependencies.Contains(UID); }

        public virtual List<int> ChildDependencies() { return new(); }
        #endregion

        #region SceneClock Actions

        /// <summary>
        /// Starts the timeline named <paramref name="timelineID"/> from first step (step 0)
        /// </summary>
        /// <param name="timelineID">ID of the timeline to start</param>
        [Preserve]
        public void Clock_StartTimeline(string timelineID)
        {
            SceneClock.Instance.StartTimeline(timelineID);
        }
        /// <summary>
        /// Starts the timeline named <paramref name="timelineID"/> from step <paramref name="step"/>
        /// </summary>
        /// <param name="timelineID">ID of the timeline to start</param>
        /// <param name="step">Index of the step in the <b>TimelineObject</b> list of the <b>SceneTimeline</b></param>
        [Preserve]
        public void Clock_StartTimeline(string timelineID, int step)
        {
            SceneClock.Instance.StartTimeline(timelineID, step);
        }

        /// <summary>
        /// Stops the timeline named <paramref name="timelineID"/>
        /// </summary>
        /// <param name="timelineID">ID of the timeline to stop</param>
        [Preserve]
        public void Clock_StopTimeline(string timelineID)
        {
            SceneClock.Instance.StopTimeline(timelineID);
        }

        /// <summary>
        /// Makes the timeline <paramref name="timelineID"/> go to step <paramref name="step"/>.<br/>
        /// If <paramref name="interrupt"/> is <b>true</b>, immediatly go to new step.<br/>
        /// Else, wait for the current step to finish its execution.
        /// </summary>
        /// <param name="timelineID">ID of the timeline to change step</param>
        /// <param name="step">Index of the step in the <b>TimelineObject</b> list of the <b>SceneTimeline</b></param>
        /// <param name="interrupt">Whether to interrupt the execution of the current step</param>
        [Preserve]
        public void Clock_TimelineGoToStep(string timelineID, int step, bool interrupt)
        {
            SceneClock.Instance.GoToStep(timelineID, step, interrupt);
        }
        #endregion

        #region Debug

        

        #endregion


        #region Utility

        #region Setup
        protected void Setup<T>(T var) where T : SceneState.ISceneVarSetupable
        {
            var?.SetUp(sceneVariablesSO);
        }
        protected void Setup<T>(IList<T> vars) where T : SceneState.ISceneVarSetupable
        {
            if (vars.IsValid())
                vars.SetUp(sceneVariablesSO);
        }
        protected void Setup<T>(params IList<T>[] vars) where T : SceneState.ISceneVarSetupable
        {
            if (vars.IsValid())
                foreach (var var in vars)
                    var.SetUp(sceneVariablesSO);
        }
        #endregion

        #region Belong
        protected void Belong<T>(T var) where T : SceneState.ISceneObjectBelongable
        {
            var.BelongTo(this);
        }
        protected void Belong<T>(IList<T> vars) where T : SceneState.ISceneObjectBelongable
        {
            if (vars.IsValid())
              vars.BelongTo(this);
        }
        protected void Belong<T>(params IList<T>[] vars) where T : SceneState.ISceneObjectBelongable
        {
            if (vars.IsValid())
                foreach (var var in vars)
                    Belong(var);
        }
        #endregion

        #region Link
        protected void Link<T>(T scriptable) where T : SceneScriptableObject
        {
            scriptable.Link(this);
        }
        protected void Link<T>(List<T> scriptables) where T : SceneScriptableObject
        {
            if (scriptables.IsValid())
                foreach (var scriptable in scriptables)
                    Link(scriptable);
        }
        protected void Link<T>(params List<T>[] vars) where T : SceneScriptableObject
        {
            if (vars.IsValid())
                foreach (var scriptables in vars)
                    Link(scriptables);
        }
        #endregion

        #endregion
    }
}
