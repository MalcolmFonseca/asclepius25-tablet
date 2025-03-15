using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Windows.WebCam;

public class VideoManagerHololens : MonoBehaviour
{
    private WebCamTexture hololensCam;
    private Texture2D videoTexture;

    GameObject videoCanvas;
    PhotonView pV;
    bool streaming = false;

    float refreshRate = .01f; //in seconds
    float timer;
    private void Start()
    {
        hololensCam = new WebCamTexture();
        hololensCam.Play();
        videoTexture = new Texture2D(hololensCam.width, hololensCam.height, TextureFormat.RGB24, false);
    }

    public void BeginStreaming()
    {
        videoCanvas = PhotonNetwork.Instantiate("VideoCanvas", new Vector3(0, 0, 0), Quaternion.identity, 0);
        pV = videoCanvas.GetComponent<PhotonView>();
        streaming = true;
    }

    private void Update()
    {
        try
        {
            if (!streaming)
            {
                BeginStreaming();
            }
        }
        catch (System.Exception)
        {
            return;
        }

        if (hololensCam.didUpdateThisFrame && streaming)
        {
            timer += Time.deltaTime;
            if(timer > refreshRate)
            {
                timer = timer - refreshRate;
                videoTexture.SetPixels(hololensCam.GetPixels());
                videoTexture.Apply();

                byte[] frameData = videoTexture.EncodeToJPG();
                pV.RPC("UpdateFrame", RpcTarget.All, frameData);
            }
        }
    }
}
