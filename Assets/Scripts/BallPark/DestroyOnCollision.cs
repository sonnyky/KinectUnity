using UnityEngine;
using System.Collections;

public class DestroyOnCollision : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionStay(Collision col)
    {
        Destroy(gameObject);
    }
    void OnCollisionEnter(Collision col)
    {
        Destroy(gameObject);
    }
}
