using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager: MonoBehaviour
{

    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    public GameObject[] spawnLocations;
    public GameObject paperFaction;
    public GameObject rockFaction;
    public GameObject scissorFaction;
    public GameObject spockFaction;
    public GameObject lizardFaction;
    public GroupAI playerFaction;

    bool gameHasStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        paperFaction.GetComponent<GroupAI>().lost.AddListener(OnFactionDied);
        rockFaction.GetComponent<GroupAI>().lost.AddListener(OnFactionDied);
        scissorFaction.GetComponent<GroupAI>().lost.AddListener(OnFactionDied);
        spockFaction.GetComponent<GroupAI>().lost.AddListener(OnFactionDied);
        lizardFaction.GetComponent<GroupAI>().lost.AddListener(OnFactionDied);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && gameHasStarted == false)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if(playerFaction == null)
                    return;
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Ground"))
                    return;

                playerFaction.SpawnNewUnit(hit.point + new Vector3(0f,3f,0f));
                if (playerFaction.units.Count == 10)
                    StartGame();
            }
        }else if(Input.GetMouseButtonDown(1) && gameHasStarted == false)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if(playerFaction == null)
                    return;
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Unit"))
                    return;
                if (hit.collider.gameObject.tag != playerFaction.tag)
                    return;
                
                playerFaction.units.Remove(hit.collider.gameObject);
                Destroy(hit.collider.gameObject);
            }
        }
    }

    public void SetPlayerFaction(string tag)
    {
        switch (tag)
        {
            case "paper":
                playerFaction = paperFaction.GetComponent<GroupAI>();
                break;
            case "rock":
                playerFaction = rockFaction.GetComponent<GroupAI>();
                break;
            case "scissor":
                playerFaction = scissorFaction.GetComponent<GroupAI>();
                break;
            case "spock":
                playerFaction = spockFaction.GetComponent<GroupAI>();
                break;
            case "lizard":
                playerFaction = lizardFaction.GetComponent<GroupAI>();
                break;
        }
    }

    void StartGame()
    {
        gameHasStarted = true;
        StartCoroutine(SpawnOtherFactions());
    }

    IEnumerator SpawnOtherFactions()
    {
        List<GameObject> factions = new List<GameObject>{paperFaction,rockFaction,scissorFaction,spockFaction,lizardFaction};
        factions.Remove(playerFaction.gameObject);
        
        for(int i = 0; i < 10; i++)
        {
            float angle = i/10f*Mathf.PI;
            foreach (GameObject group in factions)
            {
                int idx = factions.IndexOf(group);
                Vector3 pos = new Vector3(Mathf.Cos(angle),0.0f,Mathf.Sin(angle));
                group.GetComponent<GroupAI>().SpawnNewUnit(spawnLocations[idx].transform.position+pos);
            }
            yield return new WaitForSeconds(0.25f);
        }
    }


    void GameOver(string message)
    {
        Time.timeScale = 0f;
        pauseMenu.SetActive(false);
        gameOverMenu.SetActive(true);
        gameOverMenu.GetComponent<GameOverMenu>().SetMessage(message);
    }

    void OnFactionDied(GroupAI died)
    {
        GameOver(died.gameObject.tag + " lost!");
    }
}
