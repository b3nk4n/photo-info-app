using Microsoft.Phone;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ImageInfoTool.App.ViewModels
{
    class ImageViewModel
    {
        private Picture _image;

        public ImageViewModel()
        {

        }

        public ImageViewModel(Picture image)
        {
            _image = image;
        }

        public ImageSource ThumbnailImage
        {
            get
            {
                return PictureDecoder.DecodeJpeg(_image.GetThumbnail());
            }
        }

        public ImageSource Image
        {
            get
            {
                return PictureDecoder.DecodeJpeg(_image.GetImage());
            }
        }
    }
}
