using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Helper
{
    public class ThemeHelper : IThemeHelper
    {
        private IAUTHProvider _authProvider;
        private ISessionWrapper _sessionWrapper;
        private NavigationManager _navManager;
        public ThemeHelper(IAUTHProvider _authProvider, ISessionWrapper _sessionWrapper, NavigationManager _navManager) 
        { 
            this._authProvider = _authProvider;
            this._sessionWrapper = _sessionWrapper;
            this._navManager = _navManager;
        }

        public string GetCurrentColor()
        {
            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                var mun = _authProvider.GetMunicipalitySync(_sessionWrapper.AUTH_Municipality_ID.Value);

                if(mun != null)
                {
                    if (!string.IsNullOrEmpty(mun.ThemeColor))
                    {
                        return mun.ThemeColor;
                    }
                }
            }

            return "green";
        }
        public void SetThemeColor(string Color)
        {
            if (_sessionWrapper.AUTH_Municipality_ID != null)
            {
                var mun = _authProvider.GetMunicipalitySync(_sessionWrapper.AUTH_Municipality_ID.Value);

                if (mun != null)
                {
                    mun.ThemeColor = Color;

                    _authProvider.SetMunicipalitySync(mun);
                }
            }
        }
        public string GetFaviconPath(bool GetDefault = false)
        {
            if (!GetDefault && _sessionWrapper.AUTH_Municipality_ID != null)
            {
                var mun = _authProvider.GetMunicipalitySync(_sessionWrapper.AUTH_Municipality_ID.Value);

                if (mun != null)
                {
                    if (!File.Exists("D:/Comunix/ImagesCache/" + mun.ID.ToString() + ".ico"))
                    {
                        if (mun.LogoSquare != null)
                        {
                            using (var ms = new MemoryStream(mun.LogoSquare))
                            {
                                Image image = Image.FromStream(ms);

                                var icon = ParseImageToFavicon(image);

                                var result = new MemoryStream();

                                icon.Save(result);

                                File.WriteAllBytes("D:/Comunix/ImagesCache/" + mun.ID.ToString() + ".ico", result.ToArray());
                            }
                        }
                        else if(mun.Logo != null)
                        {
                            using (var ms = new MemoryStream(mun.Logo))
                            {
                                Image image = Image.FromStream(ms);

                                var icon = ParseImageToFavicon(image);

                                var result = new MemoryStream();

                                icon.Save(result);

                                File.WriteAllBytes("D:/Comunix/ImagesCache/" + mun.ID.ToString() + ".ico", result.ToArray());
                            }
                        }
                    }

                    return _navManager.BaseUri.TrimEnd('/') + "/Cache/" + mun.ID.ToString() + ".ico";
                }
            }

            return "/favicon.ico";
        }
        private Icon ParseImageToFavicon(Image img)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                using (var bw = new System.IO.BinaryWriter(ms))
                {
                    // Header
                    bw.Write((short)0);   // 0 : reserved
                    bw.Write((short)1);   // 2 : 1=ico, 2=cur
                    bw.Write((short)1);   // 4 : number of images
                                          // Image directory
                    var w = img.Width;
                    if (w >= 256) w = 0;
                    bw.Write((byte)w);    // 0 : width of image
                    var h = img.Height;
                    if (h >= 256) h = 0;
                    bw.Write((byte)h);    // 1 : height of image
                    bw.Write((byte)0);    // 2 : number of colors in palette
                    bw.Write((byte)0);    // 3 : reserved
                    bw.Write((short)0);   // 4 : number of color planes
                    bw.Write((short)0);   // 6 : bits per pixel
                    var sizeHere = ms.Position;
                    bw.Write((int)0);     // 8 : image size
                    var start = (int)ms.Position + 4;
                    bw.Write(start);      // 12: offset of image data
                                          // Image data
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    var imageSize = (int)ms.Position - start;
                    ms.Seek(sizeHere, System.IO.SeekOrigin.Begin);
                    bw.Write(imageSize);
                    ms.Seek(0, System.IO.SeekOrigin.Begin);

                    // And load it
                    return new Icon(ms);
                }
            }
        }
    }
}
