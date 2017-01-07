using System;
using System.Collections.Generic;
using Cairo;
using CSGen.Model;
using CSGen.Operations;

namespace CSGen.Model.Renderer
{
    /// <summary>
    ///   Horizontal controller node for managing a line of nodes.
    /// </summary>
    public class ContainerNode : BaseNode
    {
        protected List<BaseNode> _children = new List<BaseNode>();

        public ContainerNode()
        {
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
                _extents = _children.GetHorizontalExtents(cr);
            }
            return _extents;
        }

        public override void Draw(Cairo.Context cr, double xPos, double yPos, PrintLayer layer)
        {
            _children.HorizontalDraw(cr, xPos, yPos, layer);
        }
    }
}

