using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    private const string OVERWORLD_SCENE = "OverworldScene";

    public void StartGame()
    {
        SceneManager.LoadScene(OVERWORLD_SCENE);
    }

    public void QuitGame()
    {
        Application.Quit();

        // to stop Play Mode, but it won't be executed in a built game.
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
