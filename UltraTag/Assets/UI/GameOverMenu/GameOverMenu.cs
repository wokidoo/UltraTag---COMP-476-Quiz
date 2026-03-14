using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameOverMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        root.Q<Button>("RestartButton").clicked += OnRestartClicked;
        root.Q<Button>("QuitButton").clicked += OnQuitClicked;
    }

    public void SetMessage(string message)
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        root.Q<Label>("GameOverMessage").text = message;
    }

    void OnRestartClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnQuitClicked()
    {
        Application.Quit();
    }
}
