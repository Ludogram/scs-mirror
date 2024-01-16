using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using System.Text;

namespace Dhs5.SceneCreation
{
    [Serializable]
    public class SceneParameteredEvent : SceneState.ISceneVarSetupable, SceneState.ISceneObjectBelongable, SceneState.IInitializable, SceneState.ISceneVarDependant
    {
        [SerializeField] private SceneVariablesSO sceneVariablesSO;

        public void SetUp(SceneVariablesSO sceneVariablesSO)
        {
            varTween0.SetUp(sceneVariablesSO, varType0, true);
            varTween1.SetUp(sceneVariablesSO, varType1, true);
            varTween2.SetUp(sceneVariablesSO, varType2, true);
            varTween3.SetUp(sceneVariablesSO, varType3, true);
            varTween4.SetUp(sceneVariablesSO, varType4, true);
        }
        public void BelongTo(BaseSceneObject _sceneObject)
        {
            varTween0.BelongTo(_sceneObject);
            varTween1.BelongTo(_sceneObject);
            varTween2.BelongTo(_sceneObject);
            varTween3.BelongTo(_sceneObject);
            varTween4.BelongTo(_sceneObject);
        }

#if SCS_PARAMED_EVENT
        public BaseEventAction action;
        public BaseEventAction Action
        {
            get
            {
                if (!action.Created) action = action.CreateEventAction();
                return action;
            }
        }
#endif
        public void Init()
        {
#if SCS_PARAMED_EVENT
            if (!action.Created) action = action.CreateEventAction();
#endif
        }

        #region Custom Property Drawer variables
        [SerializeField] UnityEngine.Object obj;
        [SerializeReference] private Component component;
        [SerializeField] private int metadataToken;
        [SerializeField] private float propertyHeight;
        #endregion

        #region Parameters
        // Int
        [SerializeField] private SceneVarTween varTween0;
        [SerializeField] private SceneVarTween varTween1;
        [SerializeField] private SceneVarTween varTween2;
        [SerializeField] private SceneVarTween varTween3;
        [SerializeField] private SceneVarTween varTween4;
        // Types
        [SerializeField] private SceneVarType varType0;
        [SerializeField] private SceneVarType varType1;
        [SerializeField] private SceneVarType varType2;
        [SerializeField] private SceneVarType varType3;
        [SerializeField] private SceneVarType varType4;
        #endregion

        private List<object> GetObjectList(int size)
        {
            if (size == 0) return null;

            List<object> list = new()
            {
                varTween0.Value
            };
            if (size > 1) list.Add(varTween1.Value);
            if (size > 2) list.Add(varTween2.Value);
            if (size > 3) list.Add(varTween3.Value);
            if (size > 4) list.Add(varTween4.Value);

            return list;
        }

        public void Trigger()
        {
#if SCS_PARAMED_EVENT
            if (Action == null)
            {
                Debug.LogError("Action is null");
                return;
            }
            Action.Invoke(GetObjectList(Action.ParameterListSize));
#endif
        }

        #region Log
        public override string ToString()
        {

            StringBuilder sb = new();
#if SCS_PARAMED_EVENT

            sb.Append(obj.ToString());
            sb.Append(":");
            sb.Append(component.ToString());
            sb.Append(".");
            sb.Append(action.MethodName);
            int size = action.ArgumentsCount;
            if (size == 0) return sb.ToString();
            sb.Append("(");
            if (size > 0) sb.Append(varTween0.LogString());
            if (size > 1)
            {
                sb.Append(", ");
                sb.Append(varTween1.LogString());
            }
            if (size > 2)
            {
                sb.Append(", ");
                sb.Append(varTween2.LogString());
            }
            if (size > 3)
            {
                sb.Append(", ");
                sb.Append(varTween3.LogString());
            }
            if (size > 4)
            {
                sb.Append(", ");
                sb.Append(varTween4.LogString());
            }
            sb.Append(")");
#endif
            return sb.ToString();

        }
        #endregion

