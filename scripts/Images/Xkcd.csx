/**
* <description>
*     Grab XKCD comic image urls
* </description>
*
* <commands>
*     mmbot xkcd [latest]- The latest XKCD comic;
*     mmbot xkcd &lt;num&gt; - XKCD comic &lt;num&gt;
*     mmbot xkcd random - random XKCD comic;
* </commands>
* 
* <notes>
*     Ported from https://github.com/github/hubot-scripts/blob/master/src/scripts/xkcd.coffee
* </notes>
* 
* <author>
*     PeteGoo
* </author>
*/

var robot = Require<Robot>();

private static Random _random = new Random(DateTime.Now.Millisecond);

robot.Respond(@"xkcd(\s+latest)?$", msg =>
{
    msg.Http("http://xkcd.com/info.0.json").Get((err, res) => {
        if (res.StatusCode == HttpStatusCode.NotFound)
        {
            msg.Send("Comic not found");
            return;
        }

        res.Json(body => {
            msg.Send((string)body["title"], (string)body["img"], (string)body["alt"]);
        });
    });
});


robot.Respond(@"xkcd\s+(\d+)", msg =>
{
    var num = msg.Match[1];

    FetchComic(msg, num);
});

robot.Respond(@"xkcd\s+random", msg =>
{
    msg.Http("http://xkcd.com/info.0.json").Get((err, res) => {
        if (res.StatusCode == HttpStatusCode.NotFound)
        {
            msg.Send("Comic not found");
            return;
        }

        res.Json(body => {
            var max = int.Parse((string) body["num"]);
            var num = _random.Next(max);
            FetchComic(msg, num.ToString());
        });
    });
});


private static void FetchComic(IResponse<TextMessage> msg, string num)
{
    msg.Http(string.Format("http://xkcd.com/{0}/info.0.json", num)).Get((err, res) => {
        if (res.StatusCode == HttpStatusCode.NotFound)
        {
            msg.Send(string.Format("Comic {0} not found", num));
            return;
        }

        res.Json(body => {
            msg.Send((string) body["title"], (string) body["img"], (string) body["alt"]);
        });
    });
}
