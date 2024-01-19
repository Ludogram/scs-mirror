using Dhs5.SceneCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

public class AdditiveScenesTest : MonoBehaviour
{
    public bool loadOnAwake = false;

    private void Awake()
    {
        if (loadOnAwake)
        {
            LoadScene1();
            LoadScene2();
        }
    }


    public void LoadScene1()
    {
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
    }
    public void LoadScene2()
    {
        SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);
    }
    
    public void UnloadScene1()
    {
        SceneManager.UnloadSceneAsync(1);
    }
    public void UnloadScene2()
    {
        SceneManager.UnloadSceneAsync(2);
    }
}
