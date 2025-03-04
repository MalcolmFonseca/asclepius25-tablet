using System.Collections.Generic;
using UnityEngine;

public class TabletAnnotations : MonoBehaviour
{
    public Material lineMaterial;
    public float lineLifetime = 2f; // Time before lines disappear
    public float lineWidth = 0.02f;

    private LineRenderer currentLine;
    private List<Vector3> points = new List<Vector3>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            StartNewLine();
        }

        if ((Input.GetMouseButton(0) || Input.touchCount > 0) && currentLine != null)
        {
            Vector3 touchPos = GetTouchPosition();
            if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], touchPos) > 0.01f)
            {
                points.Add(touchPos);
                currentLine.positionCount = points.Count;
                currentLine.SetPosition(points.Count - 1, touchPos);
            }
        }

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
        currentLine.material = lineMaterial;
        currentLine.startWidth = lineWidth;
        currentLine.endWidth = lineWidth;
        currentLine.positionCount = 0;
        currentLine.useWorldSpace = true;
        points.Clear();
    }

    Vector3 GetTouchPosition()
    {
        if (Input.touchCount > 0) // Get touch position for mobile
        {
            Vector2 touchPos = Input.GetTouch(0).position;
            return Camera.main.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, 10));
        }
        else // Get mouse position for PC
        {
            Vector3 mousePos = Input.mousePosition;
            return Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10));
        }
    }
}

