using UnityEngine;

public class PoliceSpawnerFollow : MonoBehaviour
{


    //Finding the player and setting an offset behind the player to lock the spawn area.
    [SerializeField] private Transform player;
    [SerializeField] private float zOffset = -25f;
    [SerializeField] private float xOffset = -1.3f;
    [SerializeField] private float spawnBlockHeight = 0.5f; //fixed height


    //Used to lock the Box behind the player on the X and Y assigned.
    private float fixedX;
    //private float fixedY;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        //Sets the locked area on Start.
        fixedX = transform.localScale.x;
       // fixedY = transform.localScale.y;
    }

    private void LateUpdate()
    {
        Vector3 pos = transform.position;

        pos.z = player.position.z + zOffset;

        // Apply fixed X + offset
        pos.x = fixedX + xOffset;

        pos.y = spawnBlockHeight;

        transform.position = pos;
    }
}
