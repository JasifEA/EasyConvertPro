using EasyConvertPro.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using static System.Net.WebRequestMethods;

namespace EasyConvertPro.Controllers
{
    public class ImageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult ImageCompressor()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImageCompressor(string txtFileName)
        {
            var files = Request.Form.Files["file"];
            if (files == null)
            {
                ModelState.AddModelError("", "The file not found.");
                return View();
            }

            var fullPath = GetFolderLocation();
            var filePath = CompressImage(fullPath, files);

            if (filePath != null && System.IO.File.Exists(filePath))
            {
                return File(System.IO.File.OpenRead(filePath), "application/octet-stream", Path.GetFileName(filePath));
            }

            return Ok(new {IsSuccess=true, count = 1,ErrorMsg="" });
        }

        private string GetFolderLocation()
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string newFolderName = "EasyConverterPro";
            string fullPath = Path.Combine(folderPath, newFolderName);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            return fullPath;
        }
        private string CompressImage(string fullPath, IFormFile files)
        {
            var filePath = Path.Combine(fullPath, files.FileName);
            using (var stream = files.OpenReadStream())
            using (var image = Image.FromStream(stream))
            {
                if (image.Width > 1920 || image.Height > 1920)
                {
                    var width = image.Width > image.Height ? 1920 : (int)(1920 * ((decimal)image.Width / image.Height));
                    var height = image.Width > image.Height ? (int)(1920 * ((decimal)image.Height / image.Width)) : 1920;

                    using (var bitmap = new Bitmap(image, width, height))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            bitmap.Save(memoryStream, ImageFormat.Jpeg);
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                            using (var fileStream = System.IO.File.Create(filePath))
                            {
                                memoryStream.WriteTo(fileStream);
                            }
                        }
                    }
                }
            }
            return filePath;
        }

        [AllowAnonymous]
        public IActionResult ImageConverter()
        {
            ViewBag.imgFormats = new SelectList(GetImgFormats(), "Id", "Value");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ImageConverter(int Id)
        {
            try
            {
                var files = Request.Form.Files["file"];
                if (files == null)
                {
                    ModelState.AddModelError("", "The file not found.");
                    return View();
                }

                if (Id == null || Id < 1)
                {
                    ModelState.AddModelError("", "Image format missing..!!");
                    return View();
                }

                var fullPath = GetFolderLocation();
                var filePath = Path.Combine(fullPath, files.FileName);

                var imageFormatDetails = GetImgFormats().Where(x => x.Id == Id).FirstOrDefault();

                var fullPathConverted = ConvertingImage(fullPath, filePath, files, imageFormatDetails);//converting image

                if (fullPathConverted != null && System.IO.File.Exists(fullPathConverted))
                {
                    ViewBag.HideLoading = true;
                    return File(System.IO.File.OpenRead(fullPathConverted), "application/octet-stream", Path.GetFileName(fullPathConverted));
                }

            }
            catch (Exception ex) 
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }

            return View();
        }

        public List<ImageFormatModel> GetImgFormats()
        {
            return new List<ImageFormatModel>
            {
                new ImageFormatModel { Id = -1, Value = "-- Select --",ImgExtension=null },
                new ImageFormatModel { Id = 1, Value = "BITMAP" ,ImgExtension=ImageFormat.Bmp},
                new ImageFormatModel { Id = 2, Value = "GIF" , ImgExtension = ImageFormat.Gif},
                new ImageFormatModel { Id = 3, Value = "JPG/JPEG" ,ImgExtension= ImageFormat.Jpeg},
                new ImageFormatModel { Id = 4, Value = "PNG" ,ImgExtension = ImageFormat.Png},
                //new ImageFormatModel { Id = 5, Value = "SVG" },
                new ImageFormatModel { Id = 6, Value = "TIFF" ,ImgExtension = ImageFormat.Tiff}
            };

        }
        private string ConvertingImage(string fullPath, string filePath, IFormFile files, ImageFormatModel imageFormatDetails)
        {
            using (var stream = files.OpenReadStream())
            using (var image = Image.FromStream(stream))
            {
                if (image.Width > 1920 || image.Height > 1920)
                {
                    var width = image.Width > image.Height ? 1920 : (int)(1920 * ((decimal)image.Width / image.Height));
                    var height = image.Width > image.Height ? (int)(1920 * ((decimal)image.Height / image.Width)) : 1920;

                    using (var bitmap = new Bitmap(image, width, height))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            bitmap.Save(memoryStream, imageFormatDetails.ImgExtension);

                            string newName = System.IO.Path.GetFileNameWithoutExtension(files.FileName);
                            string newExtension = imageFormatDetails.Value;
                            string newFileName = $"{newName}.{newExtension}";
                            string fullPathConverted = Path.Combine(fullPath, newFileName);

                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                            using (var fileStream = System.IO.File.Create(fullPathConverted))
                            {
                                memoryStream.WriteTo(fileStream);
                            }
                            memoryStream.Dispose();
                            bitmap.Dispose();

                            return fullPathConverted;
                        }
                    }
                }
            }
            return "";
        }

        [AllowAnonymous]
        public IActionResult ImageUpscale()
        {
            return View();
        }
    }
}
