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
    private int counter = 0;

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
    private bool isDrawing = false;
    private Vector3 previousDrawnPoint, previousDrawnRotation;

    void Start()
    {
        animationManager = GameObject.Find("AnimationManager").GetComponent<AnimationManager>();
        _BodyManager = GameObject.Find("KinectBodySourceManager").GetComponent<BodySourceManager>();
        previousDrawnPoint = new Vector3(-1f, -1f, -1f);
        previousDrawnRotation = new Vector3(-1f, -1f, -1f);
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
    private void CreateGestureEffects(ulong trackingId, Vector3 Position)
    {
        if (previousDrawnPoint.z >= 0 && counter%3 == 0)
        {
            GameObject effect = GameObject.Instantiate(effects, Position, Quaternion.identity) as GameObject;
            effect.name = "EffectObject_" + trackingId.ToString();
            effect.transform.localPosition = Position;
            

            //現在の位置と過去の位置から、一軸の回転を算出する
            //とりあえずZ軸回り、床の場合はY軸回り。
            float rotationAngle = Mathf.Rad2Deg * Mathf.Atan2((Position.y - previousDrawnPoint.y),(Position.x - previousDrawnPoint.x));
            effect.transform.Rotate(0,0, rotationAngle);
            Debug.Log(rotationAngle);
           
        }
        counter++;
        previousDrawnPoint = Position;
    }
    public void ResetEffectsParameters()
    {
        isDrawing = false;
        previousDrawnPoint.x = -1f;
        previousDrawnPoint.y = -1f;
        previousDrawnPoint.z = -1f;
    }

}
