using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace CSGen.Model
{
	public class SongData
	{
		#region Private Members
		/// <summary>
		/// Collection of other tags that were discovered in the file but are not supported.  These will be kept entact.
		/// </summary>
		private NameValueCollection _miscTags = new NameValueCollection();
		private bool _modified = false;

		private string _title = string.Empty;
		private string _copyright = string.Empty;
		private string _subtitle = string.Empty;
		private string _artist = string.Empty;
		private string _ccli = string.Empty;
		private string _key = string.Empty;
		private string _capo = string.Empty;
		private string _content = string.Empty;

		private string _titleOrig = string.Empty;
		private string _copyrightOrig = string.Empty;
		private string _subtitleOrig = string.Empty;
		private string _artistOrig = string.Empty;
		private string _ccliOrig = string.Empty;
		private string _keyOrig = string.Empty;
		private string _capoOrig = string.Empty;
		private string _contentOrig = string.Empty;

		#endregion

		#region Constructor
		public SongData ()
		{
			FilePath = string.Empty;
		}
		#endregion

		public string FilePath { get; set; }

		#region Meta Tags
		// Supported Meta Tags
		public string Title
		{ 
			get { return _title; }
			set
			{
				_title = value;
				CheckModified (_titleOrig, value);
			}
		}
		public string Copyright
		{ 
			get { return _copyright; }
			set
			{
				_copyright = value;
				CheckModified (_copyrightOrig, value);
			}
		}

		public string SubTitle
		{ 
			get { return _subtitle; }
			set
			{
				_subtitle = value;
				CheckModified (_subtitleOrig, value);
			}
		}

		public string Artist
		{ 
			get { return _artist; }
			set
			{
				_artist = value;
				CheckModified (_artistOrig, value);
			}
		}

		public string Key
		{ 
			get { return _key; }
			set
			{
				_key = value;
				CheckModified (_keyOrig, value);
			}
		}

		public string Capo		
		{ 
			get { return _capo; }
			set
			{
				_capo = value;
				CheckModified (_capoOrig, value);
			}
		}

		public string CCLI		
		{ 
			get { return _ccli; }
			set
			{
				_ccli = value;
				CheckModified (_ccliOrig, value);
			}
		}

		public NameValueCollection MiscTags { get { return this._miscTags; } }
		#endregion

		public string ContentText 
		{ 
			get { return _content; }
			set
			{
				_content = value;
				CheckModified (_contentOrig, value);
			}
		}

		public bool HasData
		{
			get
			{
				return !string.IsNullOrWhiteSpace (_title)
					|| !string.IsNullOrWhiteSpace (_content);
			}
		}

		public bool IsModified{ get { return _modified; } }

		/// <summary>
		///  Clear the modified flag and reset the original values.
		/// </summary>
		public void ClearModified()
		{
			_modified = false;

			_titleOrig = _title;
			_subtitleOrig = _subtitle;
			_artistOrig = _artist;
			_copyrightOrig = _copyright;
			_ccliOrig = _ccli;
			_keyOrig = _key;
			_capoOrig = _capo;
			_contentOrig = _content;

		}


		#region Private Methods
		private void CheckModified(string orig, string newval)
		{
			if (string.Compare (orig, newval) != 0)
			{
				_modified = true;
			} 
			else 
			{
				CheckModifiedAll ();
			}		
		}


		private void CheckModifiedAll()
		{
			_modified = false;
			_modified = _modified || string.Compare (_title, _titleOrig) != 0;
			_modified = _modified || string.Compare (_copyright, _copyrightOrig) != 0;
			_modified = _modified || string.Compare (_subtitle, _subtitleOrig) != 0;
			_modified = _modified || string.Compare (_artist, _artistOrig) != 0;
			_modified = _modified || string.Compare (_copyright, _copyrightOrig) != 0;
			_modified = _modified || string.Compare (_ccli, _ccliOrig) != 0;
			_modified = _modified || string.Compare (_key, _keyOrig) != 0;
			_modified = _modified || string.Compare (_capo, _capoOrig) != 0;
			_modified = _modified || string.Compare (_content, _contentOrig) != 0;
		}
		#endregion

	}
}

