using System;
using CSGen.Operations;
using Gtk;
using Cairo;

namespace CSGen.Model
{
	public class TextNode : PrintNode
	{
		private double _underlineOffset = 0.0;
		private FontInfo _fontInfo;

		public TextNode (string text) : base(PrintNodeType.Text)
		{
			Text = text;
		}

		public TextNode (double top, double left, FontInfo font, string text) : base(PrintNodeType.Text)
		{
			Font = font;
			Text = text;
			Bottom = top;
			Left = left;
		}


		public FontInfo Font
		{ 
			get
			{ 
				return _fontInfo;
			}
			set
			{
				_fontInfo = value;

				if (_fontInfo.Underline)
					_underlineOffset = _fontInfo.UnderlineHeight + 2.0;
				else
					_underlineOffset = 0.0;
			}
		}


		public string Text { get; set; }
		public double ULOffset { get { return _underlineOffset; } }

		// Used for placement logic but not by the actual print process.
		public double _Width { get; set; }
		public double _Height { get; set; }

		public override void Render (Cairo.Context cr)
		{
			base.Render (cr);
			double ulOffset = 0.0;
			double underlineHeight = 0.0;
			if (Font.Underline == true)
			{
				underlineHeight = Font.UnderlineHeight;
				ulOffset = underlineHeight + 2.0;
			}


			cr.SetFont (Font);
			cr.MoveTo (Left, Bottom - ulOffset);

			string text = Text;
			//if (text.IndexOf ("{0}") >= 0)
			//	text = string.Format (text, index + 1, _pages.Count);
			cr.ShowText (text);

			if (Font.Underline == true)
			{
				TextExtents extents = cr.TextExtents (Text);

				cr.Rectangle (Left
				               , Bottom - underlineHeight
				               , extents.Width
				               , underlineHeight);

				cr.Fill ();

			}
		}
	}
}

