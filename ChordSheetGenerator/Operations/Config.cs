using System;
using System.IO;
using Nini.Config;
using CSGen.Model;

namespace CSGen.Operations
{
	public sealed class Config
	{
		#region Private members
		private string _iniPath; 
		private IConfigSource _source = null;
		private IConfig _uiSettings;
		private IConfig _fontsAndBackgrounds;
		private IConfig _tagChordBarSettings;
		#endregion

		#region Constructor
		public Config ()
		{
			string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			_iniPath = string.Format ("{0}{1}{2}", folder, System.IO.Path.DirectorySeparatorChar, "CSGen.ini");

			// Make sure that the ini file exists.
			if (!File.Exists (_iniPath))
				File.Create (_iniPath);

			Refresh ();
		}
		#endregion

		#region Public Methods

		public void Refresh()
		{
			_source = new IniConfigSource (_iniPath);
			_source.Alias.AddAlias ("On", true);
			_source.Alias.AddAlias ("Off", false);

			// Get or Add all configuration sections.
			_uiSettings = _source.Configs.GetOrAdd ("UI Settings");
			_fontsAndBackgrounds = _source.Configs.GetOrAdd ("Fonts and Backgrounds");
			_tagChordBarSettings = _source.Configs.GetOrAdd ("Tag/Chord Bar Settings");
		}

		public void Save()
		{
			_source.Save ();
		}
		#endregion

		#region Public Settings

		#region UI Settings
		/// <summary>
		/// 	Determines if the song information should display on startup.
		/// </summary>
		/// <value><c>true</c> if show song info; otherwise, <c>false</c>.</value>
		public bool ShowSongInfo
		{
			get { return _uiSettings.GetBoolean ("ShowSongInfo", true);	}
			set { _uiSettings.Set("ShowSongInfo", value); }
		}

		public bool ShowTagChordBar
		{
			get { return _uiSettings.GetBoolean ("ShowTagChordBar", true);	}
			set { _uiSettings.Set("ShowTagChordBar", value); }
		}


		public string DefaultFolder
		{
			get { return _uiSettings.GetString ("DefaultFolder", ""); }
			set { _uiSettings.Set ("DefaultFolder", value); }
		}

		public bool ShowHeader
		{
			get { return _uiSettings.GetBoolean ("ShowHeader", false); }
			set { _uiSettings.Set ("ShowHeader", value); }
		}

		public int HeaderFieldLeft
		{
			get { return _uiSettings.GetInt ("HeaderFieldLeft", 0); }
			set { _uiSettings.Set ("HeaderFieldLeft", value); }
		}

		public int HeaderFieldCenter
		{
			get { return _uiSettings.GetInt ("HeaderFieldCenter", 0); }
			set { _uiSettings.Set ("HeaderFieldCenter", value); }
		}

		public int HeaderFieldRight
		{
			get { return _uiSettings.GetInt ("HeaderFieldRight", 0); }
			set { _uiSettings.Set ("HeaderFieldRight", value); }
		}

		public int HeaderLineType
		{
			get { return _uiSettings.GetInt ("HeaderLineType", 0); }
			set { _uiSettings.Set ("HeaderLineType", value); }
		}

		public bool ShowFooter
		{
			get { return _uiSettings.GetBoolean ("ShowFooter", false); }
			set { _uiSettings.Set ("ShowFooter", value); }
		}

		public int FooterFieldLeft
		{
			get { return _uiSettings.GetInt ("FooterFieldLeft", 0); }
			set { _uiSettings.Set ("FooterFieldLeft", value); }
		}

		public int FooterFieldCenter
		{
			get { return _uiSettings.GetInt ("FooterFieldCenter", 0); }
			set { _uiSettings.Set ("FooterFieldCenter", value); }
		}

		public int FooterFieldRight
		{
			get { return _uiSettings.GetInt ("FooterFieldRight", 0); }
			set { _uiSettings.Set ("FooterFieldRight", value); }
		}

		public int FooterLineType
		{
			get { return _uiSettings.GetInt ("FooterLineType", 0); }
			set { _uiSettings.Set ("FooterLineType", value); }
		}

		#endregion

		#region Fonts and Backgrounds

