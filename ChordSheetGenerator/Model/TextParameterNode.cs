using System;

namespace CSGen.Model
{
	public class TextParameterNode : TextNode
	{
		public TextParameterNode (double top, double left, FontInfo font, string text) : base(top, left, font, text)
		{
		}

		public int PageNumber { get; set; }
		public int PageTotal { get; set; }

		public override void Render (Cairo.Context cr)
		{
			// Substitute values
			Text = Text.Replace ("#PageNum", PageNumber.ToString ());
			Text = Text.Replace ("#PageTotal", PageTotal.ToString ());

			base.Render (cr);
		}
	}
}

