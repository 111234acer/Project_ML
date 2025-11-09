using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMgr : MonoBehaviour
{
    public string multiScene = "PhotonLobby";
    public string singleScene = "CharacterSelectScene_TD";

    public void OnClickCP()
    {
        SceneManager.LoadScene(multiScene);
    }

    public void OnClickTD()
    {
        SceneManager.LoadScene(singleScene);
    }

    public void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif 
    }
}
