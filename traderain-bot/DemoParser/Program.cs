using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Discord;
using Discord.Commands;
using Discord.Net;
using VolvoWrench.Demo_stuff;

namespace traderain_bot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Traderain's bot";
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            new Program().Start();
        }

        private DiscordClient _client;

        public class NewsItem
        {
            public Url Url;
            public string Poster;
            public string Topic;
            public string Msg;
            public string Time;

            public NewsItem(string url, string poster, string topic, string msg, string time)
            {
                this.Url = new Url(url);
                this.Poster = poster;
                this.Topic = topic;
                this.Msg = msg;
                this.Time = time;
            }
        }


        public void Start()
        {
            _client = new DiscordClient();
            _client.MessageReceived += async (s, e) =>
            {
                if (!e.Message.IsAuthor)
                {
                    Console.WriteLine("[" + e.Message.Timestamp.ToString("HH:mm:ss") + "] Server:" + e.Server + " Channel:" + e.Channel + " User:" + e.User + " => " + e.Message.Text);
                    if (e.Message.Text.StartsWith("!ping"))
                    {
                        await e.Channel.SendMessage("Pong :ping_pong:");
                    }
                    if (e.Message.Text.StartsWith("!gaben"))
                    {
                        var rand = new Random();
                        var files = Directory.GetFiles("gabe/");
                        await e.Channel.SendFile(files[rand.Next(files.Length)]);
                    }
                    if (e.Message.Text.StartsWith("!fuck"))
                    {
                        await e.Channel.SendMessage("ur mother");
                    }
                    if (e.Message.Text.StartsWith("!meme"))
                    {
                        var rand = new Random();
                        var files = Directory.GetFiles("spider/");
                        await e.Channel.SendFile(files[rand.Next(files.Length)]);
                    }
                    if (e.Message.Text.StartsWith("!news"))
                    {
                        using (var client = new WebClient())
                        {
                            client.DownloadFile("https://forums.sourceruns.org//index.php?action=.xml", "news.xml");
                            var a = File.ReadAllLines("news.xml").Skip(2).ToArray();
                            var b = a.Take(a.Length - 1).ToList();
                            b.Insert(0,"<root>");
                            b.Add("</root>");
                            File.WriteAllLines("news.xml",b);
                            var xDoc = XDocument.Load("news.xml");
                            Console.WriteLine(xDoc.Elements("recent-post").Count());
                            var news = xDoc.Descendants("recent-post").Select(x => new NewsItem(x.Element("link")?.Value, x.Element("starter")?.Element("name")?.Value, x.Element("topic")?.Element("subject")?.Value, x.Element("body")?.Value, x.Element("time")?.Value)).ToList();
                            foreach (var nw in news)
                            {
                                await e.Channel.SendMessage("Post by " + nw.Poster + " " + nw.Time + "\n" + "In topic: " + nw.Topic + "\n" + "```" + nw.Msg + "```" + "\n" + "<" +nw.Url.Value + ">");
                            }
                            File.Delete("news.xml");
                        }
                    }
                    if (e.Message.Text.StartsWith("!help"))
                    {
                        await e.Channel.SendMessage(@"Commands:
``!help`` -> Show this help text.
``!gaben`` -> Random picture about Gabe Newell
``!ping`` -> Sends back pong if the bot is alive.
``!fuck`` -> Sends back ur mother.
``!meme`` -> Random meme.
``!news`` -> News from sourceruns.org");
                    }
                    if (e.Message.Attachments.Length > 0)
                    {
                        try
                        {
                            using (var client = new WebClient())
                            {
                                foreach (var file in e.Message.Attachments)
                                {
                                    Console.WriteLine("Getting file from: " + file.Url);
                                    client.DownloadFile(file.Url, "attachments/" + file.Filename);
                                    var fi = new FileInfo("attachments/" + file.Filename);
                                    if (Path.GetExtension("attachments/" + file.Filename) == ".dem")
                                    {
                                        var ParsedFile = CrossDemoParser.Parse("attachments/" + file.Filename);
                                        File.Delete("attachments/" + file.Filename);
                                        #region Print
                                        switch (ParsedFile.Type)
                                        {
                                            case Parseresult.UnsupportedFile:
                                                await e.Channel.SendMessage(@"```Unsupported file!```");
                                                break;
                                            case Parseresult.GoldSource:
                                                await e.Channel.SendMessage($@"```Analyzed GoldSource engine demo file ({ParsedFile.GsDemoInfo.Header.GameDir}):
----------------------------------------------------------
Demo protocol:              {ParsedFile.GsDemoInfo.Header.DemoProtocol}
Net protocol:               {ParsedFile.GsDemoInfo.Header.NetProtocol}
Directory Offset:           {ParsedFile.GsDemoInfo.Header.DirectoryOffset}
Map name:                   {ParsedFile.GsDemoInfo.Header.MapName}
Game directory:             {ParsedFile.GsDemoInfo.Header.GameDir}
Length in seconds:          {ParsedFile.GsDemoInfo.DirectoryEntries.Sum(x => x.TrackTime).ToString("n3")}s
Frame count:                {ParsedFile.GsDemoInfo.DirectoryEntries.Sum(x => x.FrameCount)}
----------------------------------------------------------```");
                                                break;
                                            case Parseresult.Hlsooe:
                                                await e.Channel.SendMessage($@"```Analyzed HLS:OOE engine demo file ({ParsedFile.HlsooeDemoInfo.Header.GameDirectory}):
----------------------------------------------------------
Demo protocol:              {ParsedFile.HlsooeDemoInfo.Header.DemoProtocol}
Net protocol:               {ParsedFile.HlsooeDemoInfo.Header.Netprotocol}
Directory offset:           {ParsedFile.HlsooeDemoInfo.Header.DirectoryOffset}
Map name:                   {ParsedFile.HlsooeDemoInfo.Header.MapName}
Game directory:             {ParsedFile.HlsooeDemoInfo.Header.GameDirectory}
Length in seconds:          {ParsedFile.HlsooeDemoInfo.DirectoryEntries.Skip(1).Sum(x => x.Frames.Last().Key.Time).ToString("n3")}s
Frame count:                {ParsedFile.HlsooeDemoInfo.DirectoryEntries.Sum(x => x.FrameCount)}
----------------------------------------------------------```");
                                                break;
                                            case Parseresult.Source:
                                                await e.Channel.SendMessage($@"```Analyzed source engine demo file ({ParsedFile.Sdi.GameDirectory}):
----------------------------------------------------------
Demo protocol:              {ParsedFile.Sdi.DemoProtocol}
Net protocol:               {ParsedFile.Sdi.NetProtocol}
Server name:                {ParsedFile.Sdi.ServerName}
Client name:                {ParsedFile.Sdi.ClientName}
Map name:                   {ParsedFile.Sdi.MapName}
Game directory:             {ParsedFile.Sdi.GameDirectory}
Length in seconds:          {ParsedFile.Sdi.Seconds.ToString("#,0.000")}s
Tick count:                 {ParsedFile.Sdi.TickCount}
Frame count:                {ParsedFile.Sdi.FrameCount}
----------------------------------------------------------```");
                                                foreach (var f in ParsedFile.Sdi.Flags)
                                                    switch (f.Name)
                                                    {
                                                        case "#SAVE#":
                                                            await e.Channel.SendMessage($"```\n#SAVE# flag at Tick: {f.Tick} -> {f.Time}s```");
                                                            break;
                                                        case "autosave":
                                                            await e.Channel.SendMessage($"```\nAutosave at Tick: {f.Tick} -> {f.Time}s```");
                                                            break;
                                                    }
                                                break;
                                        }
                                        #endregion
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            await e.Channel.SendMessage("Error!");
                        }
                    }
                }                   
            };

            _client.ExecuteAndWait(async () =>
            {
                await
                    _client.Connect("MjM4NjUwNzk0Njc5NzMwMTc4.CupUlw.-FqrkD0t_Atc3UFWA0KnG2EorqY",
                        TokenType.Bot);
                Console.WriteLine("Connected to api!");
                _client.SetStatus(UserStatus.DoNotDisturb);
                Console.WriteLine("Status set!");
                _client.SetGame("https://traderain.hu");
                Console.WriteLine("Game set!");

            });
        }
    }
}
