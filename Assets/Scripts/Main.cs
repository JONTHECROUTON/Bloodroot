using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class Main : MonoBehaviour
{
    VisualElement root;
    AudioSource audioLink;
    [SerializeField] AudioClip clickSound;
    Loader loaderLink;
    string activeTarget;
    [SerializeField] AudioClip hoverSound;
    VisualElement mainMenu;
    VisualElement optionsMenu;

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        audioLink = GetComponent<AudioSource>();
        loaderLink = FindAnyObjectByType<Loader>();

        mainMenu = root.Query<VisualElement>("MainMenu");
        optionsMenu = root.Query<VisualElement>("OptionsMenu");
        optionsMenu.style.display = DisplayStyle.None;
    }
    void Update()
    {
        root.RegisterCallback<ClickEvent>(ClickAction);
        root.RegisterCallback<PointerOverEvent>(PointerOverAction);
    }

    void ClickAction(ClickEvent eventInfo)
    {
        activeTarget = eventInfo.target.ToString();
        if(activeTarget.Contains("Button "))
        {
            audioLink.PlayOneShot(clickSound);
            if(activeTarget.Contains("Play"))
            {
                loaderLink.LoadSceneWithDelay(2);
            }
            else if(activeTarget.Contains("Quit"))
            {
                Invoke(nameof(QuitGame), 2);
            }
            else if(activeTarget.Contains("Settings"))
            {
                optionsMenu.style.display = DisplayStyle.Flex;
            }
            else if(activeTarget.Contains("Close"))
            {
                optionsMenu.style.display = DisplayStyle.None;
            }
        }
        //Debug.Log(eventInfo);
    }

    void PointerOverAction(PointerOverEvent eventInfo)
    {
        //Debug.Log(eventInfo.target);
        activeTarget = eventInfo.target.ToString();
        if(activeTarget.Contains("Button "))
        {
            audioLink.PlayOneShot(hoverSound);
        }
    }

    void QuitGame()
    {
        Application.Quit();
        Debug.Log("quit Game");
    }

    public void OnExit()
    {
        optionsMenu.style.display = DisplayStyle.None;
    }

}
