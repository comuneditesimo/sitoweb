using CodeHollow.FeedReader;
using HtmlAgilityPack;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static SQLite.SQLite3;

namespace ICWebApp.Application.Services
{
    public class NEWSService : INEWSService
    {
        private INEWSProvider _newsProvider;
        public NEWSService(INEWSProvider _newsProvider)
        {
            this._newsProvider = _newsProvider;
        }
        public async Task<bool> ReadRSSFeed(Guid AUTH_Municipality_ID)
        {
            try
            {
                var feedsToFetch = await _newsProvider.GetRSSConfig(AUTH_Municipality_ID);

                if (feedsToFetch != null)
                {
                    foreach (var feed in feedsToFetch)
                    {
                        if (feed.LANG_Language_ID != null && feed.AUTH_Municipality_ID != null &&
                            DateTime.Now - feed.LastRun > TimeSpan.FromMinutes(10))
                        {
                            var urls = await FeedReader.GetFeedUrlsFromUrlAsync(feed.Url);

                            string feedUrl = feed.Url;

                            if (urls.Count() == 1)
                                feedUrl = urls.First().Url;
                            else if (urls.Count() == 2)
                                feedUrl = urls.First().Url;

                            var data = await FeedReader.ReadAsync(feedUrl);

                            if (data.Items != null && data.Items.Count() > 0)
                            {
                                //Clear existing
                                var existingArticles =
                                    await _newsProvider.GetArticleList(feed.AUTH_Municipality_ID.Value,
                                        feed.LANG_Language_ID.Value, "RSS");

                                if (existingArticles != null)
                                {
                                    foreach (var exArtic in existingArticles)
                                    {
                                        await _newsProvider.RemoveArticle(exArtic);
                                    }
                                }

                                //Save current
                                foreach (var item in data.Items)
                                {
                                    string? content = null;
                                    string? image = null;

                                    if (item.Description != null)
                                    {
                                        var images = GetImage(item.Description);
                                        var decodedContent = HttpUtility.HtmlDecode(item.Description);

                                        foreach (var img in images)
                                        {
                                            decodedContent = decodedContent.Replace(img, "");
                                        }

                                        content = decodedContent;

                                        if (images != null && images.Count() > 0)
                                        {
                                            image = images.FirstOrDefault();
                                        }
                                    }

                                    var article = new NEWS_Article();

                                    article.ID = Guid.NewGuid();

                                    article.LANG_Languages_ID = feed.LANG_Language_ID.Value;
                                    article.AUTH_Municipality_ID = feed.AUTH_Municipality_ID.Value;
                                    article.Content = content;

                                    if (content != null)
                                    {
                                        HtmlDocument htmlDoc = new HtmlDocument();
                                        htmlDoc.LoadHtml(content);
                                        string strippedContent = htmlDoc.DocumentNode.InnerText;

                                        if (strippedContent != null && strippedContent.Length > 100)
                                        {
                                            article.ShortContent = strippedContent.Substring(0, 100) + "...";
                                        }
                                        else if (strippedContent != null)
                                        {
                                            article.ShortContent = strippedContent;
                                        }
                                    }

                                    article.Title = item.Title;
                                    article.Image = image;

                                    if (item.PublishingDate != null)
                                        article.PublishingDate = item.PublishingDate;
                                    else
                                        article.PublishingDate = DateTime.Now;

                                    article.LastFeedReadDate = DateTime.Now;
                                    article.Link = item.Link;
                                    article.InputType = "RSS";
                                    article.Enabled = true;

                                    await _newsProvider.SetArticle(article);
                                }
                            }

                            feed.LastRun = DateTime.Now;

                            await _newsProvider.SetRSSConfig(feed);
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        private List<string> GetImage(string Content)
        {
            var html = HttpUtility.HtmlDecode(Content);

            List<string> images = new List<string>();
            string pattern = @"<(img)\b[^>]*>";

            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(html);

            for (int i = 0, l = matches.Count; i < l; i++)
            {
                images.Add(matches[i].Value);
            }

            return images;
        }
    }
}
