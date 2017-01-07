using System;
using System.Collections.Generic;
using Cairo;
using CSGen.Model;
using CSGen.Operations;

namespace CSGen.Model.Renderer
{
    public class HeaderFooterNode : BaseNode
    {
        private TextNode[] _fields = new TextNode[3];
        private LineNode _lineNode = null;
        private double _pageWidth;
        private bool _isHeader;
        private SongData _song;


        public HeaderFooterNode(bool isHeader
            , double pageWidth
            , FontInfo font
            , PrintLineType lineType
            , PrintFieldType leftField
            , PrintFieldType centerField
            , PrintFieldType rightField
            , SongData song)
        {
            _isHeader = isHeader;
            _song = song;
            _pageWidth = pageWidth;

            _fields[0] = GetTextNode(leftField, font);
            _fields[1] = GetTextNode(centerField, font);
            _fields[2] = GetTextNode(rightField, font);

            if (lineType != PrintLineType.None)
            {
                _lineNode = new LineNode(_pageWidth, 0, 0, lineType);
            }
        }


        public void SetPages(int pageNumber, int totalPages)
        {
            pageNumber++;
            foreach (TextNode node in _fields)
            {
                node.Text = node.Text.Replace("#PageNum", pageNumber.ToString());
                node.Text = node.Text.Replace("#PageTotal", totalPages.ToString());
            }
        }


        private TextNode GetTextNode(PrintFieldType fieldType, FontInfo font)
        {
            TextNode node = null;
            string text = string.Empty;
            switch (fieldType)
            {
                case PrintFieldType.Title:
                    text = _song.Title;
                    break;
                case PrintFieldType.Artist:
                    text = _song.Artist;
                    break;
                case PrintFieldType.CCLI:
                    text = _song.CCLI;
                    break;
                case PrintFieldType.Copyright:
                    text = _song.Copyright;
                    break;
                case PrintFieldType.PageNum:
                    text = "#PageNum"; 
                    break;
                case PrintFieldType.PageNumOfTotal:
                    text = "#PageNum of #PageTotal";
                    break;
            }

            node = new TextNode(text, font);

            return node;                
        }



        public override TextExtents GetExtents(Cairo.Context cr)
        {
            if (!_extentsDetermined)
            {
                _extents.Height = _fields[0].GetExtents(cr).Height + _fields[0].GetExtents(cr).YAdvance;

                if (_lineNode != null)
                    _extents.Height += _lineNode.GetExtents(cr).Height;

                _extents.Width = _pageWidth;
                _extentsDetermined = true;
            }
            return _extents;
        }

        public override void Draw(Cairo.Context cr, double xPos, double yPos, PrintLayer layer)
        {     
            if (_isHeader)
            {
                if (_lineNode != null)
                {
                    _lineNode.Draw(cr, xPos, yPos, layer);
                    yPos -= _lineNode.GetExtents(cr).Height + _fields[0].GetExtents(cr).YAdvance;
                }

                DrawText(cr, xPos, yPos, layer);
            }
            else
            {
                DrawText(cr, xPos, yPos, layer);

                if (_lineNode != null)
                {
                    yPos -= _fields[0].GetExtents(cr).Height + _fields[0].GetExtents(cr).YAdvance;
                    _lineNode.Draw(cr, xPos, yPos, layer);
                }                    
            }

        }


        private void DrawText(Cairo.Context cr, double xPos, double yPos, PrintLayer layer)
        {
            _fields[0].Draw(cr, xPos, yPos, layer);
            _fields[1].Draw(cr, xPos + (_pageWidth / 2.0) - (_fields[1].GetExtents(cr).Width / 2.0), yPos, layer);
            _fields[2].Draw(cr, xPos + _pageWidth - _fields[2].GetExtents(cr).Width, yPos, layer);

        }

    }
}


