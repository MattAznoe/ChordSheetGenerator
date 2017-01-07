using System;

namespace CSGen.Model
{
    [Serializable]
    public class RegionSettings
    {
        private RGBA _background = new RGBA();

        public RegionSettings()
        {
        }

        public bool HasBackground { get; set; }
        public RGBA Background 
        {
            get
            {
                return _background;
            }
        } 


        public int Indent { get; set; }
        public int PaddingTop { get; set; }
        public int PaddingRight { get; set; }
        public int PaddingBottom { get; set; }
        public int PaddingLeft { get; set; }
    }
}

