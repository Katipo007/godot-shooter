using System;
using Godot;
using GC = Godot.Collections;
using SC = System.Collections.Generic;

namespace phios {

    public class BitmapFont : Godot.Resource {
        [Export(PropertyHint.File, "*.xml,*.fnt")]
        public readonly string BitmapFontXml;

        [Export]
        public readonly Material BitmapFontMaterial;
        [Export]
        public readonly float GlyphHeight;
        [Export]
        public readonly float GlyphWidth;
        [Export]
        public float QuadHeightScale;

        public float TextureSize { get; private set; }

        public bool Loaded { get; private set; } = false;

        private SC.Dictionary<string, BitmapFontGlyph> _glyphs = new SC.Dictionary<string, BitmapFontGlyph>();

        public BitmapFont() {
            GD.Print($"{ResourceName}._Init()");
            XMLParser xml = new XMLParser();
            Error e = xml.Open(BitmapFontXml);

            if (e != Error.Ok) {
                throw new FieldAccessException($"Failed to open font xml {BitmapFontXml}: {e}");
            }

            while (xml.Read() == Error.Ok) {
                GD.Print($"XML: {xml.GetNodeName()}");
                // parse texture size
                if (xml.GetNodeName() == "common") {
                    TextureSize = float.Parse(xml.GetNamedAttributeValue("scaleW"));
                    GD.Print($"Got font common. TextureSize: {TextureSize}");
                }

                // parse glyph
                else if (xml.GetNodeName() == "char") {
                    string glyphString = char.ConvertFromUtf32(int.Parse(xml.GetNamedAttributeValue("id")));
                    GD.Print($"Found glyph '{glyphString}'");

                    // new glyph
                    if (!_glyphs.ContainsKey(glyphString)) {
                        var glyph = new BitmapFontGlyph();
                        glyph.GlyphString = glyphString;
                        glyph.x = float.Parse(xml.GetNamedAttributeValue("x"));
                        glyph.y = float.Parse(xml.GetNamedAttributeValue("y"));
                        glyph.xOffset = float.Parse(xml.GetNamedAttributeValue("xOffset"));
                        glyph.yOffset = float.Parse(xml.GetNamedAttributeValue("yOffset"));
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

        public BitmapFontGlyph GetGlyph(string glyphString) {
            BitmapFontGlyph glyph;

            // return specified glyph
            if (_glyphs.TryGetValue(glyphString, out glyph)) {
                return glyph;
            }

            // not found
            else {
                return _glyphs["?"];
            }
        }
    } // end class
} // end namespace