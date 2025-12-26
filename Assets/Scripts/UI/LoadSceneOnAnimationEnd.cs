using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnAnimationEnd : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}