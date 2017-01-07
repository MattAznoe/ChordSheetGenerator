using System;
using Cairo;

namespace CSGen.Model
{
	public class PrintNode
	{
		public PrintNode(PrintNodeType nodeType)
		{
			NodeType = nodeType;
		}

		public virtual void Render(Context cr)
		{
		}

		public double Bottom { get; set; }
		public double Left { get; set; }
		public PrintNodeType NodeType { get; set; }

	}
}

