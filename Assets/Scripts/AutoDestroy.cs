using UnityEngine;
using System.Collections;

public class AutoDestroy : MonoBehaviour {
    private bool animate = false;
    private bool animateFinish = false;
    private AnimationManager animationManager;
    private BodySourceView bodiesInScene;
    private string currentObjectTrackingId;
    private float lifetime = 5.0f;
    private float speed = 0.1f;

	// Use this for initialization
	void Start ()
    {
        animationManager = GameObject.Find("AnimationManager").GetComponent<AnimationManager>();
        bodiesInScene = GameObject.Find("KinectBodySourceView").GetComponent<BodySourceView>();
        string[] temp = gameObject.name.Split("_"[0]);
        currentObjectTrackingId = temp[1];
        Destroy(gameObject, lifetime);
	}
	
	// Update is called once per frame
	void Update () {
        
        if (animationManager.GetAnimationFlag(currentObjectTrackingId) | animateFinish)
        {
            MoveObjectToNewPosition();
            animateFinish = true;
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
}
