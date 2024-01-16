using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dhs5.SceneCreation;
using TMPro;
using UnityEngine.Events;
using Mirror;

public class TestSceneObject : SceneObject
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private TextMeshProUGUI _text2;

    [SerializeField] private SceneVarTween _twin;

    public void OnValueChanged(SceneEventParam param)
    {
        _text.text = param.Var.IntValue.ToString();
    }

    protected override void RegisterSceneElements()
    {
        base.RegisterSceneElements();

        RegisterTween(nameof(_twin), _twin);
    }
    protected override void UpdateSceneVariables()
    {
        base.UpdateSceneVariables();

        _twin.SetUp(SceneVariablesSO, SceneVarType.FLOAT, false, false, true);
    }

    public void IncrementTwin()
    {
        if (NetworkServer.active)
        {
            _twin.FloatValue++;
        }
    }
}
