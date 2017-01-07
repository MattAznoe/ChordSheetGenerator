using System;
using Cairo;
using Gtk;
using Stetic;

namespace CSGen.Model
{
	public class FontInfo
	{
		#region Constructors
		public FontInfo ()
		{
		}

		public FontInfo (string config)
		{
			Deserialize (config);
		}


		public FontInfo (string family, FontSlant slant, FontWeight weight, double scale, double lineSpacing, bool underline = false)
		{
			Family = family;
			Slant = slant;
			Weight = weight;
			Scale = scale;
			LineSpacing = lineSpacing;
			Color = new RGBA();
			Underline = underline;
		}

	    public FontInfo (string family, FontSlant slant, FontWeight weight, double scale, double lineSpacing, Gdk.Color color, bool underline = false)
		{
			Family = family;
			Slant = slant;
			Weight = weight;
			Scale = scale;
			LineSpacing = lineSpacing;
			Color = new RGBA (color);
			Underline = underline;
		}
		#endregion

		#region Public Properties
		public string Family { get; set; }
		public FontSlant Slant { get; set; }
		public FontWeight Weight { get; set; } 
		public double Scale { get; set; }
		public double LineSpacing { get; set; }
		public RGBA Color { get; set; }
		public bool Underline { get; set; }

		public double UnderlineHeight
		{
			get
			{
				return Scale / 8.0;
			}
		}
		#endregion

		#region Public Methods
		public string Serialize()
		{
			return string.Format ("{0},{1},{2},{3},{4},{5}|{6}|{7},{8}"
			                      , Family, Slant.ToString (), Weight.ToString (), Scale, LineSpacing
			                      , Color.RGBRed, Color.RGBGreen, Color.RGBBlue, Underline);

		}

		public void Deserialize(string input)
		{
			string[] vals = input.Split (',');
			Family = vals [0];

			FontSlant slant;
			if (vals.Length > 1 && Enum.TryParse<FontSlant> (vals [1], out slant))
				Slant = slant;
			else
				Slant = FontSlant.Normal;

			FontWeight weight;
			if (vals.Length > 2 && Enum.TryParse<FontWeight> (vals [2], out weight))
				Weight = weight;
			else
				Weight = FontWeight.Normal;

			double scale;
			if (vals.Length > 3 && double.TryParse (vals [3], out scale))
				Scale = scale;
			else
				Scale = 12.0;

			double lineSpacing;
			if (vals.Length > 4 && double.TryParse (vals [4], out lineSpacing))
				LineSpacing = lineSpacing;
			else
				LineSpacing = Scale / 2;

			Color = new RGBA();
			if (vals.Length > 5)
			{
				string[] colors = vals [5].Split ('|');
				byte red, green, blue;
				if (byte.TryParse (colors [0], out red)
				    && byte.TryParse (colors [1], out green)
				    && byte.TryParse (colors [2], out blue))
				{
					Color = new RGBA(red, green, blue);//red, green, blue);
				}
			}

			if (vals.Length > 6)
			{
				Underline = Convert.ToBoolean (vals [6]);
			}
		}

		public FontInfo Clone()
		{
			FontInfo newInfo = new FontInfo (this.Family, this.Slant, this.Weight, this.Scale, this.LineSpacing, this.Underline);
			newInfo.Color = this.Color.Clone ();

			return newInfo;
		}
		#endregion
	}

}


