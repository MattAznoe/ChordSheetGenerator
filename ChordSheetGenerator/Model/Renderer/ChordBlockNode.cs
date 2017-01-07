using System;
using System.Collections.Generic;
using Cairo;
using CSGen.Model;
using CSGen.Operations;

namespace CSGen.Model.Renderer
{
    public class ChordBlockNode : BaseNode
    {
        protected TextExtents _totalExtents = new TextExtents();
        protected TextExtents _chordExtents = new TextExtents();

        protected List<BaseNode> _chordLine = new List<BaseNode>();
        protected List<BaseNode> _children = new List<BaseNode>();

        protected bool _breaksWord;

        public ChordBlockNode(TextNode chord, TextNode text, bool breaksWord = false)
        {
            _chordLine.Add(chord);
            _children.Add(text);

            _breaksWord = breaksWord;
        }



        public override TextExtents GetExtents(Cairo.Context cr)
        {
            if (!_extentsDetermined)
            {
                _extentsDetermined = true;

                _chordExtents = _chordLine.GetHorizontalExtents(cr);
                _extents = _children.GetHorizontalExtents(cr);

                // Check to see if we need to add a word connecting line.
                if (_children.Count > 0 && _extents.Width < _chordExtents.Width)
                {
                    TextNode textNode = (TextNode)_children[0];
                    if (textNode != null && _breaksWord)
                    {       
                        _children.Add(new SpacerNode(textNode.Font, 1));
                        double len = _chordExtents.Width - _extents.Width - 2;
                        if (len > 0)
                        {
                            LineNode lineNode = new LineNode(len, _extents.Height, _extents.YAdvance, PrintLineType.Single);

                            _children.Add(lineNode);

                            _extents = _children.GetHorizontalExtents(cr);
                        }
                    }                
                }

                _totalExtents = new TextExtents();
                _totalExtents.Width = Math.Max(_chordExtents.Width, _extents.Width);
                _totalExtents.Height = _chordExtents.Height + _chordExtents.YAdvance + _extents.Height;
                _totalExtents.YAdvance = _extents.YAdvance;
            }

            return _totalExtents;

        }


        public override void Draw(Cairo.Context cr, double xPos, double yPos, PrintLayer layer)
        {                        
            _children.HorizontalDraw(cr, xPos, yPos, layer);

            if (_chordLine.Count > 0)
            {
                yPos -= _extents.Height;

                TextNode chord = _chordLine[0] as TextNode;
                if (chord != null)
                {
                    yPos -= _chordExtents.YAdvance;
                }

                _chordLine.HorizontalDraw(cr, xPos, yPos, layer);
            }
        }
    }
}

