/**
* <description>
*     Pulls an image from the google images api
* </description>
*
* <commands>
*     mmbot image me &lt;query&gt; - Returns an image matching the requested search term.
* </commands>
* 
* <notes>
*     Ported from https://github.com/github/hubot/blob/master/src/scripts/google-images.coffee
* </notes>
* 
* <author>
*     PeteGoo
* </author>
*/

using System.Text.RegularExpressions;

var robot = Require<Robot>();

Random _random = new Random(DateTime.Now.Millisecond);
Regex _httpRegex = new Regex(@"^https?:\/\/", RegexOptions.Compiled | RegexOptions.IgnoreCase);

robot.Respond(@"(image|img)( me)? (.*)", msg => ImageMe(msg, msg.Match[3], url => msg.Send(url)));

robot.Respond(@"animate( me)? (.*)", msg => ImageMe(msg, msg.Match[2], url => msg.Send(url), true));

robot.Respond(@"(?:mo?u)?sta(?:s|c)he?(?: me)? (.*)", msg =>
{
    var type = _random.Next(2);
    var mustachify = string.Format("http://mustachify.me/{0}?src=", type);
    var imagery = msg.Match[1];
    if (_httpRegex.IsMatch(imagery))
    {
        msg.Send(mustachify + imagery);
    }
    else
    {
        ImageMe(msg, imagery, url => msg.Send(mustachify + url), false, true);
    }
                
});

private void ImageMe(IResponse<TextMessage> msg, string query, Action<string> cb, bool animated = false, bool faces = false )
{
    msg.Http("http://ajax.googleapis.com/ajax/services/search/images")
        .Query(new
        {
            v = "1.0",
            rsz = "8",
            q = query,
            safe = "active",
            imgtype = faces ? "face" : animated ? "animated" : null
        })
        .GetJson((err, res, body) => {

            try
            {
                var images = body["responseData"]["results"].ToArray();
                if (images.Count() > 0)
                {
                    var image = msg.Random(images);
                    cb(string.Format("{0}#.png", image["unescapedUrl"]));
                }
            }
            catch (Exception)
            {
                msg.Send("erm....issues, move along");
            }
    });
}
