using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;
using Kinect = Windows.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;


public class BodySourceManager : MonoBehaviour 
{
    private KinectSensor _Sensor;
    private BodyFrameReader _Reader;
    private Body[] _Data = null;

    //カスタムジェスチャーファイル
    private string gestureDatabaseFileName = "SwipeRight.gbd";
    private string gestureName = "Circular";
    private VisualGestureBuilderFrameSource _VGB_Source;
    private VisualGestureBuilderFrameReader _VGB_Reader;
    private VisualGestureBuilderDatabase _Database;
    private IDictionary<Gesture, DiscreteGestureResult> discreteGestureDetectionResults;
    private IDictionary<Gesture, ContinuousGestureResult> continuousGestureDetectionResults;

    public IDictionary<Gesture, DiscreteGestureResult> GetDetectedGestures()
    {
        return discreteGestureDetectionResults;
    }

    public IDictionary<Gesture, ContinuousGestureResult> GetDetectedContinuousGestures()
    {
        return continuousGestureDetectionResults;
    }
    public VisualGestureBuilderFrameSource GetRecordedGesturesData()
    {
        return _VGB_Source;
    }
 
    public Body[] GetData()
    {
        return _Data;
    }

    public void SetVGBReaderPauseState(bool state)
    {
        this._VGB_Reader.IsPaused = state;
    }
    
    void Start () 
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.BodyFrameSource.OpenReader();
            
            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
                // Set up Gesture Source
                _VGB_Source = VisualGestureBuilderFrameSource.Create(_Sensor, 0);
                // open the reader for the vgb frames
                _VGB_Reader = _VGB_Source.OpenReader();
                if (_VGB_Reader != null)
                {
                    Debug.Log("Opening VGB_reader");
                    _VGB_Reader.IsPaused = true;
                    _VGB_Reader.FrameArrived += GestureFrameArrived;
                }

                // load the 'Seated' gesture from the gesture database
                string databasePath = System.IO.Path.Combine(Application.streamingAssetsPath, gestureDatabaseFileName);
                _Database = VisualGestureBuilderDatabase.Create(databasePath);
               
                // Load all gestures
                IList<Gesture> gesturesList = _Database.AvailableGestures;
                for (int g = 0; g < gesturesList.Count; g++)
                {
                    Gesture gesture = gesturesList[g];
                    _VGB_Source.AddGesture(gesture);
                }

            }
        }   
    }
    
    void Update () 
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                if (_Data == null)
                {
                    _Data = new Body[_Sensor.BodyFrameSource.BodyCount];
                }
                
                frame.GetAndRefreshBodyData(_Data);
                
                frame.Dispose();
                frame = null;
            }
        }    
    }
    
    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }
        
        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }
            
            _Sensor = null;
        }
    }


    /// Handles gesture detection results arriving from the sensor for the associated body tracking Id
    private void GestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
    {
        VisualGestureBuilderFrameReference frameReference = e.FrameReference;
        using (VisualGestureBuilderFrame frame = frameReference.AcquireFrame())
        {
            if (frame != null)
            {
                // get the discrete gesture results which arrived with the latest frame
                IDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;
                discreteGestureDetectionResults = discreteResults;

                // get the discrete gesture results which arrived with the latest frame
                IDictionary<Gesture, ContinuousGestureResult> continuousResults = frame.ContinuousGestureResults;
                continuousGestureDetectionResults = continuousResults;

            }
        }
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
