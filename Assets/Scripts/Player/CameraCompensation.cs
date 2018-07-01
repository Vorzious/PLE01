using System;
using System.IO;
using UnityEngine;
using UnityEngine.VR;
using System.Collections;

public class CameraCompensation : MonoBehaviour {

    public int environment;
    
    public Vector3 gainContainer;
    
    public Vector3 positionLocalHeadset;
    public Vector3 positionGlobalHeadset;
    public Quaternion rotationLocalHeadsetQuaternion;
    public Vector3 rotationLocalHeadsetEuler;
    public Vector3 rotationGlobalHeadsetEuler;

    public Vector3 positionLocalContainer;
    public Vector3 positionGlobalContainer;
    public Vector3 rotationLocalContainer;
    public Vector3 rotationGlobalContainer;

    public Vector3 rotationCurrentYMinStartY;
    public float rotationYwithGainY;
    public Vector3 rotationResultEuler;

    public Vector3 startPositionLocalHeadset;
    public Vector3 startRotationLocalHeadsetEuler;
  
    public GameObject headset;

    public bool addTD = false;

    public int frameCounter = 0;
    public double timeStamp = 0;
    public double timeStampDifference = 0;
    private DateTime T0;
    private DateTime T_old;
    public string filePath = @"d:\Output\";
 //   public string filenamePrefix = @"";

    private StreamWriter _writer = null;
    private int _count = 1;
    private bool saving = false;

    public bool recenterAtStart = false;
     
    // private bool testObject = false;

    // Use this for initialization
    /// <summary>
    /// 
    /// </summary>
    void Start ()
    {
        string[] args = Environment.GetCommandLineArgs();
        StreamWriter _writerArgs = new StreamWriter(filePath + "Args.txt");
        string old_string = "";
        foreach (string arg in args)
        {
            _writerArgs.WriteLine(arg);
            if (old_string.Contains("-Gain"))
            {
                gainContainer.y = Single.Parse(arg);
                _writerArgs.WriteLine(gainContainer.y.ToString());
            }
            old_string = arg;
        }
        _writerArgs.Close();
        if (recenterAtStart)
        {
            InputTracking.Recenter();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (frameCounter == 1)
        {
           //   InputTracking.Recenter();
        } 
        frameCounter++;

        if (Input.GetKeyDown(KeyCode.R))
        {
             InputTracking.Recenter();
        }

        if ((Input.GetKeyDown(KeyCode.S)) && (!(saving)))
        {
            CreateOutputFile();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_writer != null)  _writer.Close();
            saving = false; 
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_writer != null) _writer.Close();
            saving = false;
            //Application.Quit();
        }
        //   if (Input.anyKeyDown)

