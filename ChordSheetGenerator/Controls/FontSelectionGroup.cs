using System;
using Gtk;
using Cairo;
using CSGen.Model;

namespace CSGen.Controls
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FontSelectionGroup : Gtk.Bin
	{
		private bool _updateInProgress = false;

		public FontSelectionGroup ()
		{
			this.Build ();
		}

		// Used to help identify the control from a list.
		public int Index { get; set; }

		public int TitleWidth
		{
			get { return titleLabel.WidthRequest; }
			set { titleLabel.WidthRequest = value; }
		}

		public string Title
		{
			get
			{
				return titleLabel.LabelProp;
			}
			set
			{
				titleLabel.LabelProp = value;
			}
		}


		public FontInfo FontInfo
		{
			get
			{ 
				FontInfo info = new FontInfo ();

				Pango.FontDescription fontdesc = 
					Pango.FontDescription.FromString(fontButton.FontName);
				info.Family = fontdesc.Family;
				if (fontdesc.Weight == Pango.Weight.Bold)
					info.Weight = FontWeight.Bold;
				else
					info.Weight = FontWeight.Normal;
				switch (fontdesc.Style)
				{
					case Pango.Style.Italic:
						info.Slant = FontSlant.Italic;
						break;
					case Pango.Style.Normal:
						info.Slant = FontSlant.Normal;
						break;
					case Pango.Style.Oblique:
						info.Slant = FontSlant.Oblique;
						break;
				}

				info.Scale = (double)fontdesc.Size / 1000.0;
				info.LineSpacing = spacingSpin.Value;
				info.Color = new RGBA(colorButton.Color);
				info.Underline = cb_Underline.Active;

				return info;
			}
			set
			{
				_updateInProgress = true;
				Pango.FontDescription fontdesc = new Pango.FontDescription ();
				fontdesc.Family = value.Family;
				double val = Math.Round(value.Scale * 1000.0);
				fontdesc.Size = (int)val;
				if (value.Weight == FontWeight.Bold)
					fontdesc.Weight = Pango.Weight.Bold;
				else 
					fontdesc.Weight = Pango.Weight.Normal;

				switch (value.Slant)
				{
					case FontSlant.Italic:
						fontdesc.Style = Pango.Style.Italic;
						break;
					case FontSlant.Normal:
						fontdesc.Style = Pango.Style.Normal;
						break;
					case FontSlant.Oblique:
						fontdesc.Style = Pango.Style.Oblique;
						break;
				}

				fontButton.FontName = fontdesc.ToString ();

				spacingSpin.Value = value.LineSpacing;
				colorButton.Color = value.Color.GDKColor;
				cb_Underline.Active = value.Underline;
				_updateInProgress = false;
			}
		}

		public event EventHandler Changed;

		protected void on_Change (object sender, EventArgs e)
		{
			if (Changed != null && !_updateInProgress)
				Changed (this, e);
		}


	}
}

