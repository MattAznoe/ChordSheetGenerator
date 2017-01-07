using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using CSGen.Model;


namespace CSGen.Operations
{
    public static class FileOperations
    {
        public static SongData OpenSongFile(string path)
        {
            //string[] data = System.IO.File.ReadAllLines (path, Encoding.UTF7);
            string[] data = System.IO.File.ReadAllLines(path, GetEncoding(path));
            SongData song = new SongData();

            song.FilePath = path;

            bool inMetaSection = true;
            StringBuilder editableText = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                if (inMetaSection)
                {
                    // Only process tags if they are at the start of the file.  As soon as we have a line that does not contain
                    // any tags, we will simply dump the text to the editable area.
                    if (!ParseTag(data[i], song))
                        inMetaSection = false;
                }

                if (!inMetaSection)
                {
                    editableText.AppendLine(data[i]);
                }

            }

            if (song.SubTitle.Length > 0 && song.Artist.Length == 0)
                song.Artist = song.SubTitle;

            song.ContentText = editableText.ToString();

            song.ClearModified();

            return song;

        }


        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to ASCII when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="filename">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        public static Encoding GetEncoding(string filename)
        {
            // Read the BOM
            var bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76)
                return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
                return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe)
                return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff)
                return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff)
                return Encoding.UTF32;
            return Encoding.Default;
        }

        public static void SaveSongFile(SongData song)
        {
            StringBuilder data = new StringBuilder();

            // Add our supported tags first.
            data.AppendLine(BuildTag("title", song.Title));
            data.AppendLine(BuildTag("subtitle", song.SubTitle));
            data.AppendLine(BuildTag("artist", song.Artist));
            data.AppendLine(BuildTag("copyright", song.Copyright));
            data.AppendLine(BuildTag("ccli", song.CCLI));
            data.AppendLine(BuildTag("key", song.Key));
            data.AppendLine(BuildTag("capo", song.Capo));

            foreach (string key in song.MiscTags.AllKeys)
            {
                data.AppendLine(BuildTag(key, song.MiscTags[key]));
            }

            data.Append(song.ContentText);

            System.IO.File.WriteAllText(song.FilePath, data.ToString(), Encoding.UTF8);

            song.ClearModified();

        }


        public static string BuildTag(string tagName, string tagValue)
        {
            return string.Format("{{{0}:{1}}}", tagName, tagValue);
        }



        public static bool ParseTag(string dataLine, SongData song)
        {
            bool retval = false;
            MatchCollection mc = Constants._reValueTag.Matches(dataLine);
            if (mc.Count > 0)
            {
                retval = true;
                foreach (Match m in mc)
                {
                    string tagName = m.Groups[1].Value.ToLower();
                    string tagValue = m.Groups[2].Value;

                    switch (tagName)
                    {
                        case "title":
                        case "t":
                            song.Title = tagValue;
                            break;

                        case "subtitle":
                        case "st":
                        case "su":
                            song.SubTitle = tagValue;
                            break;

                        case "artist":
                        case "a":
                            song.Artist = tagValue;
                            break;

                        case "key":
                        case "k":
                            song.Key = tagValue;
                            break;

                        case "capo":
                        case "ca":
                            song.Capo = tagValue;
                            break;

                        case "copyright":
                        case "cr":
                            song.Copyright = tagValue;
                            break;

                        case "ccli":
                            song.CCLI = tagValue;
                            break;


                        default:
                            song.MiscTags.Add(tagName, tagValue);
                            break;
                    }
                }
            }

            return retval;
        }



        public static string Transpose(string text, TransposeDirection direction, int steps, bool useSharps)
        {
            // Get all of the chords in the song.
            MatchCollection mc = Constants._reChord.Matches(text);

            if (useSharps)
                tx_Progression = Constants._chordProgressionSharp;
            else
                tx_Progression = Constants._chordProgressionFlat;

            tx_increment = direction == TransposeDirection.Down ? -steps : steps;

            return Constants._reChord.Replace(text, new MatchEvaluator(FileOperations.ReplaceChord));
        }

        private static string[] tx_Progression;
        private static int tx_increment;


        private static string ReplaceChord(Match m)
        {
            string mainChord = GetNewChord(m.Groups[1].ToString());
            string bassChord = GetNewChord(m.Groups[5].ToString());

            string newChord = string.Format("[{0}{1}{2}{3}]", mainChord, m.Groups[2].ToString(), bassChord.Length > 0 ? "/" : "", bassChord);
            return newChord;
        }


        private static string GetNewChord(string startChord)
        {
            string newChord = string.Empty;
            if (!string.IsNullOrEmpty(startChord))
            {
                if (tx_increment == 0)
                {
                    // Make sure all of the chords are using #'s or b's appropriately.
                    string[] altProgression;

                    if (tx_Progression == Constants._chordProgressionSharp)
                        altProgression = Constants._chordProgressionFlat;
                    else
                        altProgression = Constants._chordProgressionSharp;

                    int idx = altProgression.IndexOf(startChord);
                    if (idx >= 0)
                        newChord = tx_Progression[idx];
                    else
                        newChord = startChord;
                }
                else
                {
                    // Perform an actual transposition up or down.
                    int idx = tx_Progression.IndexOf(startChord);

                    idx += tx_increment; 
                    if (idx < 0)
                        idx += tx_Progression.Length;
                    if (idx >= tx_Progression.Length)
                        idx -= tx_Progression.Length;

                    newChord = tx_Progression[idx];
                }
            }
            return newChord;
        }



        //      private static string ChangeChord(Match m)
        //      {
        //          int idx = progression.IndexOf (m.Groups [1].ToString());
        //
        //          idx += increment;
        //          if (idx < 0)
        //              idx += progression.Length;
        //          if (idx > progression.Length)
        //              idx -= progression.Length;
        //
        //          string newChord = progression [idx];
        //      }
    }
}

