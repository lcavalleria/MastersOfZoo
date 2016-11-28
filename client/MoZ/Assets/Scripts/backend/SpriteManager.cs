using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public enum Position
{
    Up,
    Right,
    Down,
    Left
}
public static class SpriteManager
{
    private static float cardSize;
    private static Dictionary<string, Sprite> sprites;
    private static UnityEngine.Object[] spriteArray;

    static SpriteManager()
    {
        sprites = new Dictionary<string, Sprite>();
        spriteArray = Resources.LoadAll("");
        foreach (Object s in spriteArray)
        {
            if (s.GetType() == typeof(Sprite))
            {
                sprites.Add(s.name, (Sprite)s);
            }
        }
        cardSize = sprites["cardFrontBg"].rect.width;
    }

    public static Sprite GetSprite(string spriteName)
    {
        return sprites[spriteName];
    }
    
}

