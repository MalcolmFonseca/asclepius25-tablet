using UnityEngine;
using Microsoft.MixedReality.WebRTC;
using System.Threading.Tasks;

public class HoloLensWebRTC : MonoBehaviour
{
    public PeerConnection peerConnection;
    public CameraCapture cameraCapture;
    public VideoTrackSource videoSource;

    async void Start()
    {
        await peerConnection.InitializeAsync();
        cameraCapture = gameObject.AddComponent<CameraCapture>();
        cameraCapture.StartCapture();
        videoSource = cameraCapture.VideoTrackSource;
        peerConnection.AddLocalVideoTrack(videoSource);
    }
}