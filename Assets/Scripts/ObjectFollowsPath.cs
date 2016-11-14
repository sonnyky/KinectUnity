using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectFollowsPath : MonoBehaviour {

    private List<Vector3> pathToFollow = new List<Vector3>();
    private Vector3 tempPosition;
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
        Vector3[] Path = new Vector3[pathToFollow.Count]; 
        for(int i=0; i < pathToFollow.Count; i++)
        {
            Path[i] = pathToFollow[i];
        }

        Hashtable moveHash = new Hashtable();
        moveHash.Add("path", Path);
        moveHash.Add("speed", 2f);
        moveHash.Add("axis", "x");
        moveHash.Add("delay", .1f);
        moveHash.Add("easeType", "linear");
        moveHash.Add("oncomplete", "AnimationEnd");
        moveHash.Add("orienttopath", true);
        moveHash.Add("oncompletetarget", this.gameObject);
        iTween.MoveTo(gameObject,  moveHash);
        
    }

    private void AnimationEnd()
    {
        Destroy(gameObject);
    }
}
