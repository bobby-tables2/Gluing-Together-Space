using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// BallGenScript generates an arbiturary GameObject on pressing space.
public class BallGenScript : MonoBehaviour {
    public GameObject prefab_spawned;
    public Vector3 position = new Vector3(0,4,0);    // The coordinates in space where GameObject is to be placed.
	public int spawning_limit = 1;

	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space) && spawning_limit > 0)    // If the spacebar is held down
        {
			Instantiate(prefab_spawned, position, Quaternion.identity);   // Create the specified gameobject at the specified coordinates with the default rotation.
            spawning_limit--;
        }
    }
}