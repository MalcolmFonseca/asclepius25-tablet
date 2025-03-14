using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using UnityEngine.UI;

public class OpenIGTLinkVideoReceiver : MonoBehaviour
{
    public int port = 18944;
    private TcpListener server;
    private TcpClient client;
    private NetworkStream stream;
    private Texture2D receivedTexture;
    public RawImage display;

    void Start()
    {
        server = new TcpListener(IPAddress.Any, port);
        server.Start();
        StartCoroutine(WaitForClientConnection());
    }

    IEnumerator WaitForClientConnection()
    {
        Debug.Log("Waiting for connection...");
        client = server.AcceptTcpClient();
        stream = client.GetStream();
        Debug.Log("Connected to HoloLens!");

        while (true)
        {
            try
            {
                byte[] header = new byte[18]; // "OPENIGTLINK_IMAGE"
                stream.Read(header, 0, header.Length);
                string headerText = System.Text.Encoding.ASCII.GetString(header);

                if (headerText == "OPENIGTLINK_IMAGE")
                {
                    byte[] sizeBytes = new byte[4];
                    stream.Read(sizeBytes, 0, sizeBytes.Length);
                    int frameSize = BitConverter.ToInt32(sizeBytes, 0);

                    byte[] frameData = new byte[frameSize];
                    int bytesRead = 0;
                    while (bytesRead < frameSize)
                    {
                        bytesRead += stream.Read(frameData, bytesRead, frameSize - bytesRead);
                    }

                    ProcessFrame(frameData);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error receiving video frame: " + e.Message);
                break;
            }

            yield return null;
        }
    }

    void ProcessFrame(byte[] data)
    {
        if (receivedTexture == null)
        {
            receivedTexture = new Texture2D(640, 480, TextureFormat.RGB24, false);
        }

        receivedTexture.LoadImage(data);
        receivedTexture.Apply();
        display.texture = receivedTexture;
    }
}
