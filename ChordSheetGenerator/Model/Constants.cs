using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CSGen.Model
{
    public static class Constants
    {
        public static Regex _reValueTag = new Regex (@"{([A-Za-z]+):([^}]*)}");
        public static Regex _reChord = new Regex(Regex.Escape("[") + "([A-G][#|b]?)([0-9]?(maj|dim|sus|m)?[0-9]?)(/([A-G][#|b]?))?" + Regex.Escape("]"));
        public static Regex _reSectionTags = new Regex(@"{(sob|eob|soc|eoc|sot|eot|new_page|np|npp|start_of_bridge|end_of_bridge|start_of_chorus|end_of_chorus|start_of_tab|end_of_tab|start_of_verse|end_of_verse|start_of_versealt|end_of_versealt)}");


        public static string[] _chordProgressionSharp = new string[] { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
        public static string[] _chordProgressionFlat = new string[] { "A", "Bb", "B", "C", "Db", "D", "Eb", "E", "F", "Gb", "G", "Ab" };

        public const int _printFontTypeCount = 6;
        public const int _printRegionTypeCount = 7;

        public static int X_DISPLAY_MARGIN = 10;
        public static int Y_DISPLAY_MARGIN = 10;
        public static int SHADOW_WIDTH = 4; // included in the margin.

    }
}

