using System;
using System.Collections.Generic;
using Cairo;
using CSGen.Model;
using CSGen.Operations;

namespace CSGen.Model.Renderer
{
    public class LineNode : BaseNode
    {
        private PrintLineType _lineType;
        private double _lineThickness;

        /// <summary>
        /// Initializes a new instance of the <see cref="CSGen.Model.Renderer.LineNode"/> class.
        /// </summary>
        /// <param name="extents">The maximum dimensions for the space the line should occupy.</param>
        /// <param name="lineThickness">The width of the line.</param>
        public LineNode(double width, double height, double yAdvance, PrintLineType lineType)
        {
            _extents = new TextExtents();

            switch (lineType)
            {
                case PrintLineType.Single:
                    _lineThickness = 0.5;
                    if (height == 0)
                        height = 3;
                    break;

                case PrintLineType.Double:
                    _lineThickness = 0.5;
                    if (height == 0)
                        height = 6;
                    break;
                case PrintLineType.Thick:
                    _lineThickness = 2.0;
                    if (height == 0)
                        height = 6;
                    break;              
                case PrintLineType.None:
                    break;
            }

            _extents.Width = width;
            _extents.Height = height;
            _extents.YAdvance = yAdvance;
            _lineType = lineType;
        }

        public override TextExtents GetExtents(Cairo.Context cr)
        {
            return _extents;
        }

        public override void Draw(Cairo.Context cr, double xPos, double yPos, PrintLayer layer)
        {
            if (layer == PrintLayer.Text)
            {
                yPos -= _extents.YAdvance;

                // Only draw the line if there is enough space to display it
                if (_extents.Width > 2)
                {
                    yPos -= (_extents.Height / 2) - (_lineThickness / 2);

                    cr.Rectangle(xPos
                        , yPos
                        , _extents.Width 
                        , _lineThickness);
                    cr.Fill();
                }

                if (_lineType == PrintLineType.Double)
                {
                    yPos -= 4;

                    cr.Rectangle(xPos
                        , yPos
                        , _extents.Width 
                        , _lineThickness);
                    cr.Fill();
                }
            }
        }
    }
}

