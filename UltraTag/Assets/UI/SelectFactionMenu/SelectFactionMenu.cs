using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SelectFactionMenu : MonoBehaviour
{
    public GameObject gameManager;

    public GameObject pauseMenu;
    public GameObject UI;
    // Start is called before the first frame update
    void Start()
    {
        VisualElement root = UI.GetComponent<UIDocument>().rootVisualElement;
        root.Q<Button>("PaperButton").clicked += OnPaperSelected;
        root.Q<Button>("RockButton").clicked += OnRockSelected;
        root.Q<Button>("ScissorButton").clicked += OnScissorSelected;
        root.Q<Button>("SpockButton").clicked += OnSpockSelected;
        root.Q<Button>("LizardButton").clicked += OnLizardSelected;

    }

    void Deactive()
    {
        UI.SetActive(false);
        this.gameObject.SetActive(false);
    }

    void OnPaperSelected()
    {
        gameManager.GetComponent<GameManager>().SetPlayerFaction("paper");
        pauseMenu.gameObject.SetActive(true);
        Deactive();
    }

    void OnRockSelected()
    {
        gameManager.GetComponent<GameManager>().SetPlayerFaction("rock");
        pauseMenu.gameObject.SetActive(true);
        Deactive();
    }

    void OnScissorSelected()
    {
        gameManager.GetComponent<GameManager>().SetPlayerFaction("scissor");
        pauseMenu.gameObject.SetActive(true);
        Deactive();
    }

    void OnSpockSelected()
    {
        gameManager.GetComponent<GameManager>().SetPlayerFaction("spock");
        pauseMenu.gameObject.SetActive(true);
        Deactive();
    }

    void OnLizardSelected()
    {
        gameManager.GetComponent<GameManager>().SetPlayerFaction("lizard");
        pauseMenu.gameObject.SetActive(true);
        Deactive();
    }
}
