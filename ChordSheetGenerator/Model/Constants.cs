using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CSGen.Model
{
	public static class Constants
	{
		public static Regex _reValueTag = new Regex (@"{([A-Za-z]+):([^}]*)}");
		public static Regex _reChord = new Regex(Regex.Escape("[") + "([A-G][#|b]?)((maj|dim|sus|m)?[0-9]?)(/([A-G][#|b]?))?" + Regex.Escape("]"));
		public static Regex _reSectionTags = new Regex(@"{(sob|eob|soc|eoc|sot|eot|new_page|np|npp|start_of_bridge|end_of_bridge|start_of_chorus|end_of_chorus|start_of_tab|end_of_tab)}");


		public static string[] _chordProgressionSharp = new string[] { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
		public static string[] _chordProgressionFlat = new string[] { "A", "Bb", "B", "C", "Db", "D", "Eb", "E", "F", "Gb", "G", "Ab" };

		public const int _fontCount = 6;
	}
}