        #region Dependencies
        public List<int> Dependencies
        {
            get
            {
                List<int> dependencies = new List<int>();
#if SCS_PARAMED_EVENT
                int size = action.ArgumentsCount;
                if (size > 0) dependencies.AddRange(varTween0.Dependencies);
                if (size > 1) dependencies.AddRange(varTween1.Dependencies);
                if (size > 2) dependencies.AddRange(varTween2.Dependencies);
                if (size > 3) dependencies.AddRange(varTween3.Dependencies);
                if (size > 4) dependencies.AddRange(varTween4.Dependencies);
#endif
                return dependencies;
            }
        }
        public bool DependOn(int UID) { return Dependencies.Contains(UID); }
        public void SetForbiddenUID(int UID) { }
        #endregion
    }

#if SCS_PARAMED_EVENT
    #region Base Event Action
    [Serializable]
    public class BaseEventAction
    {
        [Serializable]
        public struct Argument
        {
            public Argument(bool value) { type = SceneVarType.BOOL; boolValue = value; intValue = 0; floatValue = 0f; stringValue = null; }
            public Argument(int value) { type = SceneVarType.INT; boolValue = false; intValue = value; floatValue = 0f; stringValue = null; }
            public Argument(float value) { type = SceneVarType.FLOAT; boolValue = false; intValue = 0; floatValue = value; stringValue = null; }
            public Argument(string value) { type = SceneVarType.STRING; boolValue = false; intValue = 0; floatValue = 0f; stringValue = value; }


            [SerializeField] SceneVarType type;

            [SerializeField] bool boolValue;
            [SerializeField] int intValue;
            [SerializeField] float floatValue;
            [SerializeField] string stringValue;

            public object Value
            {
                get
                {
                    return type switch
                    {
                        SceneVarType.BOOL => boolValue,
                        SceneVarType.INT => intValue,
                        SceneVarType.FLOAT => floatValue,
                        SceneVarType.STRING => stringValue,
                        _ => null,
                    };
                }
            }
        }

        [SerializeField] private string methodName;
        [SerializeReference] private UnityEngine.Object target;
        [SerializeField] private Argument[] arguments;
        public int ArgumentsCount => arguments.Length;
        public string MethodName => methodName;

        public BaseEventAction() { }
        public BaseEventAction(string methodName, UnityEngine.Object target, Argument[] arguments)
        {
            this.methodName = methodName;
            this.target = target;
            this.arguments = arguments;
        }

        public virtual bool Created { get; private set; } = false;

        public BaseEventAction CreateEventAction()
        {
            if (Created) return this;
            Created = true;

            if (arguments == null || arguments.Length == 0)
            {
                return CreateAction();
            }
            dynamic arg0 = arguments[0].Value;
            if (arguments.Length == 1)
            {
                return CreateAction(arg0);
            }
            dynamic arg1 = arguments[1].Value;
            if (arguments.Length == 2)
            {
                return CreateAction(arg0, arg1);
            }
            dynamic arg2 = arguments[2].Value;
            if (arguments.Length == 3)
            {
                return CreateAction(arg0, arg1, arg2);
            }
            dynamic arg3 = arguments[3].Value;
            if (arguments.Length == 4)
            {
                return CreateAction(arg0, arg1, arg2, arg3);
            }
            dynamic arg4 = arguments[4].Value;
            if (arguments.Length == 5)
            {
                return CreateAction(arg0, arg1, arg2, arg3, arg4);
            }
            return this;
        }

