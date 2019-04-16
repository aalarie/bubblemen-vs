using UnityEngine;
using System.Collections;

public class TextureOffset: MonoBehaviour 
{
	public float scrollSpeed = 0.5F;
	private Renderer rend;

	void Start() 
    {
		rend = GetComponent<Renderer>();
	}

	void Update() 
    {
		float offsetX = Time.time * scrollSpeed;
        float offsetY = offsetX;

        rend.material.SetTextureOffset("_MainTex", new Vector2(offsetX, offsetY));
	}
}