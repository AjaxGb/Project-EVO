using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCam : MonoBehaviour {

    public Vector2 origin;
    public GameObject player;

    public float sensivity = 0.2f;

	// Use this for initialization
	void Start () {
        origin = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = origin + (((Vector2)player.transform.position - origin) * sensivity);
	}
}
