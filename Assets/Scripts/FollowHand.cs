using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using OpenCVForUnity;

public class FollowHand : MonoBehaviour
{
    private AnimationManager animationManager;

    private string currentObjectTrackingId;
    //描かれる軌跡
    public GameObject effects;
    private Dictionary<string, Vector3[]> trainPoints = new Dictionary<string, Vector3[]>();
    private List<Vector3> tempTrainPoints = new List<Vector3>(); 

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;
    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },

        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },

        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };

    void Start()
    {
        animationManager = GameObject.Find("AnimationManager").GetComponent<AnimationManager>();
        _BodyManager = GameObject.Find("KinectBodySourceManager").GetComponent<BodySourceManager>();

        string[] temp = gameObject.name.Split("_"[0]);
        currentObjectTrackingId = temp[1];
    }

    void Update()
    {
        if (_BodyManager == null)
        {
            return;
        }
        if (_BodyManager == null)
        {
            return;
        }

        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }
  
        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked && body.TrackingId.ToString() == currentObjectTrackingId)
            {
                Vector3 handTipPosition = GetVector3FromJoint(body.Joints[Kinect.JointType.HandTipRight]);
                gameObject.transform.localPosition = handTipPosition;
                if (handTipPosition.z > 6.0f)
                {
                    if (tempTrainPoints.Count > 0)
                    {
                        Debug.Log(tempTrainPoints.Count);
                        animationManager.AddWayPoint(tempTrainPoints);
                        tempTrainPoints.Clear();
                    }
                    continue;
                }else
                {
                    handTipPosition.z = 0;
                    tempTrainPoints.Add(handTipPosition);
                    CreateGestureEffects(body.TrackingId, handTipPosition);
                }
            }
        }
    }
   
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
    private void SetObjectPos(GameObject Object, Vector3 newPos, ulong trackingId)
    {
        if (Object == null)
        {
            return;
        }
        Object.transform.localPosition = newPos;
    }
    private void CreateGestureEffects(ulong trackingId, Vector3 Position)
    {
        GameObject effect = GameObject.Instantiate(effects);
        effect.name = "EffectObject_" + trackingId.ToString();
        effect.transform.localPosition = Position;
    }

}
