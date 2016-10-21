using UnityEngine;
using System.Collections;

public class AnimationManager : MonoBehaviour {

    private bool animateFlag;

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


}
