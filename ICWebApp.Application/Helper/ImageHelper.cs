using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Domain.DBModels;
using Syncfusion.PdfToImageConverter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Helper
{
    public class ImageHelper : IImageHelper
    {
        private IFILEProvider _fileProvider;
        public ImageHelper(IFILEProvider _fileProvider) 
        { 
            this._fileProvider = _fileProvider;
        }

        public string GetFilePath(Guid? FILE_FileInfo_ID, string? Type = null)
        {
            if (FILE_FileInfo_ID != null)
            {
                var fileInfo = _fileProvider.GetFileInfo(FILE_FileInfo_ID.Value);

                if (fileInfo != null)
                {
                    var fileName = "D:/Comunix/ImagesCache/" + fileInfo.ID + fileInfo.FileExtension;
                    var downloadFileName = "/Cache/" + fileInfo.ID + fileInfo.FileExtension;

                    if (!string.IsNullOrEmpty(Type))
                    {
                        fileName = "D:/Comunix/ImagesCache/" + fileInfo.ID + "_" + Type + "_" + fileInfo.FileExtension;
                        downloadFileName = "/Cache/" + fileInfo.ID + "_" + Type + "_" + fileInfo.FileExtension;
                    }

                    if (!File.Exists(fileName))
                    {
                        var store = _fileProvider.GetFileStorage(fileInfo.ID);

                        if (store != null)
                        {
                            File.WriteAllBytes(fileName, store.FileImage);

                            return downloadFileName;
                        }
                    }
                    else
                    {
                        return downloadFileName;
                    }

                }
            }

            return "/Images/Backend/Background.png";
        }
        public string GetThemePath(V_HOME_Theme Item)
        {
            if (Item.FILE_FileInfo_ID != null)
            {
                var fileInfo = _fileProvider.GetFileInfo(Item.FILE_FileInfo_ID.Value);

                if (fileInfo != null)
                {
                    if (!File.Exists("D:/Comunix/ImagesCache/" + fileInfo.ID + fileInfo.FileExtension))
                    {
                        var store = _fileProvider.GetFileStorage(fileInfo.ID);

                        if (store != null)
                        {
                            File.WriteAllBytes("D:/Comunix/ImagesCache/" + fileInfo.ID + fileInfo.FileExtension, store.FileImage);

                            return "/Cache/" + fileInfo.ID + fileInfo.FileExtension;
                        }
                    }
                    else
                    {
                        return "/Cache/" + fileInfo.ID + fileInfo.FileExtension;
                    }
                }
            }

            return "/Images/Backend/Logo_512x512.png";
        }
        public string GetPDFThumbnail(Guid? FILE_FileInfo_ID)
        {
            if(FILE_FileInfo_ID != null)
            {
                var fileInfo = _fileProvider.GetFileInfo(FILE_FileInfo_ID.Value);

                if (fileInfo != null)
                {
                    var fileName = "D:/Comunix/ImagesCache/" + fileInfo.ID + "_thumbnail.png";
                    var downloadFileName = "/Cache/" + fileInfo.ID + "_thumbnail.png";

                    if (!File.Exists(fileName))
                    {
                        var data = ExportToImage(FILE_FileInfo_ID.Value);

                        if (data != null)
                        {
                            File.WriteAllBytes(fileName, data);

                            return downloadFileName;
                        }
                    }
                    else
                    {
                        return downloadFileName;
                    }
                }
            }

            return "/Images/Backend/Background.png";
        }
        public string GetFilePathThumbnail(Guid FILE_FileInfo_ID)
        {
            if (FILE_FileInfo_ID != null)
            {
                var fileInfo = _fileProvider.GetFileInfo(FILE_FileInfo_ID);

                if (fileInfo != null)
                {
                    var fileName = "D:/Comunix/ImagesCache/Thumbnail_" + fileInfo.ID + fileInfo.FileExtension;
                    var downloadFileName = "/Cache/Thumbnail_" + fileInfo.ID + fileInfo.FileExtension;

                    if (!File.Exists(fileName))
                    {
                        var store = _fileProvider.GetFileStorage(fileInfo.ID);

                        if (store != null)
                        {
                            Image image = Image.FromStream(new MemoryStream(store.FileImage));
                            Image thumb = image.GetThumbnailImage(480, 270, () => false, IntPtr.Zero);
                            thumb.Save(fileName);

                            return downloadFileName;
                        }
                    }
                    else
                    {
                        return downloadFileName;
                    }

                }
            }

            return "/Images/Backend/Background.png";
        }
        private byte[]? ExportToImage(Guid FILE_FileInfo_ID)
        {
            var pdf = _fileProvider.GetFileStorage(FILE_FileInfo_ID);

            if (pdf != null && pdf.FileImage != null)
            {
                PdfToImageConverter imageConverter = new PdfToImageConverter();
                MemoryStream ms = new MemoryStream(pdf.FileImage);
                
                imageConverter.Load(ms);

                Stream outputStream = imageConverter.Convert(0, false, false);

                if (outputStream != null && outputStream.Length > 0)
                {
                    MemoryStream stream = outputStream as MemoryStream;

                    return stream.ToArray();
                }
            }

            return null;
        }
    }
}
