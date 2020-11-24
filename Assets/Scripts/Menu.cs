using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void SpecialBtn()
    {
        LoadScene("Test Scene");
    }

    public void QuitBtn()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void LoadScene(string _scene)
    {
        SceneManager.LoadScene(_scene);
    }
}