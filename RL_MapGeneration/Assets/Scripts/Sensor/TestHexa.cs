using UnityEngine;

public class HexClusterToTexture : MonoBehaviour
{
    public int width = 512;
    public int height = 512;
    public Color hexColor = Color.green;

    void Start()
    {
        // Create a texture
        Texture2D texture = new Texture2D(width, height);

        // Calculate the center of the texture
        Vector2 center = new Vector2(width / 2, height / 2);

        // Calculate the size of each hexagon
        float hexSize = Mathf.Min(width, height) / 10f;

        // Draw hexagons on the texture
        DrawHexCluster(texture, center, hexSize);

        // Apply changes and assign the texture to the Renderer's material
        texture.Apply();
        GetComponent<Renderer>().material.mainTexture = texture;
    }

    void DrawHexCluster(Texture2D texture, Vector2 center, float hexSize)
    {
        // Draw hexagons in a cluster
        for (int row = 0; row < 5; row++) {
            int hexagonsInRow = 5 - Mathf.Abs(2 - row);
            float yOffset = row * Mathf.Sqrt(3) * hexSize;

            for (int col = 0; col < hexagonsInRow; col++) {
                float xOffset = col * 1.5f * hexSize;

                // Offset every other row
                if (row % 2 == 1) {
                    xOffset += 0.75f * hexSize;
                }

                // Calculate hexagon position
                Vector2 hexagonPosition = center + new Vector2(xOffset, yOffset);

                // Draw hexagon
                DrawHexagon(texture, hexagonPosition, hexSize);
            }
        }
    }

    void DrawHexagon(Texture2D texture, Vector2 center, float size)
    {
        for (int angle = 0; angle < 360; angle += 60) {
            // Calculate hexagon vertex position
            float x = center.x + size * Mathf.Cos(Mathf.Deg2Rad * angle);
            float y = center.y + size * Mathf.Sin(Mathf.Deg2Rad * angle);

            // Set pixel color at the calculated position
            texture.SetPixel(Mathf.RoundToInt(x), Mathf.RoundToInt(y), hexColor);
        }
    }
}