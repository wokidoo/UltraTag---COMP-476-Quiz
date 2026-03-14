using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    VisualElement _root;
    static bool gameIsPaused = false;

    void Start()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 1.0f;
        _root = pauseMenuUI.GetComponent<UIDocument>().rootVisualElement;
        _root.Q<VisualElement>("Container").Q<Button>("ResumeButton").clicked += OnResumeClicked;
        _root.Q<VisualElement>("Container").Q<Button>("RestartButton").clicked += OnRestartClicked;
        _root.Q<VisualElement>("Container").Q<Button>("QuitButton").clicked += OnQuitClicked;
        pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Check for pause key press
        {
            if (gameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    void Pause()
    {
        gameIsPaused = true;
        Time.timeScale = 0.0f;
        pauseMenuUI.SetActive(true);
        _root = pauseMenuUI.GetComponent<UIDocument>().rootVisualElement;
        _root.Q<VisualElement>("Container").Q<Button>("ResumeButton").clicked += OnResumeClicked;
        _root.Q<VisualElement>("Container").Q<Button>("RestartButton").clicked += OnRestartClicked;
        _root.Q<VisualElement>("Container").Q<Button>("QuitButton").clicked += OnQuitClicked;
    }

    void Resume()
    {
        gameIsPaused = false;
        Time.timeScale = 1.0f;
        pauseMenuUI.SetActive(false);
    }

    void OnResumeClicked()
    {
        Resume();
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
