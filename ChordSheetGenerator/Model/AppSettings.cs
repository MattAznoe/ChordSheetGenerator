using System;
using CSGen.Model;

namespace CSGen.Model
{
    [Serializable]
    public class AppSettings
    {
        private FontInfo[] _fonts = new FontInfo[Constants._printFontTypeCount];
        private RegionSettings[] _regions = new RegionSettings[Constants._printRegionTypeCount];

        public AppSettings ()
        {
            // TODO: Move to configuration
            for (int i = 0; i < _regions.Length; i++)
                _regions[i] = new RegionSettings();

            _regions[(int)PrintRegionType.ChorusBlock].Indent = 10;
            _regions[(int)PrintRegionType.ChorusBlock].HasBackground = true;
            _regions[(int)PrintRegionType.ChorusBlock].Background.SetRGBColor(53250, 53250, 53250);
            _regions[(int)PrintRegionType.ChorusBlock].PaddingBottom = 10;
            _regions[(int)PrintRegionType.ChorusBlock].PaddingTop = 10;
            _regions[(int)PrintRegionType.ChorusBlock].PaddingLeft = 10;
            _regions[(int)PrintRegionType.ChorusBlock].PaddingRight = 10;

            _regions[(int)PrintRegionType.VerseAltBlock].HasBackground = true;
            _regions[(int)PrintRegionType.VerseAltBlock].Background.SetRGBColor(60250, 60250, 60250);
            _regions[(int)PrintRegionType.VerseAltBlock].PaddingBottom = 5;
            _regions[(int)PrintRegionType.VerseAltBlock].PaddingTop = 5;
            _regions[(int)PrintRegionType.VerseAltBlock].PaddingLeft = 0;
            _regions[(int)PrintRegionType.VerseAltBlock].PaddingRight = 5;

            // TODO: Move to configuration
        }

        public string DefaultFolder { get; set; }
        public bool DefaultUseSharps { get; set; }

        public FontInfo[] Fonts
        {
            get
            {
                return _fonts;
            }
        }

        public FontInfo GetFont(PrintFontType fontType)
        {
            return _fonts[(int)fontType];
        }


        public RegionSettings[] RegionSettings
        {
            get
            {
                return _regions;
            }
        }

        public RegionSettings GetRegionSetting(PrintRegionType regionType)
        {
            return _regions[(int)regionType];
        }


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


