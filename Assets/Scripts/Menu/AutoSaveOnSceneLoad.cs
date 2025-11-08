using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSaveOnSceneLoad : MonoBehaviour
{
    private static AutoSaveOnSceneLoad instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        //DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SaveManager.SaveCurrentScene();
    }
}