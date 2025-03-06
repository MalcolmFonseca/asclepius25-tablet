using UnityEngine;
using Unity.WebRTC;
using System.Collections;

public class TabletWebRtc : MonoBehaviour
{
    public string signalingServerUrl = "ws://your-signaling-server-ip:8080"; // Replace with your actual signaling server
    private RTCPeerConnection peerConnection;
    private MediaStreamTrack videoTrack;
    public Renderer displayRenderer;

    IEnumerator Start()
    {
        // Initialize WebRTC
        WebRTC.Initialize();
        peerConnection = new RTCPeerConnection();

        // Handle remote track (incoming video from HoloLens)
        peerConnection.OnTrack = (RTCTrackEvent e) =>
        {
            if (e.Track.Kind == TrackKind.Video)
            {
                videoTrack = e.Track;
                var texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                displayRenderer.material.mainTexture = texture;
                StartCoroutine(UpdateVideo(texture));
            }
        };

        // Connect to signaling server
        using (var ws = new WebSocket(signalingServerUrl))
        {
            yield return StartCoroutine(ws.Connect());

            // Exchange SDP offer/answer
            ws.OnMessage += async (byte[] msg) =>
            {
                RTCSessionDescription desc = JsonUtility.FromJson<RTCSessionDescription>(System.Text.Encoding.UTF8.GetString(msg));
                await peerConnection.SetRemoteDescription(ref desc);

                if (desc.type == RTCSdpType.Offer)
                {
                    RTCSessionDescription answer = await peerConnection.CreateAnswer();
                    await peerConnection.SetLocalDescription(ref answer);
                    ws.Send(System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(answer)));
                }
            };
        }
    }

    IEnumerator UpdateVideo(Texture2D texture)
    {
        while (videoTrack != null)
        {
            var frame = ((VideoStreamTrack)videoTrack).CopyFrame();
            if (frame != null)
            {
                texture.LoadRawTextureData(frame.data);
                texture.Apply();
            }
            yield return null;
        }
    }

    private void OnDestroy()
    {
        peerConnection.Close();
        WebRTC.Dispose();
    }
}*/
