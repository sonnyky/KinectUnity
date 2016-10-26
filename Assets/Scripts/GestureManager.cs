using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using OpenCVForUnity;

public class GestureManager : MonoBehaviour
{
    private AnimationManager animationManager;
    public Material BoneMaterial;
    public GameObject BodySourceManager;

    //Gesture detection
    private VisualGestureBuilderFrameSource _VGB_Source;
    private IDictionary<Gesture, DiscreteGestureResult> detectedGestures;
    private bool TargetGesturesToDetect = false; //This maybe multiple in the future
    private float effectiveDistance = 9f;
    private BodySourceManager _BodyManager;

    void Start()
    {
        animationManager = GameObject.Find("AnimationManager").GetComponent<AnimationManager>();
    }

    void Update()
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
        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null)
            {
                _BodyManager.SetVGBReaderPauseState(true);
                continue;
            }
            if (body.IsTracked)
            {
                _BodyManager.SetVGBReaderPauseState(false);
                trackedIds.Add(body.TrackingId);
                _VGB_Source.TrackingId = body.TrackingId;
                DetectTargetGestureOnBodies("SwipeRight", body.TrackingId.ToString());
            }
        }
    }

    public bool DetectTargetGestureOnBodies(string gestureName, string bodyTrackingId)
    {
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
                            Debug.Log("Found gestures");
                            gestureDetected = true;
                        }
                    }
                }
            }
        }


        return gestureDetected;
    }
}
