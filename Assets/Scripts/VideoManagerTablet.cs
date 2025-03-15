using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Windows.WebCam;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;
using UnityEngine.UI;
using UnityEngine.Playables;

public class VideoManagerTablet : MonoBehaviour
{
    TcpListener server = null;
    TcpClient client = null;
    NetworkStream stream = null;
    Thread thread;

    private Texture2D receivedTexture;
    [SerializeField] private RawImage videoImage;

    private void Start()
    {
        thread = new Thread(new ThreadStart(SetupServer));
        thread.Start();
    }

    private void Update()
    {

    }

    private void SetupServer()
    {
        try
        {
            IPAddress localAddr = IPAddress.Any;
            server = new TcpListener(localAddr, 1984);
            server.Start();

            byte[] buffer = new byte[73744];

            while (true)
            {
                Debug.Log("Connecting to Video Server...");
                client = server.AcceptTcpClient();
                Debug.Log("Connected to Video Server");

                stream = client.GetStream();

                int i;

                while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    //buffer now contains sent byte array
                    UpdateFrame(buffer);
                }
                client.Close();
            }
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e);
        }
        finally
        {
            server.Stop();
        }
    }

    private void OnApplicationQuit()
    {
        stream.Close();
        client.Close();
        server.Stop();
        thread.Abort();
    }

    public void SendMessageToClient(string message)
    {
        byte[] msg = Encoding.UTF8.GetBytes(message);
        stream.Write(msg, 0, msg.Length);
        Debug.Log("Sent: " + message);
    }

    private void UpdateFrame(byte[] frameData)
    {
        receivedTexture.LoadImage(frameData);
        receivedTexture.Apply();
        videoImage.texture = receivedTexture;
    }
}
