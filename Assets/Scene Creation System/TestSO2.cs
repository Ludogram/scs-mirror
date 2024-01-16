using Dhs5.SceneCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSO2 : BaseSceneObject
{
    public List<SceneEvent> onDead;
    public List<SceneListener> listener;

    public override string DisplayName => "Particular so";

    protected override void RegisterSceneElements()
    {
        RegisterEvent("onDead", onDead);
        RegisterListener("listener", listener);
    }

    protected override void UpdateSceneVariables()
    {
        Setup(onDead);
        Setup(listener);
    }
}
