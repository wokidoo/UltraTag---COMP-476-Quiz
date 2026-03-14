using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class GroupAI : MonoBehaviour
{
    const float AGGRESSIVE = 10.0f;
    const float AVERAGE_SPEED = 6.0f;
    const float MOVE_CALMLY = 2.0f;
    const float MAX_SPEED = 10.0f;
    public float aggressiveness = 2.0f;

    public GameObject unitPrefab;

    public List<GameObject> units = new List<GameObject>();

    public UnityEvent<GroupAI> lost;

    // Update is called once per frame
    void Update()
    {
        Defuzzify();
    }

    public void SpawnNewUnit(Vector3 pos)
    {
        GameObject unit = Instantiate(unitPrefab, pos, transform.rotation);
        unit.gameObject.tag = this.gameObject.tag;

        IndividualAI individualAI = unit.GetComponent<IndividualAI>();

        if (individualAI != null)
            individualAI._groupAI = this;
        units.Add(unit);
        individualAI.died.AddListener(OnUnitDied);
    }

    void Defuzzify()
    {
        int friendlyCount = GetFriendCount();
        int enemyCount = GetEnemyCount();
        int targetCount = GetTargetCount();

        float hF = HighFriendly(friendlyCount);
        float lF = LowFriendly(friendlyCount);
        float hE = HighEnemy(enemyCount);
        float lE = LowEnemy(enemyCount);
        float hT = HighTarget(targetCount);
        float lT = LowTarget(targetCount);

        //Rule 1: "Many friends and many enemies"
        float r1 = Mathf.Min(hE,hF);
        //Rule 2: "Many enemies and few friends"
        float r2 = Mathf.Min(hE,lF);
        //Rule 3: "Many targets or few enemies"
        float r3 = Mathf.Max(lE,hT);
        //Rule 4: "Few targets or few enemies"
        float r4 = Mathf.Max(lE,lT);
        //Rule 5: "Few enemies and many targets or many friends"
        float r5 = Mathf.Min(Mathf.Max(hT,hF),1.0f-hE);
        //Rule 6: "Few enemies and few targets or many friends"
        float r6 = Mathf.Min(Mathf.Max(lT,hF),1.0f-hE);
        //Rule 7: "Few targets or few friends or few enemies"
        float r7 = Mathf.Max(Mathf.Max(lT,lF),lE);
        //Rule 8: "Many targets and few enemies"
        float r8 = Mathf.Min(hT, lE);
        //Rule 9: "Few friends and few targets"
        float r9 = Mathf.Min(lF, lT);

        float aggressiveFactor = Mathf.Max(r2, r5, r9);
        float averageFactor = Mathf.Max(r3, r6, r7);
        float calmFactor = Mathf.Max(r1, r4, r8);

        float total = aggressiveFactor + averageFactor + calmFactor;

        float result = (aggressiveFactor * AGGRESSIVE +
            averageFactor * AVERAGE_SPEED +
            calmFactor * MOVE_CALMLY
        )/total;

        aggressiveness = Mathf.Min(result,MAX_SPEED);
    }

    float HighFriendly(int count) => Mathf.Clamp01(0.2f*(count-2));
    float LowFriendly(int count)  => Mathf.Clamp01(-0.2f*(count-7));

    float HighTarget(int count) => Mathf.Clamp01(0.2f*(count-3));
    float LowTarget(int count)  => Mathf.Clamp01(-0.2f*(count-8));

    float HighEnemy(int count) => Mathf.Clamp01(0.2f*(count-1));
    float LowEnemy(int count)  => Mathf.Clamp01(-0.2f*(count-6));

    void OnUnitDied(GameObject unit)
    {
        units.Remove(unit);
        if (units.Count <= 0)
        {
            lost.Invoke(this);
        }
    }

    public int GetTargetCount()
    {
        int count = 0;
        string[] tags = GetTargetTags();
        GameObject[] arr = GameObject.FindGameObjectsWithTag(tags[0]);
        GameObject[] arr2 = GameObject.FindGameObjectsWithTag(tags[1]);
        GameObject[] combined = arr.Concat(arr2).ToArray();
                
        foreach (GameObject go in combined)
        {
            if (go.layer != LayerMask.NameToLayer("Unit"))
                continue;
            count++;
        }
        
        return count;
    }

    public int GetEnemyCount()
    {
        int count = 0;
        string[] tags = GetEnemyTags();
        GameObject[] arr = GameObject.FindGameObjectsWithTag(tags[0]);
        GameObject[] arr2 = GameObject.FindGameObjectsWithTag(tags[1]);
        GameObject[] combined = arr.Concat(arr2).ToArray();

        foreach (GameObject go in combined)
        {
            if (go.layer != LayerMask.NameToLayer("Unit"))
                continue;
            count++;
        }
        
        return count;
    }

    public int GetFriendCount()
    {
        int count = 0;
        GameObject[] arr = GameObject.FindGameObjectsWithTag(this.tag);

        foreach (GameObject go in arr)
        {
            if (go.layer != LayerMask.NameToLayer("Unit"))
                continue;
            count++;
        }

        return count;
    }

    public string[] GetEnemyTags()
    {
        switch (this.gameObject.tag)
        {
            case "rock":
                return new string[] {"spock","paper"};
            case "paper":
                return new string[] {"lizard","scissor"};
            case "scissor":
                return new string[] {"rock","spock"};
            case "spock":
                return new string[] {"lizard","paper"};
            case "lizard":
                return new string[] {"rock","scissor"};
            default:
                return new string[] {};
        }
    }

    public string[] GetTargetTags()
    {
        switch (this.gameObject.tag)
        {
            case "rock":
                return new string[] {"lizard","scissor"};
            case "paper":
                return new string[] {"rock","spock"};
            case "scissor":
                return new string[] {"lizard","paper"};
            case "spock":
                return new string[] {"rock","scissor"};
            case "lizard":
                return new string[] {"spock","paper"};
            default:
                return new string[] {};
        }
    }

    public UnitRelationship GetRelationship(string otherTag)
    {
        switch (this.gameObject.tag)
        {
            case "rock":
                if (otherTag == "scissor" || otherTag == "lizard")
                    return UnitRelationship.target;
                else if (otherTag == "paper" || otherTag == "spock")
                    return UnitRelationship.enemy;
                else
                    return UnitRelationship.friendly;
            case "paper":
                if (otherTag == "rock" || otherTag == "spock")
                    return UnitRelationship.target;
                else if (otherTag == "scissor" || otherTag=="lizard")
                    return UnitRelationship.enemy;
                else
                    return UnitRelationship.friendly;
            case "scissor":
                if (otherTag == "paper" || otherTag == "lizard")
                    return UnitRelationship.target;
                else if (otherTag == "rock" || otherTag=="spock")
                    return UnitRelationship.enemy;
                else
                    return UnitRelationship.friendly;
            case "spock":
                if (otherTag == "scissor" || otherTag == "rock")
                    return UnitRelationship.target;
                else if (otherTag == "paper" || otherTag=="lizard")
                    return UnitRelationship.enemy;
                else
                    return UnitRelationship.friendly;
            case "lizard":
                if (otherTag == "spock" || otherTag == "paper")
                        return UnitRelationship.target;
                else if (otherTag == "rock" || otherTag=="scissor")
                    return UnitRelationship.enemy;
                else
                    return UnitRelationship.friendly;
            default:
                return UnitRelationship.friendly;
        }
    }
}
