﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;

public class BodySourceView : MonoBehaviour 
{
    public Material BoneMaterial;
    public GameObject BodySourceManager;

    //手についているエフェクト
    public GameObject TrackObject;
    private List<MeteorHand> meteors = new List<MeteorHand>();
    private bool newMeteorIsFound = false;

    private VisualGestureBuilderFrameSource _VGB_Source;

    //描かれる軌跡
    public GameObject effects;

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
    
    void Update () 
    {
        if (BodySourceManager == null)
        {
            return;
        }
        
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }
        
        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }

        if (_VGB_Source == null) _VGB_Source = _BodyManager.GetRecordedGesturesData();

        List<ulong> trackedIds = new List<ulong>();
        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
              }
                
            if(body.IsTracked)
            {
                trackedIds.Add(body.TrackingId);
                _VGB_Source.TrackingId = body.TrackingId;
                // 手についているエフェクト
                if(meteors.Count == 0)
                {
                    MeteorHand thisMeteor;
                    thisMeteor.MeteorObject = (GameObject)Instantiate(TrackObject);
                    thisMeteor.MeteorObject.name = "MeteorObject" + body.TrackingId.ToString();
                    thisMeteor.TrackingId = body.TrackingId;
                    meteors.Add(thisMeteor);
                }

                foreach (MeteorHand meteor in meteors)
                {
                    if (meteor.TrackingId == body.TrackingId)
                    {
                        //Found a meteor that corresponds to this body
                        newMeteorIsFound = false;
                        break;
                    }else
                    {
                        // This body is not yet associated with any meteors
                        newMeteorIsFound = true;
                    }
                }

                    foreach (MeteorHand meteor in meteors)
                {
                    if (newMeteorIsFound == false)
                    {
                        SetMeteorObjectPos(meteor.MeteorObject, GetVector3FromJoint(body.Joints[Kinect.JointType.HandTipRight]));
                        GameObject effect = GameObject.Instantiate(effects);
                        effect.transform.localPosition = GetVector3FromJoint(body.Joints[Kinect.JointType.HandTipRight]);
                        break;
                    }
                    else
                    {
                        Debug.Log("meteor tracking id is not present");
                        MeteorHand thisMeteor;
                        thisMeteor.MeteorObject = (GameObject)Instantiate(TrackObject);
                        thisMeteor.MeteorObject.name = "MeteorObject" + body.TrackingId.ToString();
                        thisMeteor.TrackingId = body.TrackingId;
                        meteors.Add(thisMeteor);
                    }
                }
              
            }
        }
        
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
        
        // First delete untracked bodies
        foreach(ulong trackingId in knownIds)
        {
            if(!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                Destroy(GameObject.Find("MeteorObject" + trackingId.ToString()));
                _Bodies.Remove(trackingId);
                _BodyManager.SetVGBReaderPauseState(true);
            }
        }

        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
            }
            
            if(body.IsTracked)
            {
                if(!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }
                
                RefreshBodyObject(body, _Bodies[body.TrackingId]);
                _BodyManager.SetVGBReaderPauseState(false);
            }
        }
    }

    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);        
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.SetVertexCount(2);
            lr.material = BoneMaterial;
            lr.SetWidth(0.05f, 0.05f);
            
            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
        }
        
        return body;
    }
    
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;
            
            if(_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
            }
            
            Transform jointObj = bodyObject.transform.FindChild(jt.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);
            
            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if(targetJoint.HasValue)
            {
                lr.SetPosition(0, jointObj.localPosition);
                lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
                lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
            }
            else
            {
                lr.enabled = false;
            }
        }
    }
    
    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;

        case Kinect.TrackingState.Inferred:
            return Color.red;

        default:
            return Color.black;
        }
    }
    
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
    private void  SetMeteorObjectPos(GameObject MeteorObject, Vector3 RightHandPos)
    {
        if (MeteorObject == null)
        {
            return;
        }
        MeteorObject.transform.localPosition = RightHandPos;
    }
    public struct MeteorHand
    {
        public ulong TrackingId;
        public GameObject MeteorObject;
    }

    public struct EventArgs
    {
        public string name;
        public float confidence;

        public EventArgs(string _name, float _confidence)
        {
            name = _name;
            confidence = _confidence;
        }
    }
   

}
