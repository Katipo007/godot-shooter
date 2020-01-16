using System;
using Godot;

namespace phios {
    public class BitmapFontGlyph {
        public string GlyphString;
        public float x;
        public float y;
        public float xOffset;
        public float yOffset;
        public float Width;
        public float Height;
        public Vector3[] Vertices = new Vector3[4];
        public Vector2[] UVs = new Vector2[4];

        public void RecalculateGlyphMetrics(float glyphWidth, float glyphHeight, float textureSize, float bleed) {

            // calculate glyph vertices
            Vertices[0] = new Vector3(0f + xOffset / glyphWidth, 0f + yOffset / glyphHeight, 0f);
            Vertices[1] = new Vector3(1f * (Width / glyphWidth) + xOffset / glyphWidth, 1f * (Height / glyphHeight) + yOffset / glyphHeight, 0f);
            Vertices[2] = new Vector3(1f * (Width / glyphWidth) + xOffset / glyphWidth, 0f + yOffset / glyphHeight, 0f);
            Vertices[3] = new Vector3(0f + xOffset / glyphWidth, 1f * (Height / glyphHeight) + yOffset / glyphHeight, 0f);

            // calculate glyph uvs
            UVs[0] = new Vector2((x / textureSize) + (bleed / textureSize), ((textureSize - (y + Height)) / textureSize) + (bleed / textureSize));
            UVs[1] = new Vector2(((x + Width) / textureSize) - (bleed / textureSize), ((textureSize - y) / textureSize) - (bleed / textureSize));
            UVs[2] = new Vector2(((x + Width) / textureSize) - (bleed / textureSize), ((textureSize - (y + Height)) / textureSize) + (bleed / textureSize));
            UVs[3] = new Vector2((x / textureSize) + (bleed / textureSize), ((textureSize - y) / textureSize) - (bleed / textureSize));
        }
    } // end class
} // end namespace