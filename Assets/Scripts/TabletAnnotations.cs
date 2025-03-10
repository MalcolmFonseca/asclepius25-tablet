using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System;
using Photon.Pun;
using System.Net.NetworkInformation;
using System.Collections;

public class TabletAnnotations : MonoBehaviour
{
    public Material lineMaterial;
    public float lineWidth = 0.02f;
    public Color currentColor = Color.red;
    public Image colorWheel;
    public Slider redSlider, greenSlider, blueSlider;

    private GameObject lineObj;
    private LineRenderer currentLine;
    PhotonView pV;
    private List<Vector3> points = new List<Vector3>();
    private List<string> annotationData = new List<string>();

    private int annotationID = 0;
    private string fileName;
    private string filePath;
    private string downloadsPath;

    void Start()
    {
        redSlider.onValueChanged.AddListener(UpdateColor);
        greenSlider.onValueChanged.AddListener(UpdateColor);
        blueSlider.onValueChanged.AddListener(UpdateColor);
        UpdateColor(0);

        // Generate filename with timestamp
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
        fileName = $"annotation_{timestamp}.csv";

        // Define file paths
        filePath = Path.Combine(Application.dataPath, fileName); // Unity project folder
        downloadsPath = GetDownloadsPath(fileName); // OS-specific Downloads folder

        // Create the file and add headers
        string header = "Timestamp,AnnotationID,ColorR,ColorG,ColorB,X,Y,Z\n";
        File.WriteAllText(filePath, header); // Save in project directory
        File.WriteAllText(downloadsPath, header); // Save in Downloads
    }

    void Update()
    {
        if (IsPointerOverUI()) return;

        bool isDrawing = Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase != TouchPhase.Ended);

        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            StartNewLine();
        }

        if (isDrawing && currentLine != null)
        {
            Vector3 touchPos = GetTouchPosition();

            if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], touchPos) > 0.002f)
            {
                points.Add(touchPos);
                pV.RPC("UpdateLine", RpcTarget.All, points.Count, touchPos);

                LogAnnotationData(touchPos);
            }
        }

        if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
        {
            if (currentLine != null)
            {
                SaveAnnotationData();
                //Destroy(currentLine.gameObject, lineLifetime);
                //StartCoroutine(DeleteAnnotation());
                currentLine = null;
            }
        }
    }

    void StartNewLine()
    {
        lineObj = PhotonNetwork.Instantiate("Line", new Vector3(0, 0, 0), Quaternion.identity, 0);
        currentLine = lineObj.GetComponent<LineRenderer>();
        pV = lineObj.GetComponent<PhotonView>();
        pV.RPC("StartLine", RpcTarget.All, lineWidth, redSlider.value, greenSlider.value, blueSlider.value);

        points.Clear();
        annotationData.Clear();
        annotationID++;
    }

    void LogAnnotationData(Vector3 position)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        annotationData.Add($"{timestamp},{annotationID},{redSlider.value:F3},{greenSlider.value:F3},{blueSlider.value:F3},{position.x:F3},{position.y:F3},{position.z:F3}");
    }

    void SaveAnnotationData()
    {
        File.AppendAllLines(filePath, annotationData); // Save in project folder
        File.AppendAllLines(downloadsPath, annotationData); // Save in Downloads folder
        Debug.Log($"Annotations saved to: {downloadsPath}");
    }

    Vector3 GetTouchPosition()
    {
        if (Input.touchCount > 0)
        {
            Vector2 touchPos = Input.GetTouch(0).position;
            return Camera.main.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, 10));
        }
        else
        {
            Vector3 mousePos = Input.mousePosition;
            return Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10));
        }
    }

    void UpdateColor(float value)
    {
        currentColor = new Color(redSlider.value, greenSlider.value, blueSlider.value);
        colorWheel.color = currentColor;
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        if (Input.touchCount > 0)
        {
            int fingerId = Input.GetTouch(0).fingerId;
            return EventSystem.current.IsPointerOverGameObject(fingerId);
        }
        return false;
    }

    string GetDownloadsPath(string fileName)
    {
        string path;
#if UNITY_ANDROID
        path = Path.Combine("/storage/emulated/0/Download", fileName); // Android Downloads folder
#elif UNITY_STANDALONE_WIN
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", fileName); // Windows
#elif UNITY_STANDALONE_OSX
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", fileName); // macOS
#elif UNITY_STANDALONE_LINUX
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", fileName); // Linux
#else
        path = Application.persistentDataPath; // Default fallback
#endif
        return path;
    }
}
