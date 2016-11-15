using UnityEngine;
using System.Collections;
using Windows.Kinect;

using OpenCVForUnity;

public class KinectCapture : MonoBehaviour
{

    KinectSensor sensor;
    MultiSourceFrameReader reader;
    CoordinateMapper coordinateMapper;
    DepthSpacePoint[] depthSpacePoints;
    Texture2D texture;
    byte[] colorData;
    ushort[] depthData;
    byte[] bodyIndexData;
    byte[] maskData;
    public enum modeType
    {
        original,
        sepia,
        pixelize,
        comic
    }

    public modeType mode;
    Mat rgbaMat;
    Mat maskMat;
    Mat outputMat;


    //sepia
    Mat sepiaKernel;

    //pixelize
    Size pixelizeSize0;
    Mat pixelizeIntermediateMat;

    //comic
    Mat comicGrayMat;
    Mat comicLineMat;
    Mat comicMaskMat;
    Mat comicBgMat;
    Mat comicDstMat;
    byte[] comicGrayPixels;
    byte[] comicMaskPixels;



    void Start()
    {
        sensor = KinectSensor.GetDefault();

        if (sensor != null)
        {
            coordinateMapper = sensor.CoordinateMapper;

            reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex);

            FrameDescription colorFrameDesc = sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            texture = new Texture2D(colorFrameDesc.Width, colorFrameDesc.Height, TextureFormat.RGBA32, false);
            colorData = new byte[colorFrameDesc.BytesPerPixel * colorFrameDesc.LengthInPixels];


            FrameDescription depthFrameDesc = sensor.DepthFrameSource.FrameDescription;
            depthData = new ushort[depthFrameDesc.LengthInPixels];
            depthSpacePoints = new DepthSpacePoint[colorFrameDesc.LengthInPixels];

            FrameDescription bodyIndexFrameDesc = sensor.BodyIndexFrameSource.FrameDescription;
            bodyIndexData = new byte[bodyIndexFrameDesc.BytesPerPixel * bodyIndexFrameDesc.LengthInPixels];


            if (!sensor.IsOpen)
            {
                sensor.Open();
            }

            rgbaMat = new Mat(colorFrameDesc.Height, colorFrameDesc.Width, CvType.CV_8UC4);
            Debug.Log("rgbaMat " + rgbaMat.ToString());

            maskMat = new Mat(rgbaMat.rows(), rgbaMat.cols(), CvType.CV_8UC1);
            outputMat = new Mat(rgbaMat.rows(), rgbaMat.cols(), CvType.CV_8UC4);

            maskData = new byte[rgbaMat.rows() * rgbaMat.cols()];

            gameObject.transform.localScale = new Vector3(texture.width, texture.height, 1);
            gameObject.GetComponent<Renderer>().material.mainTexture = texture;
            Camera.main.orthographicSize = texture.height / 2;



            // sepia
            sepiaKernel = new Mat(4, 4, CvType.CV_32F);
            sepiaKernel.put(0, 0, /* R */0.189f, 0.769f, 0.393f, 0f);
            sepiaKernel.put(1, 0, /* G */0.168f, 0.686f, 0.349f, 0f);
            sepiaKernel.put(2, 0, /* B */0.131f, 0.534f, 0.272f, 0f);
            sepiaKernel.put(3, 0, /* A */0.000f, 0.000f, 0.000f, 1f);


            // pixelize
            pixelizeIntermediateMat = new Mat();
            pixelizeSize0 = new Size();


            //comic
            comicGrayMat = new Mat(texture.height, texture.width, CvType.CV_8UC1);
            comicLineMat = new Mat(texture.height, texture.width, CvType.CV_8UC1);
            comicMaskMat = new Mat(texture.height, texture.width, CvType.CV_8UC1);

            //create a striped background.
            comicBgMat = new Mat(texture.height, texture.width, CvType.CV_8UC1, new Scalar(255));
            for (int i = 0; i < comicBgMat.rows() * 2.5f; i = i + 4)
            {
                Imgproc.line(comicBgMat, new Point(0, 0 + i), new Point(comicBgMat.cols(), -comicBgMat.cols() + i), new Scalar(0), 1);
            }

            comicDstMat = new Mat(texture.height, texture.width, CvType.CV_8UC1);

            comicGrayPixels = new byte[comicGrayMat.cols() * comicGrayMat.rows() * comicGrayMat.channels()];
            comicMaskPixels = new byte[comicMaskMat.cols() * comicMaskMat.rows() * comicMaskMat.channels()];
        }
        else
        {
            UnityEngine.Debug.LogError("No ready Kinect found!");
        }

    }

    void Update()
    {
        if (reader != null)
        {
            MultiSourceFrame frame = reader.AcquireLatestFrame();
            if (frame != null)
            {

                using (ColorFrame colorFrame = frame.ColorFrameReference.AcquireFrame())
                {
                    if (colorFrame != null)
                    {
                        colorFrame.CopyConvertedFrameDataToArray(colorData, ColorImageFormat.Rgba);
                    }

                }
                using (DepthFrame depthFrame = frame.DepthFrameReference.AcquireFrame())
                {
                    if (depthFrame != null)
                    {
                        //Debug.Log ("bodyIndexFrame not null");
                        depthFrame.CopyFrameDataToArray(depthData);
                    }
                }
                using (BodyIndexFrame bodyIndexFrame = frame.BodyIndexFrameReference.AcquireFrame())
                {
                    if (bodyIndexFrame != null)
                    {
                        //Debug.Log ("bodyIndexFrame not null");
                        bodyIndexFrame.CopyFrameDataToArray(bodyIndexData);
                    }
                }

                frame = null;
            }
        }
        else
        {
            return;
        }

        Utils.copyToMat(colorData, outputMat);


        coordinateMapper.MapColorFrameToDepthSpace(depthData, depthSpacePoints);
        int width = rgbaMat.width();
        int height = rgbaMat.height();
        int depthWidth = 512;
        byte[] maskOn = new byte[] { 0 };
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = x + y * width;
                colorData[index] = 0;

                int depthX = (int)(depthSpacePoints[index].X + 0.5f);
                int depthY = (int)(depthSpacePoints[index].Y + 0.5f);
                if ((0 <= depthX) && (depthX < depthWidth) && (0 <= depthY) && (depthY < 424))
                {
                    int depthIndex = depthY * depthWidth + depthX;
                    int depthValue = depthData[depthIndex];
                    if(depthValue < 1500 && depthValue > 1450)
                    maskData[index] = 0;

                }
                else
                {
                    maskData[index] = 255;
                }

            }
        }
        Utils.copyToMat(maskData, maskMat);

        rgbaMat.copyTo(outputMat, maskMat);
        Utils.matToTexture(outputMat, texture);
    }

    void OnApplicationQuit()
    {
        if (reader != null)
        {
            reader.Dispose();
            reader = null;
        }

        if (sensor != null)
        {
            if (sensor.IsOpen)
            {
                sensor.Close();
            }

            sensor = null;
        }
    }

}

