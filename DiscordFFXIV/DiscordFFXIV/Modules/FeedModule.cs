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
        private int newsUpdateTime = 10000; //900000 = 15min
        private List<string> newsLink;
        private string url = "https://eu.finalfantasyxiv.com/lodestone";
        private string urlBase = "https://eu.finalfantasyxiv.com";

        public FeedModule()
        {
            var cancellationToken = new CancellationToken();
            TimerRunning(cancellationToken);
        }

        [Command("news")]
        [Summary("Gets the latest news")]
        public async Task News()
        {
                      
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

            var data = newsLink;
            using (StreamWriter file = File.CreateText(@"path.txt"))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, data);
            }
            await ReplyAsync("",false,DiscordFFXIV.Extensions.EmbedBuilderExtension.CustomEmbed("FFXIV News", content, null, null));
        }

        public async Task UpdateUrlEvent()
        {
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//li[@class='news__list']");
                newsLink = table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", ""))).ToList();
            }

            var data = newsLink;
            using (StreamWriter file = File.CreateText(@"path.txt"))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, data);
            }
        }

        public bool NewUpdateCheck()
        {
            var reader = jsonReader();

            if (reader.Count <= 2) { return false; }

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
                    await Task.Delay(newsUpdateTime - (int) (watch.ElapsedMilliseconds % 1000), token);
                    await UpdateUrlEvent();
                    if (NewUpdateCheck()) { await News(); }
                }
                catch (TaskCanceledException)
                {
                }

            }
        }

        private List<string> jsonReader()
        {
            var items = new List<string>();
            using (StreamReader r = new StreamReader("SchemaList.json"))
            {
                var json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<List<string>>(json);
                
            }

            return items;
        }


    }
}
