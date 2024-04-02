using System.Drawing.Imaging;

namespace EasyConvertPro.Models
{
    public class ImageFormatModel
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public ImageFormat ImgExtension { get; set; }
        public string FromImgExtension { get; set; }
    }
}
