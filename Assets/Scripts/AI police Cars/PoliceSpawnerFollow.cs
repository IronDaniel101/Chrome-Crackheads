using UnityEngine;

public class PoliceSpawnerFollow : MonoBehaviour
{


    //Finding the player and setting an offset behind the player to lock the spawn area.
    [SerializeField] private Transform player;
    [SerializeField] private float zOffset = -25f;


    //Used to lock the Box behind the player on the X and Y assigned.
    private float fixedX;
    private float fixedY;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        //Sets the locked area on Start.
        fixedX = transform.localScale.x;
        fixedY = transform.localScale.y;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 pos = transform.position;


        //follow only players Z/offset.
        pos.z = player.position.z + zOffset;


        //Keep x/y fixed so it doesn't wiggle left/right with the car
        pos.x = fixedX;
        pos.y = fixedY;

        transform.position = pos;

    }
}
