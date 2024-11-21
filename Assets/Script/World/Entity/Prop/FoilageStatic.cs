using UnityEngine;

public class FoilageStatic : MonoBehaviour
{
    public static FoilageStatic Instance { get; private set; }  
    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        SetSpriteBasedOnPerlinNoise();
        Destroy(this); 
    }
    void SetSpriteBasedOnPerlinNoise()
    {
        float perlinValue = Mathf.PerlinNoise(transform.position.x , transform.position.z ); // Scale the input to increase variation
        perlinValue = Mathf.Clamp01(perlinValue * 3 - 1); // Scale and offset the value to make it more extreme
        int id = Mathf.FloorToInt(perlinValue * 3) + 1; // Generates a value between 1 and 3
        spriteRenderer.sprite = Resources.Load<Sprite>($"texture/sprite/grass{id}"); 
    }
}
