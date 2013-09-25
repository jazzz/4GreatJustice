using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

public class DepthToTexture : MonoBehaviour {
    public DeviceOrEmulator device;

    Thread thread;

    Color32[] buffer;

    public Texture2D texture;

    Kinect.KinectInterface kinect;

    int size;
    public int lookBack = 1;
    void Start() {
        
        kinect = device.getKinect();
        var resolution = kinect.DepthResolution;

        var width = resolution == Kinect.NuiImageResolution.resolution320x240 ? 320 : 640;
        var height = resolution == Kinect.NuiImageResolution.resolution320x240 ? 240 : 480;

        texture = new Texture2D(width, height, TextureFormat.ARGB32, false, true);

        size = width * height;
        buffer = new Color32[size];

        thread = new Thread(() => {

            List<short[]> last = new List<short[]>();
            for (int i = 0; i < 10; ++i) {
                last.Add(new short[size]);
            }
            while (true) {
                if (kinect.pollDepth()) {
                    var depths = kinect.getDepth();
                    Color32[] writeTo = new Color32[size];
                    for (int i = 0; i < size; ++i) {
                        var s = depths[i];

                        //check for sudden 0-depth pixels, replace with old value
                        for (int j = 0; s == 0 && j < lookBack; ++j) {
                            s = last[j][i];
                        }
                        //make unknown depth far, instead of near
                        if (s == 0) {
                            s = unchecked((short)0xFFFF);
                        }
                        writeTo[i].r = (byte)((s >> 0) & 0xffff);
                        writeTo[i].g = (byte)((s >> 8) & 0xffff);
                    }

                    buffer = writeTo;
                    last.RemoveAt(last.Count - 1);
                    last.Insert(0, depths);
                }
                Thread.Sleep(1);
            }
        });
        thread.Start();

        backgroudDepth = RenderTexture.GetTemporary(width, height);

    }
    public Material material;

    void Update() {
        //grab latest buffer
        var b = Interlocked.Exchange(ref buffer, null);
        if (b == null) return;

        texture.SetPixels32(b);
        texture.Apply(false);

        if (Input.GetKeyDown(KeyCode.Space)) {
            Graphics.Blit(texture, backgroudDepth);
        }
    }

    public RenderTexture backgroudDepth;
    void OnRenderImage(RenderTexture src, RenderTexture dst) {
        material.SetTexture("_CurrentDepth", texture);
        material.SetTexture("_BackgroundDepth", backgroudDepth);
        Graphics.Blit(src, dst, material);
    }
    void OnDestroy() {
        if (thread != null) thread.Abort();
    }
}

