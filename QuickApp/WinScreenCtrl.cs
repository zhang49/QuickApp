using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickApp
{
    public partial class WinScreenCtrl : UserControl
    {
        public enum DisplayType
        {
            //平铺
            Tile,
            //填满
            Fill,
            //从Image右下角平铺
            TileBottomRight
        };
        private DisplayType mDisplayType = DisplayType.Tile;
        private Image mImage;

        public Image Image
        {
            get => mImage; set
            {
                mImage?.Dispose();
                if (value != null)
                {
                    mImage = value.Clone() as Image;
                }
                else
                {
                    mImage = null;
                }
                this.Invalidate();
                this.Update();
            }
        }

        [Browsable(false)]
        public DisplayType ImgDisplayType { get => mDisplayType; set => mDisplayType = value; }

        public WinScreenCtrl()
        {
            SetStyle();
            InitializeComponent();
        }

        private void SetStyle()
        {
            base.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
            base.UpdateStyles();
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            var graphicc = e.Graphics;
            if (Image != null)
            {
                switch (ImgDisplayType)
                {
                    case DisplayType.Tile:
                        graphicc.DrawImage(Image, this.ClientRectangle,
                        new Rectangle(0, 0, Width, Height),
                       GraphicsUnit.Pixel);
                        break;
                    case DisplayType.Fill:
                        graphicc.DrawImage(Image, new Rectangle(0, 0, this.Width, this.Height),
                            new Rectangle(0, 0, Image.Width, Image.Height),
                           GraphicsUnit.Pixel);
                        break;
                    case DisplayType.TileBottomRight:
                        graphicc.DrawImage(Image, this.ClientRectangle,
                        new Rectangle(Image.Width - Width, Image.Height - Height, Width, Height),
                       GraphicsUnit.Pixel);
                        break;
                    default:
                        break;
                }

            }
            else
            {
                graphicc.DrawString("NoneImage", this.Font, Brushes.Gray, this.ClientRectangle);
            }
        }
    }
}
