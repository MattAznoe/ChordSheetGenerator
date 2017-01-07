using System;
using CSGen.Model;
using Gtk;
using Cairo;

namespace CSGen.Operations
{
    public interface IPageRenderer
    {
        PageSetup PageSetup { get; }
        AppSettings AppSettings { get; set; }
        int Pages { get; }
        int DisplayPaperHeight { get; }
        int ActualPaperHeight { get; }

        /// <summary>
        ///  For display rendering only
        /// </summary>
        /// <value>The height of the max.</value>
        int DisplayHeightExtent { get; }
        int DisplayWidthExtent { get; }


        int CalculatePages(Cairo.Context cr);
        void DrawPage (int pageIndex, int offset = 0);
    }
}

