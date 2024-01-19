using Dhs5.SceneCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSO2 : BaseSceneObject
{
    // D�claration d'un �v�nement simple
    public List<SceneEvent> onDeadEvent;

    // D�claration d'un listener simple
    public List<SceneListener> simpleListener;
    // D�claration d'un listener specific
    public List<SceneSpecificListener> specificListener;

    // D�claration de SceneVarTween (pas twin car je suis d�bile)
    public SceneVarTween intVarTween;
    public SceneVarTween boolVarTween;

    // Changement du Display Name
    public override string DisplayName => "Particular Scene Object";


    // Registration des diff�rents �l�ments
    protected override void RegisterSceneElements()
    {
        // Registration d'un event
        RegisterEvent("onDead", onDeadEvent);

        // Registration de listeners
        RegisterListener(nameof(simpleListener), simpleListener);
        RegisterListener("SpecificListener", specificListener);

        // Registration de tweens
        RegisterTweens(("IntTween", intVarTween), ("BoolTween", boolVarTween));
        // Comme pour listener et events on peut register/setup plusieurs �l�ments
        // � la fois si ils sont du m�me type
    }

    // Setup des diff�rents �l�ments
    protected override void UpdateSceneVariables()
    {
        Setup(onDeadEvent);
        Setup(simpleListener);
        Setup(specificListener);

        // le setup des tweens est particuliers,
        // il faut pr�ciser son type,
        // si il peut �tre static,
        // �tre "anyVar" (= any type),
        // et si il peut �tre d�sactiv�
        //                                         type         static   anyVar   canBeIncative
        intVarTween.SetUp(SceneVariablesSO, SceneVarType.INT,   false,   false,      true);
        boolVarTween.SetUp(SceneVariablesSO, SceneVarType.BOOL, true,    false,      false);
    }

    // Awake extension
    protected override void OnSceneObjectAwake()
    {
        base.OnSceneObjectAwake();

        // Set l'�v�nement appel� par le specific listener
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


    // Cr�ation d'un �v�nement �coutable par un specific listener
    private void OnSpecificListenerNotified(SceneEventParam param) // Le param�tre est obligatoire
    {

    }
}
