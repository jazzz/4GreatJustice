using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class DisplayDepth : MonoBehaviour {
	
	public DepthWrapper dw;
	
	private Texture2D tex;
	// Use this for initialization
	void Start () {
		tex = new Texture2D(dw.depthWidth,dw.depthHeight,TextureFormat.ARGB32,false);
		renderer.material.mainTexture = tex;
	}
	
	// Update is called once per frame
	void Update () {
        //var depths = dw.processDepthOnly();
        //if (depths != null) {
        //    tex.SetPixels32(convertDepthToColor(depths));
        //    tex.Apply(false);
        //}
		if (dw.pollDepth())
		{
			tex.SetPixels32(convertDepthToColor(dw.depthImg));
			//tex.SetPixels32(convertPlayersToCutout(dw.segmentations));
			tex.Apply(false);
		}
	}
	
	private Color32[] convertDepthToColor(short[] depthBuf)
	{
		Color32[] img = new Color32[depthBuf.Length];
		for (int pix = 0; pix < depthBuf.Length; pix++)
		{
			img[pix].r = (byte)(depthBuf[pix] / 32);
			img[pix].g = (byte)(depthBuf[pix] / 32);
			img[pix].b = (byte)(depthBuf[pix] / 32);
		}
		return img;
	}
	
	private Color32[] convertPlayersToCutout(bool[,] players)
	{
        var size = dw.depthHeight * dw.depthWidth;
		Color32[] img = new Color32[size];
		for (int pix = 0; pix < size; pix++)
		{
			if(players[0,pix]|players[1,pix]|players[2,pix]|players[3,pix]|players[4,pix]|players[5,pix])
			{
				img[pix].a = (byte)255;
			} else {
				img[pix].a = (byte)0;
			}
		}
		return img;
	}
}
