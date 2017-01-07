using System;
using Cairo;
using CSGen.Model;
using CSGen.Operations;

namespace CSGen.Model.Renderer
{
    public class SpacerNode : BaseNode
    {
        private FontInfo _fontInfo;
        private double _width;

        public SpacerNode(FontInfo font, double width = 0.0)
        {
            _fontInfo = font; 
            _width = width;
        }

        public override TextExtents GetExtents(Cairo.Context cr)
        {
            if (!_extentsDetermined)
            {
                _extentsDetermined = true;
                _extents.Height = _fontInfo.MaxHeight;
                _extents.Width = _width;
            }

            return _extents;
        }
    }
}

