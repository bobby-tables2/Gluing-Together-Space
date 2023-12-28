using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// BallGenScript generates an arbiturary GameObject on pressing space.
public class BallGenScript : MonoBehaviour {
    public string prefab_spawned; // prefab to be created. The variable contains the filename of a prefab file located in /Assets/Resources/Prefabs/
    private GameObject _prefab_spawned;
    public Vector3 position = new Vector3(0,4,0);    // The coordinates in space where GameObject is to be placed.
	public int spawning_limit = 1;

    void Start(){
        _prefab_spawned = Resources.Load(string.Format("Prefabs/{0}", prefab_spawned), typeof(GameObject)) as GameObject;
    }

	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space) && spawning_limit > 0)    // If the spacebar is held down
        {
			Instantiate(_prefab_spawned, position, Quaternion.identity);   // Create the specified gameobject at the specified coordinates with the default rotation.
            spawning_limit--;
        }
    }
}