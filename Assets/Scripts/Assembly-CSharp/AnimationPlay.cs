using UnityEngine;

public class AnimationPlay : MonoBehaviour
{
	public int colCount = 4;

	public int rowCount = 4;

	public int rowNumber;

	public int colNumber;

	public int totalCells = 4;

	public int fps = 10;

	private Vector2 offset;

	private Vector2 size;

	private int index;

	private float timing;

	private void Update()
	{
		SetSpriteAnimation(colCount, rowCount, rowNumber, colNumber, totalCells, fps);
	}

	private void SetSpriteAnimation(int colCount, int rowCount, int rowNumber, int colNumber, int totalCells, int fps)
	{
		timing = Time.time;
		index = (int)(timing * (float)fps);
		index %= totalCells;
		size = new Vector2(1f / (float)colCount, 1f / (float)rowCount);
		int num = index % colCount;
		int num2 = index / colCount;
		offset = new Vector2((float)(num + colNumber) * size.x, 1f - size.y - (float)(num2 + rowNumber) * size.y);
		GetComponent<Renderer>().material.SetTextureOffset("_MainTex", offset);
		GetComponent<Renderer>().material.SetTextureScale("_MainTex", size);
	}
}
