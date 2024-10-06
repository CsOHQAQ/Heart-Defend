using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwistTex : TextureGenerator
{
    public Gradient gradient = new Gradient();

    public override Texture2D Generate(Vector2Int size)
    {
        Texture2D texture = new Texture2D(size.x, size.y);
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, new Color((float)x / texture.width, (float)y / texture.height, gradient.Evaluate(2 * Vector2.Distance(new Vector2((float)x / texture.width, (float)y / texture.height), new Vector2(0.5f, 0.5f))).b, 1));
            }
        }
        texture.Apply();
        return texture;
    }
}