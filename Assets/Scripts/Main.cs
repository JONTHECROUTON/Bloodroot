using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

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

    Slider activeSlider;
    [SerializeField] float sliderValueFloat;
    string sliderName;
    string sliderValueString;
    List<Slider> sliderList;
    [SerializeField] float resetValue = 100;



    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        audioLink = GetComponent<AudioSource>();
        loaderLink = FindAnyObjectByType<Loader>();

        mainMenu = root.Query<VisualElement>("MainMenu");
        optionsMenu = root.Query<VisualElement>("OptionsMenu");
        optionsMenu.style.display = DisplayStyle.None;

        root.RegisterCallback<ClickEvent>(ClickAction);
        root.RegisterCallback<PointerOverEvent>(PointerOverAction);

        sliderList = root.Query<Slider>().ToList();
        foreach(Slider currentSlider in sliderList)
        {
            activeSlider = currentSlider;

            currentSlider.value = PlayerPrefs.GetFloat(currentSlider.name, resetValue);
            UpdateSliderLabel();

            if(currentSlider.name == "Master_volume")
            {
                AudioListener.volume = currentSlider.value / 100;
            }

        }
    }
    void Update()
    {
        
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
                root.RegisterCallback<ChangeEvent<float>>(ChangeAction);
            }
            else if(activeTarget.Contains("Close"))
            {
                optionsMenu.style.display = DisplayStyle.None;
                root.UnregisterCallback<ChangeEvent<float>>(ChangeAction);
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

    void ChangeAction(ChangeEvent<float> eventInfo)
    {
        activeSlider = (Slider)eventInfo.target;
        sliderValueFloat = activeSlider.value;

        PlayerPrefs.SetFloat(activeSlider.name, activeSlider.value);

        UpdateSliderLabel();

        if (activeSlider.name == "Master_Volume")
        {
            AudioListener.volume = sliderValueFloat / 100;
        }
    }

    void QuitGame()
    {
        Application.Quit();
        Debug.Log("quit Game");
    }

    void UpdateSliderLabel()
    {
        sliderValueString = activeSlider.value.ToString("0");
        sliderName = activeSlider.name;
        sliderName = sliderName.Replace("-", "/").Replace("_", " ");
        sliderName = sliderName + ": " + sliderValueString + "%";

        activeSlider.label = sliderName;
    }

    public void OnExit()
    {
        optionsMenu.style.display = DisplayStyle.None;
    }

    public void OnReload()
    {
        foreach (Slider currentSlider in sliderList)
        {
            activeSlider = currentSlider;
            
            PlayerPrefs.SetFloat(currentSlider.name, resetValue);
            currentSlider.value = resetValue;

            UpdateSliderLabel();
            AudioListener.volume = resetValue / 100;
        }
    }

}
