using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectFollowsPath : MonoBehaviour {

    private List<Vector3> pathToFollow = new List<Vector3>();
    private Vector3 tempPosition = new Vector3(0,0,0);
    public float speed = 10f;
    public float elapsedTime = 0, finishTime = 3f;
    
    private int counter  = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
      
    }
    public void SetPathToFollow(List<Vector3> path)
    {
        pathToFollow = path;
        gameObject.transform.position = pathToFollow[0];
        tempPosition = pathToFollow[0];
       
        for (int i = 0; i<pathToFollow.Count; i++)
        {
            Hashtable moveHash = new Hashtable();
            moveHash.Add("position", pathToFollow[i]);
            moveHash.Add("time", 8f);
            moveHash.Add("delay", .5f);
            moveHash.Add("easeType", "easeInOutBack");
            moveHash.Add("oncomplete", "AnimationEnd");
            moveHash.Add("orienttopath", true);
            moveHash.Add("oncompletetarget", this.gameObject);
            iTween.MoveTo(gameObject,  moveHash);
        }
    }

    private void AnimationEnd()
    {
        Destroy(gameObject);
    }
}
