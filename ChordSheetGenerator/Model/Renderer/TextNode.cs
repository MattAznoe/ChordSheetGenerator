using System;
using Cairo;
using CSGen.Model;
using CSGen.Operations;


namespace CSGen.Model.Renderer
{
    public class TextNode : BaseNode
    {
        private string _text;
        private FontInfo _fontInfo;
        private int _addSpaces;

        public TextNode(string text, FontInfo fontInfo, int addSpaces = 0)
        {
            _text = text;
            _fontInfo = fontInfo;

            _addSpaces = addSpaces;
        }

        public FontInfo Font { get { return _fontInfo; } }

        /// <summary>
        ///   Reset the text contents.
        /// </summary>
        /// <param name="text">Text.</param>
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                _extentsDetermined = false;
            }
        }

        public override TextExtents GetExtents(Cairo.Context cr)
        {
            if (!_extentsDetermined)
            {
                _extentsDetermined = true;

                cr.SetFont(_fontInfo);

                _extents = cr.TextExtents(_text);     
                _extents.Height = _fontInfo.MaxHeight;  // always use max height to ensure vertical alignment.

                if (_addSpaces > 0)
                    _extents.Width += _addSpaces * _fontInfo.SpaceWidth;
                else
                    _extents.Width += _extents.XAdvance - _extents.Width;

                if (_fontInfo.Underline)
                    _extents.Height += _fontInfo.UnderlineHeight + 2.0;

                _extents.YAdvance = _fontInfo.LineOffset;
            }
            return _extents;
        }

        public override void Draw(Cairo.Context cr, double xPos, double yPos, PrintLayer layer)
        {
            if (layer == PrintLayer.Text)
            {
                cr.SetFont(_fontInfo);

                yPos -= _extents.YAdvance;

                if (_fontInfo.Underline == true)
                {
                    TextExtents extents = GetExtents(cr);

                    cr.Rectangle(xPos
                        , yPos - _fontInfo.UnderlineHeight
                        , extents.Width
                        , _fontInfo.UnderlineHeight);

                    cr.Fill();

                    yPos -= _fontInfo.UnderlineHeight + 2.0;
                }

                cr.MoveTo(xPos, yPos);
                cr.ShowText(_text);
            }
        }

    }
}

