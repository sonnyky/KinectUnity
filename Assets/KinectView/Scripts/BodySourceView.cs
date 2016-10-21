using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;

public class BodySourceView : MonoBehaviour 
{
    private AnimationManager animationManager;

    public Material BoneMaterial;
    public GameObject BodySourceManager;

    //手についているエフェクト
    public GameObject TrackObject;
    private List<MeteorHand> meteors = new List<MeteorHand>();
    private bool newMeteorIsFound = false;

    //Gesture detection
    private VisualGestureBuilderFrameSource _VGB_Source;
    private IDictionary<Gesture, DiscreteGestureResult> detectedGestures;
    private IDictionary<Gesture, ContinuousGestureResult> detectedContinuousGestures;
    private bool TargetGesturesToDetect = false; //This maybe multiple in the future
    private float effectiveDistance = 9f;

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

    void Start()
    {
        animationManager = GameObject.Find("AnimationManager").GetComponent<AnimationManager>();
    }
    
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

         _VGB_Source = _BodyManager.GetRecordedGesturesData();
        detectedGestures = _BodyManager.GetDetectedGestures();
        detectedContinuousGestures = _BodyManager.GetDetectedContinuousGestures();
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
                TargetGesturesToDetect = DetectTargetGestureOnBodies("SwipeRight", body.TrackingId.ToString());

                if (TargetGesturesToDetect && body.HandRightState == Kinect.HandState.Open)
                {
                    animationManager.EnableAnimation(true, body.TrackingId.ToString());
                }else
                {
                    animationManager.EnableAnimation(false, body.TrackingId.ToString());
                }

                // 手についているエフェクト
                if(meteors.Count == 0)
                {
                    MeteorHand thisMeteor;
                    thisMeteor.MeteorObject = (GameObject)Instantiate(TrackObject);
                    thisMeteor.MeteorObject.name = "MeteorObject" + body.TrackingId.ToString();
                    thisMeteor.TrackingId = body.TrackingId;
                    meteors.Add(thisMeteor);
                }
                MeteorHand newFoundMeteor;
                newFoundMeteor.TrackingId = 0;
                newFoundMeteor.MeteorObject = null;
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
                        Vector3 hand_tip_pos = GetVector3FromJoint(body.Joints[Kinect.JointType.HandTipRight]);
                        Vector3 effect_pos = hand_tip_pos;
                        SetMeteorObjectPos(meteor.MeteorObject, GetVector3FromJoint(body.Joints[Kinect.JointType.HandTipRight]));

                        //Create effects
                        if (hand_tip_pos.z <= effectiveDistance)
                        {
                            if (TargetGesturesToDetect)
                            {
                                effect_pos.z = 0;
                                GameObject effect = GameObject.Instantiate(effects);
                                effect.name = "Effect_" + body.TrackingId.ToString();
                                effect.transform.localPosition = effect_pos;
                            }else
                            {
                                effect_pos.z = 0;
                                GameObject effect = GameObject.Instantiate(effects);
                                effect.name = "Effect_" + body.TrackingId.ToString();
                                effect.transform.localPosition = effect_pos;
                            }                       
                        }
                        break;
                    }
                    else
                    {
                        Debug.Log("meteor tracking id is not present");
                        newFoundMeteor.MeteorObject = (GameObject)Instantiate(TrackObject);
                        newFoundMeteor.MeteorObject.name = "MeteorObject" + body.TrackingId.ToString();
                        newFoundMeteor.TrackingId = body.TrackingId;
                    }
                }
                if(newFoundMeteor.TrackingId != 0)
                {
                    meteors.Add(newFoundMeteor);
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

                MeteorHand meteorToRemove;
                meteorToRemove.TrackingId = 0;
                meteorToRemove.MeteorObject = null;
                foreach (MeteorHand meteor in meteors)
                {
                    if(meteor.TrackingId == trackingId)
                    {
                        meteorToRemove = meteor;
                    }
                }
                if (meteorToRemove.TrackingId != 0) {
                    meteors.Remove(meteorToRemove);
                }
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
                
                RefreshBodyObject(body, _Bodies[body.TrackingId]); //For drawing stick figure
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
   
    public bool DetectTargetGestureOnBodies(string gestureName, string bodyTrackingId)
    {
        //Debug.Log("Detecting gestures");
        bool gestureDetected = false;
        foreach (Gesture gesture in _VGB_Source.Gestures)
        {
            if (detectedGestures != null)
            {
                if (gesture.GestureType == GestureType.Discrete)
                {
                    DiscreteGestureResult result = null;
                    detectedGestures.TryGetValue(gesture, out result);
                    if (result != null)
                    {
                        if (gesture.Name == gestureName && _VGB_Source.TrackingId.ToString() == bodyTrackingId && result.Detected)
                        {
                           // Debug.Log("Detected gestures");
                            gestureDetected = true;
                        }
                    }
                }
            }
            /*
            if (detectedContinuousGestures != null)
            {
                if (gesture.GestureType == GestureType.Continuous)
                {
                    ContinuousGestureResult result = null;
                    detectedContinuousGestures.TryGetValue(gesture, out result);
                  
                    Debug.Log(gesture.Name);
                    if (result != null)
                    {
                        Debug.Log(result.Progress);
                        if (gesture.Name == gestureName && _VGB_Source.TrackingId.ToString() == bodyTrackingId)
                        {
                            
                            gestureDetected = false;
                        }
                    }
                }
            }
            */

        }

     
        return gestureDetected;
    }

}
