using System;

namespace CSGen.Model	
{
	public class LineNode : PrintNode
	{
		public LineNode (double top, double left, double width, double thickness) : base(PrintNodeType.Line) 
		{
			Bottom = top;
			Left = left;
			LineWidth = width;
			LineThickness = thickness;
		}

		public double LineWidth { get; set; }
		public double LineThickness { get; set; }

		public override void Render (Cairo.Context cr)
		{
			base.Render (cr);

			cr.Rectangle (Left
			              , Bottom - LineThickness
			              , LineWidth
			              , LineThickness);
			cr.Fill ();

		}
	}
}

