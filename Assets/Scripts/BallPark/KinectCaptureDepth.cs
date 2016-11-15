using UnityEngine;
using System.Collections;
using Windows.Kinect;

using OpenCVForUnity;
using System.Collections.Generic;

public class KinectCaptureDepth : MonoBehaviour
{

    KinectSensor sensor;
    MultiSourceFrameReader reader;
    CoordinateMapper coordinateMapper;
    DepthSpacePoint[] depthSpacePoints;
    Texture2D texture;
    Texture2D downscaleTexture;
    byte[] colorData;
    ushort[] depthData;
    byte[] maskData;

    int maxDistance = 2600;
    int minDistance = 2400;

    Mat rgbaMat;
    Mat maskMat;
    Mat outputMat;
    Mat downscaleMat;

    //pixelize
    Size pixelizeSize0;
    Mat pixelizeIntermediateMat;

    byte[] comicGrayPixels;
    byte[] comicMaskPixels;
    OpenCVForUnity.Rect roiHand;
    private Point effectPoint;

    void Start()
    {

        roiHand = new OpenCVForUnity.Rect();
        roiHand.x = roiHand.y = -1;
        effectPoint = new Point(0,0);

        sensor = KinectSensor.GetDefault();

        if (sensor != null)
        {
            coordinateMapper = sensor.CoordinateMapper;

            reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth);

            FrameDescription colorFrameDesc = sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            texture = new Texture2D(colorFrameDesc.Width, colorFrameDesc.Height, TextureFormat.RGBA32, false);
            downscaleTexture = new Texture2D(colorFrameDesc.Width/2, colorFrameDesc.Height/2, TextureFormat.RGBA32, false);
            colorData = new byte[colorFrameDesc.BytesPerPixel * colorFrameDesc.LengthInPixels];


            FrameDescription depthFrameDesc = sensor.DepthFrameSource.FrameDescription;
            depthData = new ushort[depthFrameDesc.LengthInPixels];
            depthSpacePoints = new DepthSpacePoint[colorFrameDesc.LengthInPixels];

            if (!sensor.IsOpen)
            {
                sensor.Open();
            }

            rgbaMat = new Mat(colorFrameDesc.Height, colorFrameDesc.Width, CvType.CV_8UC4);
            Debug.Log("rgbaMat " + rgbaMat.ToString());

            maskMat = new Mat(rgbaMat.rows(), rgbaMat.cols(), CvType.CV_8UC1);
            outputMat = new Mat(rgbaMat.rows(), rgbaMat.cols(), CvType.CV_8UC4);
            downscaleMat = new Mat(rgbaMat.rows() / 2, rgbaMat.cols() / 2, CvType.CV_8UC4);
            maskData = new byte[rgbaMat.rows() * rgbaMat.cols()];
            /*
            gameObject.transform.localScale = new Vector3(texture.width, texture.height, 1);
            gameObject.GetComponent<Renderer>().material.mainTexture = texture;
            Camera.main.orthographicSize = texture.height / 2;
            */
            gameObject.transform.localScale = new Vector3(downscaleTexture.width, downscaleTexture.height, 1);
            gameObject.GetComponent<Renderer>().material.mainTexture = downscaleTexture;
            Camera.main.orthographicSize = downscaleTexture.height / 2;

            // pixelize
            pixelizeIntermediateMat = new Mat();
            pixelizeSize0 = new Size();
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
                maskData[index] = 255;
                int depthX = (int)(depthSpacePoints[index].X + 0.5f);
                int depthY = (int)(depthSpacePoints[index].Y + 0.5f);
                if ((0 <= depthX) && (depthX < depthWidth) && (0 <= depthY) && (depthY < 424))
                {
                    int depthIndex = depthY * depthWidth + depthX;
                    int depthValue = depthData[depthIndex];
                    if (depthValue < maxDistance && depthValue > minDistance) //Within desired range
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
        Imgproc.resize(outputMat, downscaleMat, downscaleMat.size());

        Core.flip(downscaleMat, downscaleMat, 1);

        roiHand = filterRedColor(downscaleMat);
        effectPoint = new Point(roiHand.x, roiHand.y);
        Imgproc.circle(downscaleMat, effectPoint, 10, new OpenCVForUnity.Scalar(255, 0, 0));
        roiHand.x = -1;
        
        Utils.matToTexture(downscaleMat, downscaleTexture);
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
    private OpenCVForUnity.Rect filterRedColor(Mat input)
    {
        int largest_area = 0;
        int largest_contour_index = 0;

        int B_MIN = 100;
        int B_MAX = 255;
        int G_MIN = 100;
        int G_MAX = 255;
        int R_MIN = 100;
        int R_MAX = 255;
        Mat mask = new Mat(), mask1, mask2, mask_grey;
        Mat hsv_image = new Mat();
        Imgproc.cvtColor(input, hsv_image, Imgproc.COLOR_BGR2HSV);
        mask1 = input.clone(); mask2 = input.clone();
        //filter the image in BGR color space
        Core.inRange(hsv_image, new Scalar(B_MIN, G_MIN, R_MIN), new Scalar(B_MAX, G_MAX, R_MAX), mask);
        //cv::inRange(hsv_image, cv::Scalar(55, 100, 50), cv::Scalar(65, 255, 255), mask1);
        //cv::inRange(hsv_image, cv::Scalar(50, 100, 50), cv::Scalar(70, 255, 255), mask2);
        List<MatOfPoint> contours = new List<MatOfPoint>(); // Vector for storing contour
        Mat hierarchy = new Mat();
        //mask = mask1 | mask2;
        Imgproc.findContours(mask, contours, hierarchy, Imgproc.RETR_CCOMP, Imgproc.CHAIN_APPROX_SIMPLE); // Find the contours in the image
        OpenCVForUnity.Rect bounding_rect = new OpenCVForUnity.Rect();

        for (int i = 0; i < contours.Count; i++) // iterate through each contour. 
        {
            double a = Imgproc.contourArea(contours[i], false);  //  Find the area of contour

            /* 
            if (a>largest_area) {
                largest_area = (int)a;
                largest_contour_index = i;                //Store the index of largest contour
                bounding_rect = Imgproc.boundingRect(contours[i]); // Find the bounding rectangle for biggest contour
            }
            */
            Debug.Log(a);
            if(a > 50)
            {
                bounding_rect = Imgproc.boundingRect(contours[i]);
            }
        }
        //cv::Scalar color(255, 255, 255);
        //drawContours(mask, contours, largest_contour_index, color, CV_FILLED, 8, hierarchy); // Draw the largest contour using previously stored index.
        //rectangle(mask, bounding_rect, cv::Scalar(0, 255, 0), 1, 8, 0);
        //cv::imshow("skin", mask);
        return bounding_rect;
    }

    public Point GetEffectPoint()
    {
        return effectPoint;
    }

}