        private EventAction CreateAction()
        {
            Action action = (Action)Delegate.CreateDelegate(typeof(Action), target, methodName);
            return new EventAction(action);
        }
        private EventAction<T1> CreateAction<T1>(T1 arg0)
        {
            Action<T1> action = (Action<T1>)Delegate.CreateDelegate(typeof(Action<T1>), target, methodName);
            return new EventAction<T1>(action);
        }
        private EventAction<T1, T2> CreateAction<T1, T2>(T1 arg0, T2 arg1)
        {
            Action<T1, T2> action = (Action<T1, T2>)Delegate.CreateDelegate(typeof(Action<T1, T2>), target, methodName);
            return new EventAction<T1, T2>(action);
        }
        private EventAction<T1, T2, T3> CreateAction<T1, T2, T3>(T1 arg0, T2 arg1, T3 arg2)
        {
            Action<T1, T2, T3> action = (Action<T1, T2, T3>)Delegate.CreateDelegate(typeof(Action<T1, T2, T3>), target, methodName);
            return new EventAction<T1, T2, T3>(action);
        }
        private EventAction<T1, T2, T3, T4> CreateAction<T1, T2, T3, T4>(T1 arg0, T2 arg1, T3 arg2, T4 arg3)
        {
            Action<T1, T2, T3, T4> action = (Action<T1, T2, T3, T4>)Delegate.CreateDelegate(typeof(Action<T1, T2, T3, T4>), target, methodName);
            return new EventAction<T1, T2, T3, T4>(action);
        }
        private EventAction<T1, T2, T3, T4, T5> CreateAction<T1, T2, T3, T4, T5>(T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4)
        {
            Action<T1, T2, T3, T4, T5> action = (Action<T1, T2, T3, T4, T5>)Delegate.CreateDelegate(typeof(Action<T1, T2, T3, T4, T5>), target, methodName);
            return new EventAction<T1, T2, T3, T4, T5>(action);
        }

        public virtual void Invoke(List<object> values = null)
        {
            
        }

        public virtual int ParameterListSize => -1;
    }

    [Serializable]
    public class EventAction : BaseEventAction
    {
        private Action action;
        public override int ParameterListSize => 0;
        public override bool Created => true;

        public EventAction(Action _action)
        {
            action = _action;
        }

        public override void Invoke(List<object> values = null)
        {
            base.Invoke(values);
            action?.Invoke();
        }
    }
    [Serializable]
    public class EventAction<T1> : BaseEventAction
    {
        private Action<T1> action;

        public override int ParameterListSize => 1;
        public override bool Created => true;

        public EventAction(Action<T1> _action)
        {
            action = _action;
        }

        public override void Invoke(List<object> values = null)
        {
            base.Invoke(values);
            action?.Invoke((T1)values[0]);
        }
    }
    [Serializable]
    public class EventAction<T1, T2> : BaseEventAction
    {
        private Action<T1, T2> action;

        public override int ParameterListSize => 2;
        public override bool Created => true;

        public EventAction(Action<T1, T2> _action)
        {
            action = _action;
        }

        public override void Invoke(List<object> values = null)
        {
            base.Invoke(values);
            action?.Invoke((T1)values[0], (T2)values[1]);
        }
    }
    [Serializable]
    public class EventAction<T1, T2, T3> : BaseEventAction
    {
        private Action<T1, T2, T3> action;

        public override int ParameterListSize => 3;
        public override bool Created => true;

        public EventAction(Action<T1, T2, T3> _action)
        {
            action = _action;
        }

        public override void Invoke(List<object> values = null)
        {
            base.Invoke(values);
            action?.Invoke((T1)values[0], (T2)values[1], (T3)values[2]);
        }
    }
    [Serializable]
    public class EventAction<T1, T2, T3, T4> : BaseEventAction
    {
        private Action<T1, T2, T3, T4> action;

        public override int ParameterListSize => 4;
        public override bool Created => true;

        public EventAction(Action<T1, T2, T3, T4> _action)
        {
            action = _action;
        }

        public override void Invoke(List<object> values = null)
        {
            base.Invoke(values);
            action?.Invoke((T1)values[0], (T2)values[1], (T3)values[2], (T4)values[3]);
        }
    }
    [Serializable]
    public class EventAction<T1, T2, T3, T4, T5> : BaseEventAction
    {
        private Action<T1, T2, T3, T4, T5> action;

        public override int ParameterListSize => 5;
        public override bool Created => true;

        public EventAction(Action<T1, T2, T3, T4, T5> _action)
        {
            action = _action;
        }

        public override void Invoke(List<object> values = null)
        {
            base.Invoke(values);
            action?.Invoke((T1)values[0], (T2)values[1], (T3)values[2], (T4)values[3], (T5)values[4]);
        }
    }
    #endregion
#endif
}
