using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    [SerializeField] float sceneTimer;
    [SerializeField] int sceneNumber;

    void Start()
    {
       if(sceneTimer > 0)
        {
            LoadSceneWithDelay(sceneTimer);
        }
    }
    void Update()
    {
        
    }

    public void LoadSceneWithDelay(float delayTime)
    {
        Invoke(nameof(SceneLoader), delayTime);
    }

    public void SceneLoader()
    {
        //Debug.Log("Move to Next Scene");
        SceneManager.LoadScene(sceneNumber);

    }

    public void OnSkip()
    {
        SceneLoader();
    }

    public void OnExit()
    {
        SceneLoader();
    }

}
