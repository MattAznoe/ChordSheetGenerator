using System;
using System.Collections.Generic;
using Cairo;
using CSGen.Model;
using CSGen.Operations;

namespace CSGen.Model.Renderer
{
    public class PageNode : RegionNode
    {

        private double _currentYPos = 0;
        private int _pageNumber = 0;
        private PageSettings _pageSettings;
        private AppSettings _appSettings;
        private HeaderFooterNode _header = null;
        private HeaderFooterNode _footer = null;
        private double _availablePageHeight = 0;
        private RegionNode _currentRegion = null;

        #region Constructor
        public PageNode() : base(PrintRegionType.None)
        {
            _pageSettings = new PageSettings();
            _appSettings = new AppSettings();
        }

        public PageNode(int pageNumber, PageSettings pageSettings, AppSettings appSettings, SongData song, PrintRegionType startingRegionType = PrintRegionType.None)
            : base (PrintRegionType.None)
        {
            if (startingRegionType != PrintRegionType.None)
            {
                BeginRegion(startingRegionType);
            }

            _pageNumber = pageNumber;
            _pageSettings = pageSettings;
            _appSettings = appSettings;

            _currentYPos = pageSettings.TopMargin;

            FontInfo headerFooterFont = appSettings.GetFont(PrintFontType.HeaderFooterText);

            if (appSettings.ShowHeader)
            {
                _header = new HeaderFooterNode(
                    true
                    , _pageSettings.PageWidth
                    , headerFooterFont
                    , appSettings.HeaderLineType
                    , appSettings.HeaderFieldLeft
                    , appSettings.HeaderFieldCenter
                    , appSettings.HeaderFieldRight
                    , song
                );
            }

            if (appSettings.ShowFooter)
            {
                _footer = new HeaderFooterNode(
                    false
                    , _pageSettings.PageWidth
                    , headerFooterFont
                    , appSettings.FooterLineType
                    , appSettings.FooterFieldLeft
                    , appSettings.FooterFieldCenter
                    , appSettings.FooterFieldRight
                    , song
                );
            }
        }
        #endregion

        public PrintRegionType CurrentRegionType
        {
            get
            {
                if (_currentRegion != null)
                    return _currentRegion.RegionType;
                return PrintRegionType.None;
            }
        }

        public void BeginRegion (PrintRegionType regionType)
        {
            RegionSettings settings = _appSettings.GetRegionSetting(regionType);
            _currentRegion = new RegionNode(regionType, settings);

            // Add the default padding values.
            _currentYPos += settings.PaddingTop + settings.PaddingBottom;

            _children.Add(_currentRegion);
        }

        public void EndRegion ()
        {
            _currentRegion = null;
        }

        public bool TryAddLine(Cairo.Context cr, BaseNode lineNode)
        {
            // If we have not yet determined the actual page height, calculate it now.
            if (_availablePageHeight == 0)
            {
                _availablePageHeight = _pageSettings.PageHeight;

                if (_header != null)
                    _availablePageHeight -= _header.GetExtents(cr).Height;

                if (_footer != null)
                    _availablePageHeight -= _footer.GetExtents(cr).Height;                                 
            }

            // Now see if the text field will fit on our page.
            bool retval = false;
            TextExtents extents = lineNode.GetExtents(cr);
            if (_currentYPos + extents.Height < _availablePageHeight)
            {
                if (_currentRegion != null)
                    _currentRegion.AddNode(lineNode);
                else
                    _children.Add(lineNode);
                retval = true;

                _currentYPos += extents.Height + extents.YAdvance;
            }

            return retval;
        }


        public void SetTotalPageCount (int pageCount)
        {
            if (_header != null)
                _header.SetPages(_pageNumber, pageCount);
            if (_footer != null)
                _footer.SetPages(_pageNumber, pageCount);
        }


        //        public override TextExtents GetExtents(Cairo.Context cr)
        //        {
        //            TextExtents extents = new TextExtents();
        //            extents.Height = _pageSettings.PageHeight;
        //            extents.Width = _pageSettings.PageWidth;           
        //            return extents;
        //        }

        public override void Draw(Cairo.Context cr, double xPos, double yPos, PrintLayer layer)
        {          
            if (layer == PrintLayer.DisplayBackground)
            {
                // Create the shadow effect
                cr.SetSourceRGBA (0.5, 0.5, 0.5, 0.5);
                cr.Rectangle (xPos + Constants.SHADOW_WIDTH
                    , yPos + Constants.SHADOW_WIDTH
                    , _pageSettings.PaperWidth
                    , _pageSettings.PaperHeight);
                cr.Fill ();



                cr.SetSourceRGB (1, 1, 1);
                cr.Rectangle (xPos
                    , yPos
                    , _pageSettings.PaperWidth
                    , _pageSettings.PaperHeight);
                cr.Fill ();

            }

            double pageTop = yPos;

            yPos += _pageSettings.TopMargin;
            xPos += _pageSettings.LeftMargin;

            if (_header != null)
            {
                yPos += _header.GetExtents(cr).Height;
                _header.Draw(cr, xPos, yPos, layer);
            }

            // Move the cursor to the bottom of the page elements.  
            TextExtents extents = GetExtents(cr);
            yPos += extents.Height + extents.YAdvance; 

            // Draw the page elements in reverse order.
            base.Draw(cr, xPos, yPos, layer);

            if (_footer != null)
            {
                yPos = pageTop + _pageSettings.PageHeight;
                _footer.Draw(cr, xPos, yPos, layer);
            }
        }
    }
}

