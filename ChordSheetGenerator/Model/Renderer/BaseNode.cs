using System;
using System.Collections.Generic;
using Cairo;

namespace CSGen.Model.Renderer
{
    public class BaseNode
    {
        protected bool _extentsDetermined = false;
        protected TextExtents _extents = new TextExtents();

        public BaseNode()
        {
        }

        public virtual TextExtents GetExtents(Cairo.Context cr)
        {
            return _extents;
        }

        public virtual void Draw(Cairo.Context cr, double xPos, double yPos, PrintLayer layer)
        {
        }
    }

    public static class INodeExtensions
    {
        public static TextExtents GetHorizontalExtents (this List<BaseNode> list, Cairo.Context cr)
        {
            TextExtents extents = new TextExtents();
            foreach (BaseNode node in list)
            {
                TextExtents newExtent = node.GetExtents(cr);

                extents.Height = Math.Max(extents.Height, newExtent.Height);
                extents.YAdvance = Math.Max(extents.YAdvance, newExtent.YAdvance);

                extents.Width += newExtent.Width;
            }

            return extents;
        }

        public static TextExtents GetVerticalExtents (this List<BaseNode> list, Cairo.Context cr)
        {
            TextExtents extents = new TextExtents();
            foreach (BaseNode node in list)
            {
                TextExtents newExtent = node.GetExtents(cr);

                extents.Width = Math.Max(extents.Width, newExtent.Width);

                extents.Height += newExtent.Height;
                extents.YAdvance += newExtent.YAdvance;
            }

            return extents;
        }

        public static void HorizontalDraw(this List<BaseNode> list, Cairo.Context cr, double xPos, double yPos, PrintLayer layer)
        {
            // Draw all of the children side-by-side
            foreach (BaseNode node in list)
            {
                TextExtents extents = node.GetExtents(cr);

                node.Draw(cr, xPos, yPos, layer);

                xPos += extents.Width;
            }
        }

        public static void VerticalDraw(this List<BaseNode> list, Cairo.Context cr, double xPos, double yPos, PrintLayer layer)
        {
            // Draw all of the children vertically, but to keep the lines correctly, we will actually
            // render them in reverse from the bottom up.
            for (int i = list.Count - 1; i >= 0; i--)
            {
                BaseNode node = list[i];
                TextExtents extents = node.GetExtents(cr);

                node.Draw(cr, xPos, yPos, layer);

                yPos -= extents.Height + extents.YAdvance;
            }
        }

    }

}

