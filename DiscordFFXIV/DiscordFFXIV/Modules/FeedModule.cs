using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Discord.Commands;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Timers;

namespace DiscordFFXIV.Modules
{
    [Name("Feed")]
    [Summary("Do some math I guess")]
    public class FeedModule : ModuleBase<SocketCommandContext>
    {
        private int newsUpdateTime = 900000; //900000 = 15min
        private List<string> newsLink;
        private string url = "https://eu.finalfantasyxiv.com/lodestone";
        private string urlBase = "https://eu.finalfantasyxiv.com";
        public string NewsStatus = "";
        public string SpecialStatus = "";
        private List<Feed> Feeds;

        public FeedModule()
        {
            var cancellationToken = new CancellationToken();
            TimerRunning(cancellationToken);
            Feeds = new List<Feed>
            {
                new Feed("news", ""), new Feed("special", "")
            };
        }

        [Command("status")]
        [Summary("Gets the status of the feed")]
        public async Task Status()
        {
            await ReplyAsync("", false, DiscordFFXIV.Extensions.EmbedBuilderExtension.CustomEmbed("FFXIV Feed Status", "News feed: " + Feeds.Where(x => x.name == "news").FirstOrDefault().status
            + "\n" + "Special News feed: " + Feeds.Where(x => x.name == "special").FirstOrDefault().status + "\n", null, null));
        }

        [Command("start news")]
        [Summary("Gets the latest news")]
        public async Task News()
        {
            var feed = Feeds.Where(x => x.name == "news").FirstOrDefault();
            feed.StatusOnline();

            var content = "";

            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectNodes("//li[@class='news__list']");
                newsLink = table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", ""))).ToList();

                var fullUrl = urlBase + newsLink.First();

                var html2 = await client.GetStringAsync(fullUrl);
                var doc2 = new HtmlAgilityPack.HtmlDocument();
                doc2.LoadHtml(html2);

                var table2 = doc2.DocumentNode.SelectSingleNode("//div[@class='news__detail__wrapper']");
                content = table2.InnerText;
            }

            var Feedurl = Feeds.Where(x => x.name == "news").FirstOrDefault();
            Feedurl.url = newsLink.First();
            await UpdateUrlEvent();
            var data = newsLink;

            var filePath = @"news.txt";
            // Read existing json data
            var jsonData = System.IO.File.ReadAllText(filePath);
            // De-serialize to object or create new list
            var employeeList = new List<string>();

            // Add any new employees
            employeeList[0] = data.First();
            employeeList[1] = data.First();

            // Update json data string
            jsonData = JsonConvert.SerializeObject(employeeList);
            System.IO.File.WriteAllText(filePath, jsonData);

            await ReplyAsync("", false, DiscordFFXIV.Extensions.EmbedBuilderExtension.CustomEmbed("FFXIV News", content, null, null));
        }

        [Command("start special")]
        [Summary("Gets the latest news")]
        public async Task SpecialNews()
        {
            var feed = Feeds.Where(x => x.name == "special").FirstOrDefault();
            feed.StatusOnline();

            var content = "";

            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//li[@class='news__list']");
                newsLink = table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", ""))).ToList();

                var fullUrl = urlBase + newsLink.First();

                var html2 = await client.GetStringAsync(fullUrl);
                var doc2 = new HtmlAgilityPack.HtmlDocument();
                doc2.LoadHtml(html2);

                table = doc2.DocumentNode.SelectSingleNode("//div[@class='news__detail__wrapper']");
                content = table.InnerText;
            }

            await UpdateUrlEvent();
            var data = newsLink;
            var Feedurl = Feeds.Where(x => x.name == "special").FirstOrDefault();
            Feedurl.url = newsLink.First();


            var filePath = @"special.txt";
            // Read existing json data
            var jsonData = System.IO.File.ReadAllText(filePath);
            // De-serialize to object or create new list
            var employeeList = new List<string>
            {
                data.First(), data.First()
            };

            // Update json data string
            jsonData = JsonConvert.SerializeObject(employeeList);
            System.IO.File.WriteAllText(filePath, jsonData);

            await ReplyAsync("", false, DiscordFFXIV.Extensions.EmbedBuilderExtension.CustomEmbed("FFXIV News", content, null, null));
        }

        public async Task UpdateUrlEvent()
        {
            foreach (var feed in Feeds)
            {
                var oldData = new List<string>();
                using (var client = new HttpClient())
                {
                    var html = await client.GetStringAsync(url);
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);

                    oldData = newsLink;
                    var table = doc.DocumentNode.SelectSingleNode("//li[@class='news__list']");
                    newsLink = table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", ""))).ToList();
                }

                var data = newsLink;

                var filePath = feed.name + ".txt";
                // Read existing json data
                var jsonData = System.IO.File.ReadAllText(filePath);
                // De-serialize to object or create new list
                var employeeList = JsonConvert.DeserializeObject<List<string>>(jsonData) ?? new List<string>();

                if (employeeList.Count <= 2 || !oldData.Any())
                {
                    employeeList.Add(data.First());
                    employeeList.Add(data.First());
                }
                else
                {
                    // Add any new employees
                    employeeList[0] = data.First();
                    employeeList[1] = oldData.First();
                }

                // Update json data string
                jsonData = JsonConvert.SerializeObject(employeeList);
                System.IO.File.WriteAllText(filePath, jsonData);
            }
        }

        public bool NewUpdateCheck(string path)
        {
            var reader = jsonReader(path);

            if (reader.Count < 2) { return false; }

            if (reader.ToArray()[0] == reader.ToArray()[1]) { return false; }

            return true;
        }

        async void TimerRunning(CancellationToken token)
        {
            //var startTime = DateTime.Now;
            var watch = Stopwatch.StartNew();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine("Event Updated");
                    await Task.Delay(newsUpdateTime - (int) (watch.ElapsedMilliseconds % 1000), token);
                    await UpdateUrlEvent();
                    if (NewUpdateCheck("special.txt")) { await SpecialNews(); }
                }
                catch (TaskCanceledException) { }
            }
        }

        private List<string> jsonReader(string path)
        {
            var items = new List<string>();
            using (StreamReader r = new StreamReader(path))
            {
                var json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<List<string>>(json);
            }

            return items;
        }

        public class Feed
        {
            public string url;
            public string name;
            public string status;

            public Feed(string Name,
                string Url)
            {
                name = Name;
                url = Url;
                status = "Offline";
            }

            public void StatusOnline()
            {
                status = "Online";
            }

            public void StatusOffline()
            {
                status = "Offline";
            }
        }
    }
}