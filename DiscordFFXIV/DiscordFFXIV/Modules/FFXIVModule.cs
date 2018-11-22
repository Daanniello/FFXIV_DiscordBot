using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordFFXIV.Modules
{
    [Name("Feed")]
    [Summary("")]
    class FfxivModule : ModuleBase<SocketCommandContext>
    {
        [Command("News")]
        [Summary("Gets the latest news")]
        public async Task News()
        {
            List<string> websiteLink = new List<string>();
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync("https://eu.finalfantasyxiv.com/lodestone/");
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                var table = doc.DocumentNode.SelectSingleNode("//li[@class='news__list']");
                websiteLink = table.Descendants("a").Select(a => WebUtility.HtmlDecode(a.GetAttributeValue("href", ""))).ToList();

            }

            await ReplyAsync(websiteLink.First());
        }
    }
}
