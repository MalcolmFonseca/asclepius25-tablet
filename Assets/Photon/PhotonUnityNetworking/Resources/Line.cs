using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UIElements;

public class Line : MonoBehaviourPunCallbacks
{
    [PunRPC] void StartLine(float lineWidth, float red, float green, float blue) {
        LineRenderer currentLine = GetComponent<LineRenderer>();
        currentLine.material = new Material(Shader.Find("Sprites/Default"));
        currentLine.startWidth = lineWidth;
        currentLine.endWidth = lineWidth;
        currentLine.positionCount = 0;
        currentLine.useWorldSpace = true;
        currentLine.numCornerVertices = 10;
        currentLine.numCapVertices = 10;
        Color selectedColor = new Color(red,green,blue);
        currentLine.startColor = selectedColor;
        currentLine.endColor = selectedColor;
    }

    [PunRPC] void UpdateLine(int pointCount, Vector3 touchPos)
    {
        LineRenderer currentLine = GetComponent<LineRenderer>();
        currentLine.positionCount = pointCount;
        currentLine.SetPosition(pointCount - 1, touchPos);
    }
}
