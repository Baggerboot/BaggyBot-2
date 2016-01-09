using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

namespace BaggyBot.Commands
{
    class HttpInterface : ICommand
    {
        public PermissionLevel Permissions { get { return PermissionLevel.All; } }

        public void Use(CommandArgs command)
        {
            var method = command.Args[0].ToUpper();
            var url = command.Args[1];
            if (url.StartsWith("file://"))
            {
                command.Reply("you sneaky bastard; you didn't think I was going to allow that, did you?");
                return;
            }
            if (!url.StartsWith("http"))
            {
                url = "http://" + url;
            }
            var body = string.Join(" ", command.Args.Skip(2));
            string response;
            using (var client = new WebClient())
            {
                try
                {
                    if (method == "GET")
                    {
                        response = client.DownloadString(url);
                    }
                    else
                    {
                        response = client.UploadString(url, method, body);
                    }
                    command.ReturnMessage("Response: " + response);
                }
                catch (WebException e)
                {
                    command.ReturnMessage("The HTTP request failed ({0}).", e.Message);
                }
               
            }
        }
    }
}
