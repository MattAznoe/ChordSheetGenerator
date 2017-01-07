using System;
using System.Collections.Generic;
using CSGen.Model;
using CSGen.Controls;
using CSGen.Operations;

namespace CSGen
{
	public partial class SettingsDialog : Gtk.Dialog
	{
		private FontInfo[] _workingFonts;
		private List<FontSelectionGroup> _fontControls = new List<FontSelectionGroup>();
		private AppSettings _appSettings;

		public SettingsDialog (AppSettings appSettings, FontInfo[] fonts)
		{
			this.Build ();

			settingsPane.Page = 0;

			if(!string.IsNullOrEmpty(appSettings.DefaultFolder))
				defaultFolder.SetUri (appSettings.DefaultFolder);

			#region Initialize Fonts
			_workingFonts = new FontInfo[fonts.Length];
			for (int i = 0; i < fonts.Length; i++)
			{
				_workingFonts [i] = fonts[i].Clone();
			}

			_appSettings = appSettings.Clone ();

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
				ctrl.FontInfo = _workingFonts [i];
				ctrl.TitleWidth = 150;
			}
			#endregion Initialize Fonts

			#region Initialize Header/Footer
			PopulateFieldList (cbHeaderLeft, appSettings.HeaderFieldLeft);
			PopulateFieldList (cbHeaderCenter, appSettings.HeaderFieldCenter);
			PopulateFieldList (cbHeaderRight, appSettings.HeaderFieldRight);
			PopulateFieldList (cbFooterLeft, appSettings.FooterFieldLeft);
			PopulateFieldList (cbFooterCenter, appSettings.FooterFieldCenter);
			PopulateFieldList (cbFooterRight, appSettings.HeaderFieldRight);

			PopulateLineTypeList (cbHeaderLine, appSettings.HeaderLineType);
			PopulateLineTypeList (cbFooterLine, appSettings.FooterLineType);

			chkShowHeader.Active = appSettings.ShowHeader;
			chkShowFooter.Active = appSettings.ShowFooter;

			EnableHeaderFields(chkShowHeader.Active);
			EnableFooterFields(chkShowFooter.Active);
			#endregion Initialize Header/Footer
		}


		public FontInfo[] WorkingFonts { get { return _workingFonts; } }

		public AppSettings AppSettings { get { return _appSettings; } }

		public event EventHandler ChordSheetOptionsChanged;

		void on_FontChanged (object sender, EventArgs e)
		{
			FontSelectionGroup ctrl = (FontSelectionGroup)sender;
			_workingFonts[ctrl.Index] = ctrl.FontInfo.Clone();

			if (ChordSheetOptionsChanged != null)
				ChordSheetOptionsChanged (this, e);
		}

		protected void on_buttonOK_Clicked (object sender, EventArgs e)
		{
			_appSettings.ShowHeader = chkShowHeader.Active;
			_appSettings.HeaderFieldLeft = (PrintFieldType)cbHeaderLeft.Active;
			_appSettings.HeaderFieldCenter = (PrintFieldType)cbHeaderCenter.Active;
			_appSettings.HeaderFieldRight = (PrintFieldType)cbHeaderRight.Active;
			_appSettings.HeaderLineType = (PrintLineType)cbHeaderLine.Active;

			_appSettings.ShowFooter = chkShowFooter.Active;
			_appSettings.FooterFieldLeft = (PrintFieldType)cbFooterLeft.Active;
			_appSettings.FooterFieldCenter = (PrintFieldType)cbFooterCenter.Active;
			_appSettings.FooterFieldRight = (PrintFieldType)cbFooterRight.Active;
			_appSettings.FooterLineType = (PrintLineType)cbFooterLine.Active;


			this.Respond (Gtk.ResponseType.Ok);

		}

		protected void on_buttonCancel_Clicked (object sender, EventArgs e)
		{
			this.Respond (Gtk.ResponseType.Cancel);
		}

		protected void on_defaultFolder_SelectionChanged (object sender, EventArgs e)
		{
			if(!string.IsNullOrEmpty(defaultFolder.Filename))
				_appSettings.DefaultFolder = defaultFolder.Filename;
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
		}

		protected void EnableFooterFields(bool enabled)
		{
			cbFooterLeft.Sensitive = enabled;
			cbFooterCenter.Sensitive = enabled;
			cbFooterRight.Sensitive = enabled;
			cbFooterLine.Sensitive = enabled;
		}


		protected void on_ShowHeaderToggle (object sender, EventArgs e)
		{
			EnableHeaderFields (chkShowHeader.Active);
		}

		protected void on_ShowFooterToggled (object sender, EventArgs e)
		{
			EnableFooterFields (chkShowFooter.Active);
		}
	}
}

