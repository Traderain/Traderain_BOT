using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using _02_commands_framework.Services;
using Npgsql;
using System;
using System.Data;
using System.Collections.Generic;

namespace _02_commands_framework.Modules
{
    // Modules must be public and inherit from an IModuleBase
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        // Dependency Injection will fill this value in for us
        public PictureService PictureService { get; set; }

        [Command("ping")]
        [Alias("pong", "hello")]
        public Task PingAsync()
            => ReplyAsync("pong!");

        [Command("w3dbq")]
        public async Task w3dbqasync([Remainder] string text)
        {
            DataSet ds = new DataSet();

            if(text.Contains("DROP") || text.Contains("CREATE") || !text.StartsWith("\"") || !text.EndsWith("\""))
            {
                await ReplyAsync("Sorry! Only select is supported!");
                return;
            }
            try
            {
                // PostgeSQL-style connection string
                string connstring = "Server=vps-7578eb7e.vps.ovh.net;Port=5432;User Id=wguest;Password=pwd;Database=wmod;";
                // Making connection with Npgsql provider
                NpgsqlConnection conn = new NpgsqlConnection(connstring);
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = text.Trim('\"').Trim();
                cmd.CommandType = CommandType.Text;
                var res = cmd.ExecuteReader();
                var retstr = "";
                for (int i = 0; i < res.FieldCount; i++)
                {
                    retstr += res.GetName(i) + "\t|\t";
                }
                retstr += "\n";
                while (res.Read())
                {
                    for(int i = 0; i < res.FieldCount; i++)
                    {
                        retstr += res[i] + "\t";
                    }
                    retstr += "\n";
                }
                await ReplyAsync("Here are the results :rocket:\n" + retstr);

                conn.Close();
            }
            catch (Exception msg)
            {
                // something went wrong, and you wanna know why
                await ReplyAsync("ERR:" + msg.ToString());
            }
        }
    }
}
