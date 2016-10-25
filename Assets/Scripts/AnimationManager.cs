using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationManager : MonoBehaviour {

    public GameObject animationObject;

    private int numOfAnimatedObjects = 3;
    private bool animateFlag;
    private Dictionary<int, List<Vector3>> waypointsGrouped = new Dictionary<int, List<Vector3>>();
    private int dictionaryKeyForWayPoints = 0;

    // Use this for initialization
    void Start () {
        animateFlag = false;
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void EnableAnimation(bool enableFlag, string trackingId)
    {
        //trackingId is not used right now but will be necessary for multiple bodies
        animateFlag = enableFlag;
    }

    public bool GetAnimationFlag(string trackingId)
    {
        //trackingId is not used right now but will be necessary for multiple bodies
        return animateFlag;
    }

    public void AddWayPoint(List<Vector3> wayPoint)
    {
        waypointsGrouped.Add(dictionaryKeyForWayPoints++, wayPoint);
        GameObject truck = GameObject.Instantiate(animationObject);
        truck.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        if (wayPoint[0].x < wayPoint[wayPoint.Count-1].x)
        {
            truck.transform.Rotate( new Vector3(0, 90f, 0));
        }else
        {
            truck.transform.Rotate(new Vector3(0, -90f, 0));
        }
        truck.GetComponent<ObjectFollowsPath>().SetPathToFollow(wayPoint);
    }

    private void StartAnimation()
    {
        
    }
}
