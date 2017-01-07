using System;

namespace CSGen
{
    [Serializable]
    public class RGBA
    {
        #region Private Members
        private double _red;
        private double _green;
        private double _blue;
        private double _alpha;

        private byte _bRed;
        private byte _bGreen;
        private byte _bBlue;
        #endregion

        #region Constructor
        public RGBA()
        {
            _alpha = 1.0;
        }


        public RGBA(byte red, byte green, byte blue)
        {
            _bRed = red;
            _bGreen = green;
            _bBlue = blue;

            _red = (double)red / 255.0;
            _green = (double)green / 255.0;
            _blue = (double)blue / 255.0;

            _alpha = 1.0;
        }

        public RGBA(double red, double green, double blue, double alpha = 1.0)
        {
            _red = red;
            _green = green;
            _blue = blue;
            _alpha = alpha;
        }

        public RGBA(Gdk.Color color)
        {
            SetGdkColor (color);
        }
        #endregion

        public double Red { get { return _red; } }
        public double Green { get { return _green; } }
        public double Blue { get { return _blue; } }
        public double Alpha { get { return _alpha; } }

        public byte RGBRed { get { return _bRed; } }
        public byte RGBGreen { get { return _bGreen; } }
        public byte RGBBlue { get { return _bBlue; } }

        public void SetGdkColor(Gdk.Color color)
        {
            _red = (double)color.Red / (double)ushort.MaxValue;
            _green = (double)color.Green / (double)ushort.MaxValue;
            _blue = (double)color.Blue / (double)ushort.MaxValue;

            _bRed = Convert.ToByte(Math.Round(Red * 255.0));
            _bGreen = Convert.ToByte(Math.Round(Green * 255.0));
            _bBlue = Convert.ToByte(Math.Round(Blue * 255.0));
        }

        public void SetRGBColor(ushort red, ushort green, ushort blue)
        {
            _red = (double)red / (double)ushort.MaxValue;
            _green = (double)green / (double)ushort.MaxValue;
            _blue = (double)blue / (double)ushort.MaxValue;

            _bRed = Convert.ToByte(Math.Round(Red * 255.0));
            _bGreen = Convert.ToByte(Math.Round(Green * 255.0));
            _bBlue = Convert.ToByte(Math.Round(Blue * 255.0));

        }

        public Gdk.Color GDKColor
        {
            get
            {
                return new Gdk.Color (_bRed, _bGreen, _bBlue);
            }
        }

        public RGBA Clone()
        {
            return (RGBA)this.MemberwiseClone ();
        }
    }

}

