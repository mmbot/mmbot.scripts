/**
* <description>
*     returns a random post from Reddit.com/r/getmotivated
* </description>
*
* <configuration>
*
* </configuration>
*
* <commands>
*     mmbot motivate me - returns a random post from the front page of reddit.com/r/GetMotivated/hot
* </commands>
* 
* <notes>
*     API Access Rules: https://github.com/reddit/reddit/wiki/API
* </notes>
* 
* <author>
*     jamessantiago
* </author>
*/

var robot = Require<Robot>();

robot.Respond(@"motivate( me)?", msg =>
{
    msg.Http("http://www.reddit.com/r/GetMotivated/hot")
        .Headers(new Dictionary<string, string>
            {
                {"Accept-Language", "en-us,en;q=0.5"},
                {"Accept-Charset", "utf-8"},
                {"User-Agent", "mmbot/1.0 by petegoo/jamessantiago"}
            })
        .GetHtml((err, res, body) => {
            if (err != null)
            {
                msg.Send("Unable to query reddit: " + err.ToString());
            }
            else
            {           
                Random r = new Random();            
                var nodes = body.DocumentNode.SelectNodes("//div/div/div/div/div/p[@class='title']/a");
                var i = r.Next(0, nodes.Count - 1);
                var node = nodes[i];
                msg.Send(node.InnerText);
                msg.Send(node.Attributes["href"].Value);
            }
        });
});
