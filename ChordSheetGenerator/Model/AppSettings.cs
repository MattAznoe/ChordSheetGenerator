using System;
using CSGen.Model;

namespace CSGen.Model
{
	[Serializable]
	public class AppSettings
	{
		public AppSettings ()
		{
		}

		public string DefaultFolder { get; set; }

		#region Header/Footer Settings
		public bool ShowHeader { get; set; }
		public PrintFieldType HeaderFieldLeft { get; set; }
		public PrintFieldType HeaderFieldCenter { get; set; }
		public PrintFieldType HeaderFieldRight { get; set; }
		public PrintLineType HeaderLineType { get; set; }

		public bool ShowFooter { get; set; }
		public PrintFieldType FooterFieldLeft { get; set; }
		public PrintFieldType FooterFieldCenter { get; set; }
		public PrintFieldType FooterFieldRight { get; set; }
		public PrintLineType FooterLineType { get; set; }
		#endregion

	}

}


