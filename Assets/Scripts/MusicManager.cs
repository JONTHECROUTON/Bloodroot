using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [SerializeField] int numberOfScenes;
    
    void Start()
    {
        if (numberOfScenes > 1)
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.activeSceneChanged += ChangeActiveScene;
            gameObject.tag = "Untagged";
        }
    }

    void ChangeActiveScene(Scene a, Scene b)
    {
        numberOfScenes--;
        if(numberOfScenes < 1)
        {
            SceneManager.activeSceneChanged -= ChangeActiveScene;
            Destroy(gameObject);
        }
        else
        {
            Destroy(GameObject.FindGameObjectWithTag("Music"));
        }

    }

}
