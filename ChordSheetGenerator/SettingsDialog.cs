using System;
using System.Collections.Generic;
using CSGen.Model;
using CSGen.Controls;
using CSGen.Operations;

namespace CSGen
{
    public partial class SettingsDialog : Gtk.Dialog
    {
        private List<FontSelectionGroup> _fontControls = new List<FontSelectionGroup>();
        private AppSettings _workingAppSettings;

        public SettingsDialog (AppSettings appSettings)
        {
            this.Build ();

            settingsPane.Page = 0;

            if(!string.IsNullOrEmpty(appSettings.DefaultFolder))
                defaultFolder.SetUri (appSettings.DefaultFolder);

            #region Initialize Fonts
            _workingAppSettings = appSettings.Clone ();

            // Hook up the font controls
            _fontControls.Add (fontselectiongroup1);
            _fontControls.Add (fontselectiongroup2);
            _fontControls.Add (fontselectiongroup3);
            _fontControls.Add (fontselectiongroup4);
            _fontControls.Add (fontselectiongroup5);
            _fontControls.Add (fontselectiongroup6);

            // Build the font controls.
            for (int i = 0; i < _fontControls.Count; i++)
            {
                FontSelectionGroup ctrl = _fontControls [i];
                ctrl.Index = i;
                switch ((PrintFontType)i)
                {
                    case PrintFontType.Title:
                        ctrl.Title = "Song Title:";
                        break;
                    case PrintFontType.SongInfo:
                        ctrl.Title = "Song Information Fields:";
                        break;
                    case PrintFontType.Text:
                        ctrl.Title = "Song Lyrics:";
                        break;
                    case PrintFontType.Chords:
                        ctrl.Title = "Chords:";
                        break;
                    case PrintFontType.Comments:
                        ctrl.Title = "Comments: ";
                        break;
                    case PrintFontType.HeaderFooterText:
                        ctrl.Title = "Header and Footer Text:";
                        break;
                }

                ctrl.Changed += on_FontChanged;
                ctrl.FontInfo = _workingAppSettings.Fonts [i];
                ctrl.TitleWidth = 150;
            }
            #endregion Initialize Fonts

            #region Initialize Header/Footer
            PopulateFieldList (cbHeaderLeft, appSettings.HeaderFieldLeft);
            PopulateFieldList (cbHeaderCenter, appSettings.HeaderFieldCenter);
            PopulateFieldList (cbHeaderRight, appSettings.HeaderFieldRight);
            PopulateFieldList (cbFooterLeft, appSettings.FooterFieldLeft);
            PopulateFieldList (cbFooterCenter, appSettings.FooterFieldCenter);
            PopulateFieldList (cbFooterRight, appSettings.FooterFieldRight);

            PopulateLineTypeList (cbHeaderLine, appSettings.HeaderLineType);
            PopulateLineTypeList (cbFooterLine, appSettings.FooterLineType);

            chkDefaultUseSharps.Active = appSettings.DefaultUseSharps;
            chkShowHeader.Active = appSettings.ShowHeader;
            chkShowFooter.Active = appSettings.ShowFooter;

            EnableHeaderFields(chkShowHeader.Active);
            EnableFooterFields(chkShowFooter.Active);
            #endregion Initialize Header/Footer
        }


        public AppSettings WorkingAppSettings { get { return _workingAppSettings; } }

        public event EventHandler ChordSheetOptionsChanged;

        void on_FontChanged (object sender, EventArgs e)
        {
            FontSelectionGroup ctrl = (FontSelectionGroup)sender;
            _workingAppSettings.Fonts[ctrl.Index] = ctrl.FontInfo.Clone();

            if (ChordSheetOptionsChanged != null)
                ChordSheetOptionsChanged (this, e);
        }

        protected void on_buttonOK_Clicked (object sender, EventArgs e)
        {
            _workingAppSettings.DefaultUseSharps = chkDefaultUseSharps.Active;

            this.Respond (Gtk.ResponseType.Ok);

        }

        protected void on_buttonCancel_Clicked (object sender, EventArgs e)
        {
            this.Respond (Gtk.ResponseType.Cancel);
        }

        protected void on_defaultFolder_SelectionChanged (object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(defaultFolder.Filename))
                _workingAppSettings.DefaultFolder = defaultFolder.Filename;
        }

        protected void PopulateFieldList(Gtk.ComboBox ctrl, PrintFieldType fieldType = PrintFieldType.None)
        {
            ctrl.AppendText ("None");
            ctrl.AppendText ("Title");
            ctrl.AppendText ("Page #");
            ctrl.AppendText ("Page # of #");
            ctrl.AppendText ("Artist");
            ctrl.AppendText ("Copyright");
            ctrl.AppendText ("CCLI");
            ctrl.Active = (int)fieldType;
        }

        protected void PopulateLineTypeList(Gtk.ComboBox ctrl, PrintLineType lineType = PrintLineType.None)
        {
            ctrl.AppendText ("None");
            ctrl.AppendText ("Single");
            ctrl.AppendText ("Double");
            ctrl.AppendText ("Thick");
            ctrl.Active = (int)lineType;
        }

        protected void EnableHeaderFields(bool enabled)
        {
            cbHeaderLeft.Sensitive = enabled;
            cbHeaderCenter.Sensitive = enabled;
            cbHeaderRight.Sensitive = enabled;
            cbHeaderLine.Sensitive = enabled;

            GetHeaderFooterSettings();
            if (ChordSheetOptionsChanged != null)
                ChordSheetOptionsChanged (this, null);            
        }

        protected void EnableFooterFields(bool enabled)
        {
            cbFooterLeft.Sensitive = enabled;
            cbFooterCenter.Sensitive = enabled;
            cbFooterRight.Sensitive = enabled;
            cbFooterLine.Sensitive = enabled;

            GetHeaderFooterSettings();
            if (ChordSheetOptionsChanged != null)
                ChordSheetOptionsChanged (this, null);            
        }


        protected void on_ShowHeaderToggle (object sender, EventArgs e)
        {
            EnableHeaderFields (chkShowHeader.Active);
        }

        protected void on_ShowFooterToggled (object sender, EventArgs e)
        {
            EnableFooterFields (chkShowFooter.Active);
        }

        protected void HeaderFooterChanged(object sender, EventArgs e)
        {
            GetHeaderFooterSettings();

            if (ChordSheetOptionsChanged != null)
                ChordSheetOptionsChanged (this, e);            
        }

        private void GetHeaderFooterSettings()
        {
            _workingAppSettings.ShowHeader = chkShowHeader.Active;
            _workingAppSettings.HeaderFieldLeft = (PrintFieldType)cbHeaderLeft.Active;
            _workingAppSettings.HeaderFieldCenter = (PrintFieldType)cbHeaderCenter.Active;
            _workingAppSettings.HeaderFieldRight = (PrintFieldType)cbHeaderRight.Active;
            _workingAppSettings.HeaderLineType = (PrintLineType)cbHeaderLine.Active;

            _workingAppSettings.ShowFooter = chkShowFooter.Active;
            _workingAppSettings.FooterFieldLeft = (PrintFieldType)cbFooterLeft.Active;
            _workingAppSettings.FooterFieldCenter = (PrintFieldType)cbFooterCenter.Active;
            _workingAppSettings.FooterFieldRight = (PrintFieldType)cbFooterRight.Active;
            _workingAppSettings.FooterLineType = (PrintLineType)cbFooterLine.Active;


        }
    }
}