		#region Default Values
		private static string[] DefaultFonts = new string[] 
			{"Sans,Normal,Normal,12.288,1.5,0|0|0"
			, "Sans,Normal,Normal,12.288,1.5,0|0|0"
			, "Sans,Normal,Normal,12.288,1.5,0|0|0"
			, "Sans,Normal,Normal,12.288,1.5,0|0|0"
			, "Sans,Normal,Normal,12.288,1.5,0|0|0"
			, "Sans,Normal,Normal,12.288,1.5,0|0|0"};
		#endregion

		public FontInfo GetFont(PrintFontType fontType)
		{
			string def = (int)fontType < DefaultFonts.Length ? DefaultFonts [(int)fontType] : "";
			return new FontInfo(_fontsAndBackgrounds.Get (fontType.ToString(), def));
		}

		public void SetFont(FontInfo info, PrintFontType fontType)
		{
			string val = info.Serialize ();
			_fontsAndBackgrounds.Set (fontType.ToString (), val);
		}

		#endregion

		#region Tag/Chord Bar

		#region Default Values
		private static string[] DefaultChordOptions = new string[] 
			{"[A]", "[Am]", "[Bm]", "[C]", "[D]", "[Dm]", "[E]", "[Em]", "[F]", "[G]"};

		private static string[] DefaultTagOptions = new string[] 
			{"Comment,{c:}"
			,"Intro,{c:Intro}"
			,"Verse,{c:Verse}"
			,"Chorus,{c:Chorus}"
			,"Bridge,{c:Bridge}"
			,"Chorus Block,{start_of_chorus}\n\n{end_of_chorus}"
			,"Bridge Block,{start_of_bridge}\n\n{end_of_bridge}"
			,"Tab,{start_of_tab}\n\n{end_of_tab}"
			,"New Page,{new_page}\n"};

		private static string[] DefaultTagKeyMapping = new string[]
			{"1", "2", "3", "4", "5", "6", "7", "8", "9", "0"};
		#endregion

		public int NumberOfBarOptions
		{
			get { return _tagChordBarSettings.GetInt ("NumberOfBarOptions", 15); }
			set { _tagChordBarSettings.Set ("NumberOfBarOptions", value); }
		}

		public string GetTagBarOption(int index)
		{
			string def = index < DefaultTagOptions.Length ? DefaultTagOptions [index] : "";
			return _tagChordBarSettings.Get (string.Format("TabBarOption{0}", (index+1)), def);
		}

		public void SetTagBarOption(int index, string value)
		{
			_tagChordBarSettings.Set (string.Format ("TabBarOption{0}", (index + 1)), value);
		}

		public string GetTagBarKeyMap(int index)
		{
			string def = index < DefaultTagKeyMapping.Length ? DefaultTagKeyMapping [index] : "";
			return _tagChordBarSettings.Get (string.Format("TabBarKeyMap{0}", (index+1)), def);
		}

		public void SetTagBarKeyMap(int index, string value)
		{
			_tagChordBarSettings.Set (string.Format ("TabBarKeyMap{0}", (index + 1)), value);
		}

		public string GetChordBarOption(int index)
		{
			string def = index < DefaultChordOptions.Length ? DefaultChordOptions [index] : "";
			return _tagChordBarSettings.Get (string.Format("ChordBarOption{0}", (index+1)), def);
		}

		public void SetChordBarOption(int index, string value)
		{
			_tagChordBarSettings.Set (string.Format ("ChordBarOption{0}", (index + 1)), value);
		}

		public string GetChordBarKeyMap(int index)
		{
			string def = index < DefaultTagKeyMapping.Length ? DefaultTagKeyMapping [index] : "";
			return _tagChordBarSettings.Get (string.Format("ChordBarKeyMap{0}", (index+1)), def);
		}

		public void SetChordBarKeyMap(int index, string value)
		{
			_tagChordBarSettings.Set (string.Format ("ChordBarKeyMap{0}", (index + 1)), value);
		}

		#endregion Chord Bar


		#endregion

	}

	public static class ConfigExtension
	{
		public static IConfig GetOrAdd( this ConfigCollection cfg, string name)
		{
			if (cfg [name] != null)
				return cfg [name];
			return cfg.Add (name);
		}


	}
}

