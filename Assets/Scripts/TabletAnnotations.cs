using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TabletAnnotations : MonoBehaviour
{
    public Material lineMaterial;
    public float lineLifetime = 2f; // Time before lines disappear
    public float lineWidth = 0.02f;
    public Color currentColor = Color.red;
    public Image colorWheel;
    public Slider redSlider, greenSlider, blueSlider;

    private LineRenderer currentLine;
    private List<Vector3> points = new List<Vector3>();

    void Start()
    {
        redSlider.onValueChanged.AddListener(UpdateColor);
        greenSlider.onValueChanged.AddListener(UpdateColor);
        blueSlider.onValueChanged.AddListener(UpdateColor);
        UpdateColor(0); // Initialize color
    }

    void Update()
{
    if (IsPointerOverUI()) return; // Prevent drawing if interacting with UI

    bool isDrawing = Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase != TouchPhase.Ended);

    // Start a new line when the user clicks or taps
    if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
    {
        StartNewLine();
    }

    // Draw the line as the user drags or holds the mouse/touch
    if (isDrawing && currentLine != null)
    {
        Vector3 touchPos = GetTouchPosition();

        // Add points only if there's significant movement
        if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], touchPos) > 0.002f) // Lower threshold for trackpad
        {
            points.Add(touchPos);
            currentLine.positionCount = points.Count;
            currentLine.SetPosition(points.Count - 1, touchPos);
        }

        // Update the line color as the sliders change
        Color selectedColor = new Color(redSlider.value, greenSlider.value, blueSlider.value);
        currentLine.startColor = selectedColor;
        currentLine.endColor = selectedColor;
    }

    // Clean up when the user releases the mouse or touch
    if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
    {
        if (currentLine != null)
        {
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
    currentLine.numCornerVertices = 10; // Smooth curves
    currentLine.numCapVertices = 10; // Rounded ends

    // Set the initial line color based on the sliders
    Color selectedColor = new Color(redSlider.value, greenSlider.value, blueSlider.value);
    currentLine.startColor = selectedColor;
    currentLine.endColor = selectedColor;

    points.Clear();
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

        // Check if any touch is over UI
        if (Input.touchCount > 0)
        {
            int fingerId = Input.GetTouch(0).fingerId;
            return EventSystem.current.IsPointerOverGameObject(fingerId);
        }

        return false;
    }


}

