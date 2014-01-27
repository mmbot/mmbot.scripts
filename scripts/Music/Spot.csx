/**
* <description>
*     Fetches a link to a spotify track
* </description>
*
* <commands>
*     mmbot spot me &lt;query&gt; - searches for a matching track;
*     mmbot spot me winning - plays the best track evar.
* </commands>
* 
* 
* <author>
*     petegoo
* </author>
*/

var robot = Require<Robot>();

robot.Respond(@"spot me winning", msg =>
{
    msg.Send("http://open.spotify.com/track/77NNZQSqzLNqh2A9JhLRkg");
    msg.Message.Done = true;
});

robot.Respond(@"spot me (.*)$", msg =>
{
    var q = msg.Match[1];
    msg.Http("http://ws.spotify.com/search/1/track.json")
        .Query(new {q})
        .GetJson((err, res, body) => {

        foreach(var t in body["tracks"])
        {
            try
            {
                if (t["album"]["availability"]["territories"].ToString() == "worldwide" || t["album"]["availability"]["territories"].ToString().IndexOf("NZ") > -1)
                {
                    msg.Send(string.Format("http://open.spotify.com/track/{0}",
                        t["href"].ToString().Replace("spotify:track:", string.Empty)));
                    msg.Message.Done = true;
                    return;
                }
            }
            catch (Exception)
            {
                        
            }
        }
    });
});
