using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dhs5.SceneCreation;
using TMPro;

public class TestSceneObject : SceneObject
{
    [SerializeField] private TextMeshProUGUI _text;

    public void OnValueChanged(SceneEventParam param)
    {
        _text.text = param.Var.IntValue.ToString();
    }
}
