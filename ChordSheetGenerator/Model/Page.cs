using System;
using System.Collections.Generic;

namespace CSGen.Model
{
	public class Page
	{
		private List<PrintNode> _nodes = new List<PrintNode>();

		public Page (int pageNumber)
		{
			PageNumber = pageNumber;
		}

		public int PageNumber { get; set; }

		public List<PrintNode> Nodes { get { return _nodes; } }

	}
}

