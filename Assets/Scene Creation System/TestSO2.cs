using Dhs5.SceneCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSO2 : BaseSceneObject
{
    // Déclaration d'un évènement simple
    public List<SceneEvent> onDeadEvent;

    // Déclaration d'un listener simple
    public List<SceneListener> simpleListener;
    // Déclaration d'un listener specific
    public List<SceneSpecificListener> specificListener;

    // Déclaration de SceneVarTween (pas twin car je suis débile)
    public SceneVarTween intVarTween;
    public SceneVarTween boolVarTween;

    // Changement du Display Name
    public override string DisplayName => "Particular Scene Object";


    // Registration des différents éléments
    protected override void RegisterSceneElements()
    {
        // Registration d'un event
        RegisterEvent("onDead", onDeadEvent);

        // Registration de listeners
        RegisterListener(nameof(simpleListener), simpleListener);
        RegisterListener("SpecificListener", specificListener);

        // Registration de tweens
        RegisterTweens(("IntTween", intVarTween), ("BoolTween", boolVarTween));
        // Comme pour listener et events on peut register/setup plusieurs éléments
        // à la fois si ils sont du même type
    }

    // Setup des différents éléments
    protected override void UpdateSceneVariables()
    {
        Setup(onDeadEvent);
        Setup(simpleListener);
        Setup(specificListener);

        // le setup des tweens est particuliers,
        // il faut préciser son type,
        // si il peut être static,
        // être "anyVar" (= any type),
        // et si il peut être désactivé
        //                                         type         static   anyVar   canBeIncative
        intVarTween.SetUp(SceneVariablesSO, SceneVarType.INT,   false,   false,      true);
        boolVarTween.SetUp(SceneVariablesSO, SceneVarType.BOOL, true,    false,      false);
    }

    // Awake extension
    protected override void OnSceneObjectAwake()
    {
        base.OnSceneObjectAwake();

        // Set l'évènement appelé par le specific listener
        specificListener.SetEvents(OnSpecificListenerNotified);
    }
    // On Validate Extension
    protected override void OnSceneObjectValidate()
    {
        base.OnSceneObjectValidate();
    }
    // OnEnable extension
    protected override void OnSceneObjectEnable()
    {
        base.OnSceneObjectEnable();
    }
    // OnDisable extension
    protected override void OnSceneObjectDisable()
    {
        base.OnSceneObjectDisable();
    }


    // Création d'un évènement écoutable par un specific listener
    private void OnSpecificListenerNotified(SceneEventParam param) // Le paramètre est obligatoire
    {

    }
}
