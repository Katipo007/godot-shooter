using System;
using Godot;
using GC = Godot.Collections;
using SC = System.Collections.Generic;

namespace Phios
{

    public class BitmapFont : Godot.Resource
    {
        [Export(PropertyHint.File, "*.xml,*.fnt")]
        public readonly string BitmapFontXml;

        [Export]
        public readonly Material BitmapFontMaterial;
        [Export]
        public readonly float GlyphHeight = 0f;
        [Export]
        public readonly float GlyphWidth = 0f;
        [Export]
        public float QuadHeightScale = 1f;

        public float TextureSize { get; private set; }

        public bool Loaded { get; private set; } = false;

        private SC.Dictionary<string, BitmapFontGlyph> _glyphs = new SC.Dictionary<string, BitmapFontGlyph>();

        public void Init()
        {
            // exit early if already loaded
            if (Loaded)
                return;

            GD.Print("phios.BitmapFont is being setup");
            XMLParser xml = new XMLParser();
            Error e = xml.Open(BitmapFontXml);

            if (e != Error.Ok)
            {
                throw new FieldAccessException($"Failed to open font xml {BitmapFontXml}: {e}");
            }

            while (xml.Read() == Error.Ok)
            {
                if (xml.GetNodeType() != XMLParser.NodeType.Element)
                    continue;

                string nodeName = xml.GetNodeName();
                if (nodeName == "")
                    continue;

                // parse texture size
                if (nodeName == "common")
                {
                    TextureSize = float.Parse(xml.GetNamedAttributeValue("scaleW"));
                }

                // parse glyph
                else if (nodeName == "char")
                {
                    string glyphString = char.ConvertFromUtf32(int.Parse(xml.GetNamedAttributeValue("id")));
                    // new glyph
                    if (!_glyphs.ContainsKey(glyphString))
                    {
                        var glyph = new BitmapFontGlyph();
                        glyph.GlyphString = glyphString;
                        glyph.x = float.Parse(xml.GetNamedAttributeValue("x"));
                        glyph.y = float.Parse(xml.GetNamedAttributeValue("y"));
                        glyph.xOffset = float.Parse(xml.GetNamedAttributeValue("xoffset"));
                        glyph.yOffset = float.Parse(xml.GetNamedAttributeValue("yoffset"));
                        glyph.Width = float.Parse(xml.GetNamedAttributeValue("width"));
                        glyph.Height = float.Parse(xml.GetNamedAttributeValue("height"));
                        _glyphs.Add(glyphString, glyph);
                        glyph.RecalculateGlyphMetrics(GlyphWidth, GlyphHeight, TextureSize, 0f);
                    }
                }
            }

            // finished loading
            Loaded = true;
            GD.Print($"FONT TEXTURE SIZE: {TextureSize}");
            GD.Print($"{_glyphs.Count} GLYPHS LOADED!");
        }

        public BitmapFontGlyph GetGlyph(string glyphString)
        {
            BitmapFontGlyph glyph;

            // return specified glyph
            if (_glyphs.TryGetValue(glyphString, out glyph))
            {
                return glyph;
            }

            // not found
            else
            {
                return _glyphs["?"];
            }
        }
    } // end class
} // end namespace
