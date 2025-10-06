using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessLevelHandler : MonoBehaviour
{

    [SerializeField]
    GameObject[] sectionsPrefabs;

    GameObject[] sectionsPool = new GameObject[20];

    GameObject[] sections = new GameObject[10];

    Transform playerCarTransform;

    WaitForSeconds waitFor100ms = new WaitForSeconds(0.1f);
    
    const float sectionLength = 26;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Gets location of player
        playerCarTransform = GameObject.FindGameObjectWithTag("Player").transform;

        int prefabIndex = 0;

        //Creating Pool for Endless Sections
        for (int i = 0; i < sectionsPool.Length; i++)
        {
            sectionsPool[i] = Instantiate(sectionsPrefabs[prefabIndex]);
            sectionsPool[i].SetActive(false);

            prefabIndex++;

            //Loop the prefab index if prefabs run out
            if (prefabIndex > sectionsPrefabs.Length - 1)
                prefabIndex = 0;

        }

        //Add the Starting Sections for the road
        for (int i = 0; i < sections.Length; i++)
        {
            //Get a random Section
            GameObject randomSection = GetRandomSectionFromPool();

            //Move it into position and set it to active
            randomSection.transform.position = new Vector3(sectionsPool[i].transform.position.x, 0, i * sectionLength);
            randomSection.SetActive(true);

            //Set the sections in the array
            sections[i] = randomSection;
        }

        StartCoroutine(UpdateLessOftenCO());
    }

    IEnumerator UpdateLessOftenCO()
    {
        while (true)
        {
            UpdateSectionPositions();
            yield return waitFor100ms;
        }
    }

    void UpdateSectionPositions()
    {
        for (int i = 0; i < sections.Length; i++)
        {
            //Check if section is too far behind
            if (sections[i].transform.position.z - playerCarTransform.position.z < -sectionLength)
            {
                //Store the position of the section and disable it
                Vector3 lastSectionPosition = sections[i].transform.position;
                sections[i].SetActive(false);

                //Get a new section & enable it & move it forward
                sections[i] = GetRandomSectionFromPool();

                //Move the new section into place and active it 
                sections[i].transform.position = new Vector3(lastSectionPosition.x, 0, lastSectionPosition.z + sectionLength * sections.Length);
                sections[i].SetActive(true);
            }
        }
    }

    GameObject GetRandomSectionFromPool()
    {
        //Pick a random index and pray
        int randomIndex = Random.Range(0, sectionsPool.Length);

        bool isNewSectionFound = false;

        while(!isNewSectionFound)
        {
            //Check if the section is not active, in that case we've found a section
            if (!sectionsPool[randomIndex].activeInHierarchy)
                isNewSectionFound = true;
            else
            {
                //If it was active we need to try to find another section so we increase the index
                randomIndex++;

                //Ensure that we loop around if we reach the end of the array
                if (randomIndex > sectionsPool.Length - 1)
                    randomIndex = 0;
            }
        }

        return sectionsPool[randomIndex];
    }
    
}
