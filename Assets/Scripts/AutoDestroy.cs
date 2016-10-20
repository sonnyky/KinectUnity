using UnityEngine;
using System.Collections;

public class AutoDestroy : MonoBehaviour {
    private float lifetime = 5.0f;
    private float speed = 0.1f;
    private BodySourceView bodiesInScene;
    private string currentObjecTrackingId;
    private bool animate = false;

	// Use this for initialization
	void Start ()
    {
        bodiesInScene = GameObject.Find("KinectBodySourceView").GetComponent<BodySourceView>();
        string[] temp = gameObject.name.Split("_"[0]);
        currentObjecTrackingId = temp[1];
        Destroy(gameObject, lifetime);
	}
	
	// Update is called once per frame
	void Update () {
        
        if (animate)
        {
            MoveObjectToNewPosition();
        }
        
    }
    private void MoveObjectToNewPosition()
    {
        Vector3 newPosition = gameObject.transform.localPosition;
        newPosition.x += speed;
        //newPosition.y = Mathf.Sin(newPosition.x);
        newPosition.z = 0;
        gameObject.transform.localPosition = newPosition;
    }

    public void SetupAnimation(bool animate_val)
    {
        animate = animate_val;
    }
   
}
