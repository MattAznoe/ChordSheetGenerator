using System;
using System.Collections.Generic;
using Cairo;
using CSGen.Model;
using CSGen.Operations;

namespace CSGen.Model.Renderer
{
    /// <summary>
    ///    Vertical region controller allowing control of multiple grouped lines.
    /// </summary>
    public class RegionNode : BaseNode
    {
        protected List<BaseNode> _children = new List<BaseNode>();
        protected PrintRegionType _regionType;
        protected RegionSettings _regionSettings = null;

        public RegionNode(PrintRegionType regionType, RegionSettings regionSettings = null)
        {
            _regionType = regionType;
            _regionSettings = regionSettings;
        }

        public PrintRegionType RegionType
        {
            get
            {
                return _regionType;
            }
        }

        public void AddNode(BaseNode node)
        {
            _children.Add(node);
        }



        public override TextExtents GetExtents(Cairo.Context cr)
        {
            if (!_extentsDetermined)
            {
                _extentsDetermined = true;
                _extents = _children.GetVerticalExtents(cr);

                if (_regionSettings != null)
                {
                    _extents.Height += _regionSettings.PaddingTop + _regionSettings.PaddingBottom;
                    _extents.Width += _regionSettings.Indent + _regionSettings.PaddingLeft + _regionSettings.PaddingRight;
                }
            }
            return _extents;
        }

        public override void Draw(Cairo.Context cr, double xPos, double yPos, PrintLayer layer)
        {
            if (_regionSettings != null)
            {
                xPos += _regionSettings.Indent;

                if (layer == PrintLayer.Background && _regionSettings.HasBackground)
                {
                    TextExtents extents = GetExtents(cr);

                    // If there is a background, draw it here.
                    cr.SetSourceRGB(_regionSettings.Background.Red, _regionSettings.Background.Green, _regionSettings.Background.Blue);

                    cr.Rectangle(xPos
                        , yPos - extents.Height - extents.YAdvance
                        , extents.Width - _regionSettings.Indent 
                        , extents.Height + extents.YAdvance);
                    cr.Fill();

                }
                else if (layer == PrintLayer.Text)
                {
                    xPos += _regionSettings.PaddingLeft;
                    yPos -= _regionSettings.PaddingBottom;
                }
            }
            _children.VerticalDraw(cr, xPos, yPos, layer);
        }
    }
}

