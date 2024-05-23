using ICWebApp.Application.Interface;
using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.DataStore.MSSQL.Repositories;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;

namespace ICWebApp.Application.Helper
{
    public class RoomGalerieHelper : IRoomGalerieHelper
    {
        private IUnitOfWork _unitOfWork;
        private NavigationManager NavManager;
        private string BasePath = "D:/Comunix";

        public RoomGalerieHelper(NavigationManager NavManager, IUnitOfWork _unitOfWork)
        {
            this.NavManager = NavManager;
            this._unitOfWork = _unitOfWork;
        }
        public string GetImagePath(Guid ROOM_RoomGalerie_ID)
        {
            var item = _unitOfWork.Repository<ROOM_RoomGalerie>().FirstOrDefault(a => a.ID == ROOM_RoomGalerie_ID);

            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }

            if (item != null)
            {
                if (item.ServerPath != null) 
                {
                    var path = BasePath + item.ServerPath;

                    if (!File.Exists(path))
                    {
                        if (item.FILE_FileInfo != null)
                        {
                            var file = _unitOfWork.Repository<FILE_FileStorage>().FirstOrDefault(p => p.FILE_FileInfo_ID == item.FILE_FileInfo.Value);

                            if (file != null && file.FileImage != null) 
                            {
                                if (file.FILE_FileInfo == null || file.FILE_FileInfo.FileExtension.Contains(".jpg"))
{

                                    Stream ms = new MemoryStream(file.FileImage);

                                    var jpgImage = System.Drawing.Image.FromStream(ms);
                                    if (jpgImage.Height > 1080 || jpgImage.Width > 1920)
                                    {
                                        jpgImage = ScaleImage(jpgImage, 1920, 1080);
                                    }

                                    SaveJpeg(path, jpgImage, 50);

                                }
                                else
                                {
                                    File.WriteAllBytes(path, file.FileImage);
                                }
                   
                            } 
                        }
                    }

                    return item.ServerPath;
                }
            }

            return "";
        }
        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }
        public static void SaveJpeg(string path, System.Drawing.Image img, int quality)
        {
            if (quality < 0 || quality > 100)
                throw new ArgumentOutOfRangeException("quality must be between 0 and 100.");
            EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;


            img.Save(path, jpegCodec, encoderParams);

        }
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];

            return null;
        }
    }
}
