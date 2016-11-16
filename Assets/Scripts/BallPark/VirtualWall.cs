using UnityEngine;
using System.Collections;
using OpenCVForUnity;

public class VirtualWall : MonoBehaviour {
    public GameObject KinectSensor;
    private Vector3 pointOfEffect;
    private Point pointOfEffect2d;
    private Vector3 testDummyPoint;
    public GameObject specialEffectParticle;
    public GameObject damagingProjectile;
	// Use this for initialization
	void Start () {
        testDummyPoint = new Vector3(20f, 20f, 500f);
        pointOfEffect2d = new Point();
        pointOfEffect = new Vector3(0, 0, 0);
	}
	
	// Update is called once per frame
	void Update () {
        pointOfEffect2d = KinectSensor.GetComponent<KinectCaptureDepth>().GetEffectPoint();

        if (pointOfEffect2d.x > 10 && pointOfEffect2d.y > 10)
        {
            pointOfEffect.z = 500f;
            //Debug.Log("Pos : " + pointOfEffect.x + ", " + pointOfEffect.y + ", " + pointOfEffect.z);
            pointOfEffect.x = (float)pointOfEffect2d.x - 480f;
            pointOfEffect.y = 270f - (float)pointOfEffect2d.y;
            GameObject.Instantiate(specialEffectParticle, pointOfEffect, Quaternion.identity);
            GameObject.Instantiate(damagingProjectile, pointOfEffect, Quaternion.identity);
        }
    }
}
