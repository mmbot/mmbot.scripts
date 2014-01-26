/**
* <description>
*    Downloads a script from a gist and adds it to the current environment
* </description>
*
* <commands>
*    mmbot script gist <gist url>
* </commands>
* 
* <author>
*    dkarzon
* </author>
*/

var robot = Require<Robot>();

robot.Respond(@"script gist (.*)", msg =>
{
    if (!msg.Message.User.IsAdmin(robot))
    {
        msg.Send("You must be an admin to run this command");
        return;
    }

    string url = msg.Match[1].Trim();

    Uri uri;
    try
    {
        uri = new Uri(url);
    }
    catch (Exception ex)
    {
        msg.Send("Invalid Uri: " + ex.Message);
        return;
    }

    if (!uri.Host.Equals("gist.github.com", StringComparison.OrdinalIgnoreCase))
    {
        msg.Send("Only accepting Github Gists, try again later...");
        return;
    }

    var gistId = url.Substring(url.LastIndexOf("/") + 1);

    msg.Http(string.Format("https://api.github.com/gists/{0}", gistId))
        .Headers(new Dictionary<string, string>{{"User-Agent", "mmbot"}})
        .GetJson((ex, response, body) =>
        {
            if (ex != null)
            {
                msg.Send("That's a bad one...");
                return;
            }
            foreach (var gistFile in body["files"])
            {
                var script = (string)gistFile.Children().First()["content"];
                var name = (string)gistFile.Children().First()["filename"];

                if (!name.EndsWith(".csx"))
                {
                    name += ".csx";
                }

                var filePath = Path.Combine(Environment.CurrentDirectory, Path.Combine("scripts", name));
                File.WriteAllText(filePath, script);

                try
                {
                    robot.LoadScriptFile(name, filePath);
                    msg.Send(string.Format("Successfully added script: {0}", name));
                }
                catch (Exception scriptEx)
                {
                    if (File.Exists(filePath))
                    {
                        //clean up
                        try
                        {
                            File.Delete(filePath);
                        }
                        catch { }
                    }
                    msg.Send(string.Format("Failed to load script: ({0}) - {1}", name, scriptEx.Message));
                }
            }
        });
});