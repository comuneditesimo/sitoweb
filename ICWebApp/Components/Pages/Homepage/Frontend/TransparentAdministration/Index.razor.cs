using DocumentFormat.OpenXml.EMMA;
using HtmlAgilityPack;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Homepage.Amtstafel;
using Microsoft.AspNetCore.Components;
using Stripe;
using System.Drawing.Text;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ICWebApp.Components.Pages.Homepage.Frontend.TransparentAdministration
{
    public partial class Index
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IAnchorService AnchorService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Parameter] public string? SubUrl { get; set; }

        private AUTH_Municipality? Municipality;
        private MarkupString? AmtstafelHtml;

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("HP_MAINMENU_TRANSPARENT_AMMINISTRATION");
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = TextProvider.Get("HP_MAINMENU_TRANSPARENT_AMMINISTRATION_DESCRIPTION");
            SessionWrapper.ShowTitleSepparation = false;
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;
            SessionWrapper.ShowQuestioneer = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/AmministrazioneTrasparente", "HP_MAINMENU_TRANSPARENT_AMMINISTRATION", null, null, false);

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        protected override async void OnParametersSet()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Municipality = await AuthProvider.GetMunicipality(SessionWrapper.AUTH_Municipality_ID.Value);

                if (Municipality != null && !string.IsNullOrEmpty(Municipality.AmtstafelLinkCode))
                {
                    var link = "https://transparenz.gvcc.net/Transparenz/Transparenz2014?gemeinde=21" + Municipality.AmtstafelLinkCode;

                    if (!string.IsNullOrEmpty(SubUrl))
                    {
                        link = Uri.UnescapeDataString(SubUrl).Replace("^", "/").Replace("amp;", "").Replace("_", "^");
                    }

                    WebClient wc = new WebClient();

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    string htmlog = Encoding.Latin1.GetString(wc.DownloadData(link));   //NEED TO BE DOUBLE CALL --- BECAUSE OF A BUG ON TARGET SITE
                    string html = Encoding.Latin1.GetString(wc.DownloadData(link + "&lang=" + LangProvider.GetLanguage2DigitCode().ToLower()));

                    var doc = new HtmlDocument();

                    doc.LoadHtml(html);

                    foreach (HtmlNode node in doc.DocumentNode.Descendants("a"))
                    {
                        var href = node.GetAttributeValue("href", "");

                        if (!href.Contains("https://transparenz.gvcc.net/"))
                        {
                            node.SetAttributeValue("onclick", "openDocument('" + NavManager.BaseUri + "Hp/AmministrazioneTrasparente/" + Uri.EscapeDataString(("https://transparenz.gvcc.net/Transparenz/" + href).Replace("/", "^")) + "', '_self');");
                            node.SetAttributeValue("href", NavManager.BaseUri + "Hp/AmministrazioneTrasparente/" + Uri.EscapeDataString(("https://transparenz.gvcc.net/Transparenz/" + href).Replace("/", "^")));
                        }
                        else
                        {
                            node.SetAttributeValue("onclick", "openDocument('" + NavManager.BaseUri + "Hp/AmministrazioneTrasparente/" + Uri.EscapeDataString((href).Replace("/", "^")) + "', '_self');");
                            node.SetAttributeValue("href", NavManager.BaseUri + "Hp/AmministrazioneTrasparente/" + Uri.EscapeDataString((href).Replace("/", "^")));
                        }
                    }

                    int orderID = 0;

                    AnchorService.ClearAnchors();

                    foreach (HtmlNode node in doc.DocumentNode.Descendants("h2"))
                    {
                        if (node.Attributes.Where(p => p.Name == "Id").Any())
                        {
                            node.SetAttributeValue("Id", node.InnerText.Replace(" ", "_").ToLower());
                        }
                        else
                        {
                            node.Attributes.Add("Id", node.InnerText.Replace(" ", "_").ToLower());
                        }

                        AnchorService.AddAnchor(node.InnerText, node.Id, orderID);

                        orderID++;
                    }

                    foreach(HtmlNode node in doc.DocumentNode.Descendants("img").Where(p => p.GetAttributeValue("src", "").Contains("page_white_acrobat.png")))
                    {
                        if (node.Attributes.Where(p => p.Name == "Id").Any())
                        {
                            node.SetAttributeValue("Id", Guid.NewGuid().ToString().ToLower());
                        }
                        else
                        {
                            node.Attributes.Add("Id", Guid.NewGuid().ToString().ToLower());
                        }
                    }

                    var nodes = new List<HtmlNode>(doc.DocumentNode.Descendants("img").Where(p => p.GetAttributeValue("src", "").Contains("page_white_acrobat.png")));

                    foreach (HtmlNode node in nodes)
                    {
                        var ID = node.GetAttributeValue("Id", "");

                        var ogNode = doc.DocumentNode.SelectSingleNode("//*[@id='" + ID + "']");

                        var newNodeString = "<svg class=\"icon icon-primary icon-sm\" aria-hidden=\"true\"><use xlink:href=\"css/bootstrap-italia/svg/sprites.svg#it-file-pdf-ext\"></use></svg>";

                        ogNode.ParentNode.ReplaceChild(HtmlNode.CreateNode(newNodeString), ogNode);
                    }

                    foreach (HtmlNode node in doc.DocumentNode.Descendants("img").Where(p => p.GetAttributeValue("src", "").Contains("page_go.png")))
                    {
                        if (node.Attributes.Where(p => p.Name == "Id").Any())
                        {
                            node.SetAttributeValue("Id", Guid.NewGuid().ToString().ToLower());
                        }
                        else
                        {
                            node.Attributes.Add("Id", Guid.NewGuid().ToString().ToLower());
                        }
                    }

                    var nodesPageGo = new List<HtmlNode>(doc.DocumentNode.Descendants("img").Where(p => p.GetAttributeValue("src", "").Contains("page_go.png")));

                    foreach (HtmlNode node in nodesPageGo)
                    {
                        var ID = node.GetAttributeValue("Id", "");

                        var ogNode = doc.DocumentNode.SelectSingleNode("//*[@id='" + ID + "']");

                        var newNodeString = "<svg class=\"icon icon-primary icon-sm\" aria-hidden=\"true\"><use xlink:href=\"css/bootstrap-italia/svg/sprites.svg#it-external-link\r\n\"></use></svg>";

                        ogNode.ParentNode.ReplaceChild(HtmlNode.CreateNode(newNodeString), ogNode);
                    }

                    var result = doc.GetElementbyId("maincol").OuterHtml;

                    result = result.Replace("tablediv", "tablediv mb-30");
                    result = result.Replace("<br>", "");
                    result = result.Replace("</br>", "");

                    AmtstafelHtml = (MarkupString)result;
                }
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            base.OnParametersSet();
        }
    }
}