        updateContainer();
    }

    void LateUpdate()
    {
      //  updateContainer();
    }

    private void updateContainer()
    {
        positionLocalHeadset = new Vector3(headset.transform.localPosition.x, headset.transform.localPosition.y, headset.transform.localPosition.z);
        positionGlobalHeadset = new Vector3(headset.transform.position.x, headset.transform.position.y, headset.transform.position.z);

        rotationLocalHeadsetQuaternion = headset.transform.localRotation;
        rotationLocalHeadsetEuler = rotationLocalHeadsetQuaternion.eulerAngles;  scaleAroundOriginalRotation(ref rotationLocalHeadsetEuler);
        rotationGlobalHeadsetEuler = headset.transform.rotation.eulerAngles;   scaleAroundOriginalRotation(ref rotationGlobalHeadsetEuler);

        // alleen yaw, rotatie rond de y-as
        rotationCurrentYMinStartY = new Vector3(0, rotationLocalHeadsetEuler.y - startRotationLocalHeadsetEuler.y, 0);
        scaleTo180(ref rotationCurrentYMinStartY);
        rotationYwithGainY = rotationCurrentYMinStartY.y * gainContainer.y;
        rotationResultEuler = new Vector3(0, rotationYwithGainY, 0);
        Quaternion r_result2 = new Quaternion(0, 0, 0, 0);
        r_result2 = Quaternion.Euler(rotationResultEuler.x, rotationResultEuler.y, rotationResultEuler.z);
        this.transform.localRotation = r_result2;

        //this.transform.localPosition = positionLocalHeadset;

        positionLocalContainer = this.transform.localPosition;
        positionGlobalContainer = this.transform.position;
        rotationLocalContainer = this.transform.localRotation.eulerAngles; scaleTo180(ref rotationLocalContainer);
        rotationGlobalContainer = this.transform.rotation.eulerAngles; scaleTo180(ref rotationGlobalContainer);

        if (saving) WriteFrame();
    }

    public void scaleAroundOriginalRotation(ref Vector3 v)
    {
        if (v.x >= 180) { v.x = v.x - (float)360.0; }
        if ((v.y - startRotationLocalHeadsetEuler.y) >= 180) { v.y = v.y - (float)360.0; }
        if (v.z >= 180) { v.z = v.z - (float)360.0; }
        if (v.x < -180) { v.x = v.x + (float)360.0; }
        if ((v.y - startRotationLocalHeadsetEuler.y) < -180) { v.y = v.y + (float)360.0; }
        if (v.z < -180) { v.z = v.z + (float)360.0; }
    }

    public void scaleTo180(ref Vector3 v)
    {
        if (v.x >= 180) { v.x = v.x - (float)360.0; }
        if (v.y >= 180) { v.y = v.y - (float)360.0; }
        if (v.z >= 180) { v.z = v.z - (float)360.0; }
        if (v.x < -180) { v.x = v.x + (float)360.0; }
        if (v.y < -180) { v.y = v.y - (float)360.0; }
        if (v.z < -180) { v.z = v.z + (float)360.0; }
    }

    public void CreateOutputFile()
    {
        try
        {
            if (_writer != null)
                _writer.Close();

            // Get available filename
            while (File.Exists(@filePath + _count.ToString() + ".txt"))
            {
                _count++;
            }
            // Create an instance of StreamWriter to write text to a file.
            _writer = new StreamWriter(filePath + _count.ToString() + ".txt");
            string str = "frameCounter timeStamp timeStampDifference environment " +
                         "gainContainer.x gainContainer.y gainContainer.z " +
                         "rotationCurrentYHeadMinStartYHead.x rotationCurrentYHeadMinStartYHead.y rotationCurrentYHeadMinStartYHead.z " +
                         "rotationResultEuler.x rotationResultEuler.y rotationResultEuler.z " +
                         "positionLocalHeadset.x positionLocalHeadset.y positionLocalHeadset.z " +
                         "positionGlobalHeadset.x positionGlobalHeadset.y positionGlobalHeadset.z " +
                         "rotationLocalHeadsetEuler.x rotationLocalHeadsetEuler.y rotationLocalHeadsetEuler.z " +
                         "rotationGlobalHeadsetEuler.x rotationGlobalHeadsetEuler.y rotationGlobalHeadsetEuler.z " +
                         "positionLocalContainer.x positionLocalContainer.y positionLocalContainer.z " +
                         "positionGlobalContainer.x positionGlobalContainer.y positionGlobalContainer.z " +
                         "rotationLocalContainer.x rotationLocalContainer.y rotationLocalContainer.z " +
                         "rotationGlobalContainer.x rotationGlobalContainer.y rotationGlobalContainer.z " +
                         "startPositionLocalHeadset.x startPositionLocalHeadset.y startPositionLocalHeadset.z " +
                         "startRotationLocalHeadsetEuler.x startRotationLocalHeadsetEuler.y startRotationLocalHeadsetEuler.z";
           if (addTD) _writer.WriteLine(str);
            StreamWriter _writerTD = new StreamWriter(filePath + _count.ToString() + ".td");
            _writerTD.WriteLine(str);
            _writerTD.Close();
            T0 = DateTime.Now;
            T_old = T0;
            saving = true;
        }
        catch (Exception err)
        {
            Debug.LogException(err);
        }
    }

    public void WriteFrame()
    {
        DateTime now = DateTime.Now;
        TimeSpan sp1 = now.Subtract(T0);
        timeStamp = sp1.TotalMilliseconds;
        TimeSpan sp2 = now.Subtract(T_old);
        timeStampDifference = sp2.TotalMilliseconds;
        T_old = now;
        if ((saving) && (_writer != null))
        {
            try
            {
                _writer.WriteLine("framecounter = " + frameCounter.ToString() + " " + timeStamp.ToString() + " " + timeStampDifference.ToString() + " " + environment.ToString() + " " +
                                  gainContainer.x.ToString() + " " + gainContainer.y.ToString() + " " + gainContainer.z.ToString() + " " +
                                  rotationCurrentYMinStartY.x.ToString() + " " + rotationCurrentYMinStartY.y.ToString() + " " + rotationCurrentYMinStartY.z.ToString() + " " +
                                  rotationResultEuler.x.ToString() + " " + rotationResultEuler.y.ToString() + " " + rotationResultEuler.z.ToString() + " " +
                                  positionLocalHeadset.x.ToString() + " " + positionLocalHeadset.y.ToString() + " " + positionLocalHeadset.z.ToString() + " " +
                                  positionGlobalHeadset.x.ToString() + " " + positionGlobalHeadset.y.ToString() + " " + positionGlobalHeadset.z.ToString() + " " +
                                  rotationLocalHeadsetEuler.x.ToString() + " " + rotationLocalHeadsetEuler.y.ToString() + " " + rotationLocalHeadsetEuler.z.ToString() + " " +
                                  rotationGlobalHeadsetEuler.x.ToString() + " " + rotationGlobalHeadsetEuler.y.ToString() + " " + rotationGlobalHeadsetEuler.z.ToString() + " " +
                                  positionLocalContainer.x.ToString() + " " + positionLocalContainer.y.ToString() + " " + positionLocalContainer.z.ToString() + " " +
                                  positionGlobalContainer.x.ToString() + " " + positionGlobalContainer.y.ToString() + " " + positionGlobalContainer.z.ToString() + " " +
                                  rotationLocalContainer.x.ToString() + " " + rotationLocalContainer.y.ToString() + " " + rotationLocalContainer.z.ToString() + " " +
                                  rotationGlobalContainer.x.ToString() + " " + rotationGlobalContainer.y.ToString() + " " + rotationGlobalContainer.z.ToString() + " " +
                                  startPositionLocalHeadset.x.ToString() + " " + startPositionLocalHeadset.y.ToString() + " " + startPositionLocalHeadset.z.ToString() + " " +
                                  startRotationLocalHeadsetEuler.x.ToString() + " " + startRotationLocalHeadsetEuler.y.ToString() + " " + startRotationLocalHeadsetEuler.z.ToString());
            }
            catch (Exception err)
            {
                Debug.LogException(err);
            }
        }
    }

    void OnDestroy()
    {
        if (_writer != null)
            _writer.Close();
        saving = false;
    }

    void OnApplicationQuit()
    {
        if (_writer != null)
            _writer.Close();
        saving = false;
    }
}

