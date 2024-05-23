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

namespace ICWebApp.Components.Pages.Homepage.Frontend.Amtstafel
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
            SessionWrapper.PageTitle = TextProvider.Get("HP_MAINMENU_AMTSTAFEL");
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = TextProvider.Get("HP_MAINMENU_AMTSTAFEL_DESCRIPTION");
            SessionWrapper.ShowTitleSepparation = false;
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;
            SessionWrapper.ShowQuestioneer = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Albopretorio", "HP_MAINMENU_AMTSTAFEL", null, null, true);

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
                    var link = "https://data.gvcc.net/AlboPretorioOnline/?gemeinde=" + Municipality.AmtstafelLinkCode + "&lang=" + LangProvider.GetLanguage2DigitCode();

                    if (!string.IsNullOrEmpty(SubUrl))
                    {
                        link = Uri.UnescapeDataString(SubUrl).Replace("^", "/").Replace("amp;", "").Replace("_", "^");
                    }

                    WebClient wc = new WebClient();
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    string html = wc.DownloadString(link);

                    var doc = new HtmlDocument();

                    doc.LoadHtml(html);

                    foreach (HtmlNode node in doc.DocumentNode.Descendants("a"))
                    {
                        var att = node.GetAttributeValue("onclick", "");

                        var regex = new Regex("http(s)?:(.*)(.PDF|.pdf)");
                        var match = regex.Match(att);

                        if (match != null && !string.IsNullOrEmpty(match.Value) && !string.IsNullOrEmpty(att))
                        {
                            node.SetAttributeValue("onclick", "openDocument('" + match.Value + "', '_blank');");
                        }
                        else if (!string.IsNullOrEmpty(att))
                        {
                            var redirectRegex = new Regex(".open[(][\\\\]['](.*)(',\\\\')(.*)(\\\\'\\);)");
                            var redirectmatch = redirectRegex.Match(att);

                            if (redirectmatch != null && !string.IsNullOrEmpty(redirectmatch.Value))
                            {
                                var redirectLink = redirectmatch.Value.Replace(@".open(\'", "");
                                redirectLink = redirectLink.Replace(@"\',\'_self\');", "");

                                node.SetAttributeValue("onclick", "openDocument('" + NavManager.BaseUri + "Hp/Albopretorio/" + Uri.EscapeDataString(("https://data.gvcc.net/AlboPretorioOnline/" + redirectLink).Replace("/", "^")) + "', '_self');");
                            }
                        }
                        else
                        {
                            node.Attributes.Remove("onclick");
                        }

                        var href = node.GetAttributeValue("href", "");

                        if (href != "#")
                        {
                            node.Attributes.Add("target", "_blank");
                        }
                        else
                        {
                            node.Attributes.Remove("href");
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

                    doc = ReplaceTables(doc);

                    var result = doc.GetElementbyId("maincol").OuterHtml;

                    result = result.Replace("tablediv", "tablediv mb-30");

                    AmtstafelHtml = (MarkupString)result;
                }
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            base.OnParametersSet();
        }
        private void BackToRoot()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Hp/Albopretorio", true);
            StateHasChanged();
        }
        private HtmlDocument ReplaceTables(HtmlDocument doc)
        {
            List<List<TableElement>> tables = new List<List<TableElement>>();

            foreach (HtmlNode head in doc.DocumentNode.Descendants("table"))
            {
                var elementList = new List<TableElement>();

                foreach (HtmlNode body in head.ChildNodes.Where(p => p.Name == "tbody").ToList())
                {
                    foreach(HtmlNode tr in body.ChildNodes.Where(p => p.Name == "tr").ToList())
                    {
                        if(tr.ChildNodes.Where(p => p.Name == "td").Count() == 2)
                        {
                            var newElement = new TableElement();

                            var descNode = tr.ChildNodes.Where(p => p.Name == "td").ToList()[0];

                            if (descNode != null)
                            {
                                newElement.Description = descNode.InnerText;
                            }

                            var docNode = tr.ChildNodes.Where(p => p.Name == "td").ToList()[1];

                            if (docNode != null)
                            {
                                var att = docNode.ChildNodes.FirstOrDefault();

                                if (att != null)
                                {
                                    newElement.DocumentUrl = att.GetAttributeValue("onclick", "");
                                }
                            }

                            elementList.Add(newElement);
                        }

                        if (tr.ChildNodes.Where(p => p.Name == "td").Count() > 6)
                        {
                            int comissionOffset = 0;

                            if (tr.ChildNodes.Where(p => p.Name == "td").Count() == 7)
                            {
                                comissionOffset = -1;
                            }

                            var newElement = new TableElement();

                            var nrNode = tr.ChildNodes.Where(p => p.Name == "td").ToList()[0];

                            if (nrNode != null)
                            {
                                newElement.Number = nrNode.InnerText;
                            }

                            var datumNode = tr.ChildNodes.Where(p => p.Name == "td").ToList()[1];

                            if (datumNode != null)
                            {
                                newElement.Date = datumNode.InnerText;
                            }

                            var descNode = tr.ChildNodes.Where(p => p.Name == "td").ToList()[2];

                            if (descNode != null)
                            {
                                newElement.Description = descNode.InnerText;
                            }

                            if (comissionOffset == 0)
                            {
                                var kommNode = tr.ChildNodes.Where(p => p.Name == "td").ToList()[3];

                                if (kommNode != null)
                                {
                                    newElement.Commission = kommNode.InnerText;
                                }
                            }

                            var vonNode = tr.ChildNodes.Where(p => p.Name == "td").ToList()[4 + comissionOffset];

                            if (vonNode != null)
                            {
                                newElement.OnlineFrom = vonNode.InnerText;
                            }

                            var bisNode = tr.ChildNodes.Where(p => p.Name == "td").ToList()[5 + comissionOffset];

                            if (bisNode != null)
                            {
                                newElement.OnlineTo = bisNode.InnerText;
                            }

                            var docNode = tr.ChildNodes.Where(p => p.Name == "td").ToList()[6 + comissionOffset];

                            if (docNode != null)
                            {
                                var att = docNode.ChildNodes.FirstOrDefault();

                                if (att != null)
                                {
                                    newElement.DocumentUrl = att.GetAttributeValue("onclick", "");
                                }
                            }

                            var anlagenNode = tr.ChildNodes.Where(p => p.Name == "td").ToList()[7 + comissionOffset];

                            if (anlagenNode != null)
                            {
                                var att = anlagenNode.ChildNodes.FirstOrDefault();

                                if (att != null)
                                {
                                    newElement.AttachmentUrl = att.GetAttributeValue("onclick", "");
                                }
                            }

                            elementList.Add(newElement);                            
                        }
                    }                    
                }

                tables.Add(elementList);
            }

            //Delete and Replace

            var ogTables = new List<HtmlNode>(doc.DocumentNode.Descendants("table"));

            foreach (var table in tables)
            {
                var tableTitle = doc.DocumentNode.Descendants("table").ElementAt(0).ParentNode.ChildNodes.Where(p => p.Name == "h2").FirstOrDefault();

                StringBuilder newTable = new StringBuilder();

                newTable.Append("<div class=\"it-page-section mb-30 has-bg-grey p-4\">");
                newTable.Append("<div class=\"row g-4 albopretorio-table\">");

                if(tableTitle != null)
                {
                    newTable.AppendLine("<div class=\"anchor-title-container\">");
                    newTable.AppendLine("<h2 class=\"mb-0\">");
                    newTable.AppendLine(tableTitle.InnerText);
                    newTable.AppendLine("</h2>");
                    newTable.AppendLine("</div>");

                    var item = table.FirstOrDefault();

                    if (item != null && !string.IsNullOrEmpty(item.Number))
                    {
                        newTable.AppendLine("<div class=\"styled-list-container\">");
                        newTable.AppendLine(TextProvider.Get("HOMEPAGE_ALBOPRETORIO_CONTAINER_DESCRIPTION"));
                        newTable.AppendLine("</div>");
                    }
                }

                foreach (var item in table)
                {
                    newTable.AppendLine("<div class=\"col-12 albopretorio-item\">");
                    newTable.AppendLine("<div class=\"cmp-card-simple card-wrapper pb-0 rounded border border-light albopretorio-item\">");
                    newTable.AppendLine("<div class=\"card shadow-sm rounded\">");
                    newTable.AppendLine("<div class=\"card-body\">");

                    //Date + Comission
                    if (!string.IsNullOrEmpty(item.Date))
                    {
                        var datetimeFormat = DateTime.Parse(item.Date);

                        newTable.AppendLine("<div class=\"category-top\">");
                        newTable.AppendLine("<span class=\"text-decoration-none\">");
                        newTable.AppendLine(datetimeFormat.ToString("dd MMM yy"));
                        newTable.AppendLine("</span>");

                        if (!string.IsNullOrEmpty(item.Commission))
                        {
                            newTable.AppendLine("<span class=\"data\">");
                            newTable.AppendLine("</span>");
                            newTable.AppendLine("<span class=\"category\">");
                            newTable.AppendLine(item.Commission);
                            newTable.AppendLine("</span>");
                        }
                        newTable.AppendLine("</div>");
                    }

                    if (!string.IsNullOrEmpty(item.Number))
                    {
                        //Title
                        newTable.AppendLine("<a class=\"text-decoration-none\" onclick=\"" + item.DocumentUrl + "\">");
                        newTable.AppendLine("<h3 class=\"card-title t-primary title-xlarge\">" + item.Number + "</h3>");
                        newTable.AppendLine("</a>");
                    }

                    if (!string.IsNullOrEmpty(item.Description))
                    {
                        //Description
                        if (!string.IsNullOrEmpty(item.Number))
                        {
                            newTable.AppendLine("<p class=\"text-paragraph mb-0 pt-3\">");
                        }
                        else
                        {
                            newTable.AppendLine("<p class=\"text-paragraph mb-0\">");
                        }
                        newTable.AppendLine(item.Description);
                        newTable.AppendLine("</p>");
                    }

                    if (!string.IsNullOrEmpty(item.DocumentUrl))
                    {
                        //Doclink
                        newTable.AppendLine("<div class=\"d-flex flex-nowrap pt-3\">");
                        newTable.AppendLine("<div class=\"icon\">");
                        newTable.AppendLine("<svg class=\"icon icon-primary icon-sm\" aria-hidden=\"true\"><use xlink:href=\"css/bootstrap-italia/svg/sprites.svg#it-file-pdf-ext\"></use></svg>");
                        newTable.AppendLine("</div>");
                        newTable.AppendLine("<a class=\"text-decoration-none albopretorio-document-link\" onclick=\"" + item.DocumentUrl + "\">" + TextProvider.Get("HOMEPAGE_ALBOPRETORIO_SHOW_DOCUMENT") + "</a>");
                        newTable.AppendLine("</div>");
                    }

                    if (!string.IsNullOrEmpty(item.AttachmentUrl))
                    {
                        //Attachmentlink
                        newTable.AppendLine("<div class=\"d-flex flex-nowrap\">");
                        newTable.AppendLine("<div class=\"icon\">");
                        newTable.AppendLine("<svg class=\"icon icon-primary icon-sm\" aria-hidden=\"true\"><use xlink:href=\"css/bootstrap-italia/svg/sprites.svg#it-clip\"></use></svg>");
                        newTable.AppendLine("</div>");
                        newTable.AppendLine("<a class=\"text-decoration-none albopretorio-document-link\" onclick=\"" + item.AttachmentUrl + "\">" + TextProvider.Get("HOMEPAGE_ALBOPRETORIO_ATTACHMENTS") + "</a>");
                        newTable.AppendLine("</div>");
                    }

                    if (!string.IsNullOrEmpty(item.OnlineFrom))
                    {

                        //Onlinetime
                        newTable.AppendLine("<p class=\"albopretorio-online-time pt-3 mb-0\">");
                        newTable.AppendLine(TextProvider.Get("HOMEPAGE_ALBOPRETORIO_ONLINE_TIME") + " ");
                        newTable.AppendLine(TextProvider.Get("HOMEPAGE_ALBOPRETORIO_ONLINE_FROM") + " <b>" + item.OnlineFrom + "</b>");
                        newTable.AppendLine(TextProvider.Get("HOMEPAGE_ALBOPRETORIO_ONLINE_TO") + " <b>" + item.OnlineTo + "</b>");
                        newTable.AppendLine("</p>");
                    }

                    newTable.AppendLine("</div>");
                    newTable.AppendLine("</div>");
                    newTable.AppendLine("</div>");
                    newTable.AppendLine("</div>");
                }

                newTable.AppendLine("</div>");
                newTable.AppendLine("</div>");

                doc.DocumentNode.Descendants("table").ElementAt(0).ParentNode.RemoveChild(tableTitle);

                foreach (var p in doc.DocumentNode.Descendants("table").ElementAt(0).ParentNode.ChildNodes.Where(p => p.Name == "p").ToList())
                {
                    doc.DocumentNode.Descendants("table").ElementAt(0).ParentNode.RemoveChild(p);
                }

                doc.DocumentNode.Descendants("table").ElementAt(0).ParentNode.ReplaceChild(HtmlNode.CreateNode(newTable.ToString()), doc.DocumentNode.Descendants("table").ElementAt(0));
            }

            return doc;
        }
    }
}
