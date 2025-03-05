using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO; // For File Handling
using System; // For DateTime

public class TabletAnnotations : MonoBehaviour
{
    public Material lineMaterial;
    public float lineLifetime = 2f;
    public float lineWidth = 0.02f;
    public Color currentColor = Color.red;
    public Image colorWheel;
    public Slider redSlider, greenSlider, blueSlider;

    private LineRenderer currentLine;
    private List<Vector3> points = new List<Vector3>();
    private List<string> annotationData = new List<string>();

    private int annotationID = 0;
    private string filePath;

    void Start()
    {
        redSlider.onValueChanged.AddListener(UpdateColor);
        greenSlider.onValueChanged.AddListener(UpdateColor);
        blueSlider.onValueChanged.AddListener(UpdateColor);
        UpdateColor(0);

        // Generate a unique filename based on the current time
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
        string fileName = $"annotation_{timestamp}.csv";

        // Save it in a visible location
        #if UNITY_ANDROID
            filePath = Path.Combine(Application.persistentDataPath, fileName); // Best location for Android
        #else
            filePath = Path.Combine(Application.dataPath, fileName); // Visible in Unity editor
        #endif

        // Create the file and add headers
        File.WriteAllText(filePath, "Timestamp,AnnotationID,ColorR,ColorG,ColorB,X,Y,Z\n");
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
                currentLine.positionCount = points.Count;
                currentLine.SetPosition(points.Count - 1, touchPos);

                LogAnnotationData(touchPos);
            }
        }

        if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
        {
            if (currentLine != null)
            {
                SaveAnnotationData();
                Destroy(currentLine.gameObject, lineLifetime);
                currentLine = null;
            }
        }
    }

    void StartNewLine()
    {
        GameObject lineObj = new GameObject("Line");
        currentLine = lineObj.AddComponent<LineRenderer>();
        currentLine.material = new Material(Shader.Find("Sprites/Default"));
        currentLine.startWidth = lineWidth;
        currentLine.endWidth = lineWidth;
        currentLine.positionCount = 0;
        currentLine.useWorldSpace = true;
        currentLine.numCornerVertices = 10;
        currentLine.numCapVertices = 10;

        Color selectedColor = new Color(redSlider.value, greenSlider.value, blueSlider.value);
        currentLine.startColor = selectedColor;
        currentLine.endColor = selectedColor;

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
        File.AppendAllLines(filePath, annotationData);
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
}