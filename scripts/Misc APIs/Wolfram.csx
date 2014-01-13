/**
* <description>
*    Queries wolfram alpha
* </description>
*
* <configuration>
*    MMBOT_WOLFRAM_APPID
* </configuration>
* <commands>
*    mmbot question (question) - Queries wolfram alpha for the answer to the question
* </commands>
* 
* <notes>
*    Ported from https://github.com/github/hubot-scripts/blob/master/src/scripts/wolfram.coffee
* </notes>
* 
* <author>
*    jamessantiago
* </author>
*/

var robot = Require<Robot>();

robot.Respond(@"(question|wfa) (detailed)?(.*)$", msg =>
{
    var appId = robot.GetConfigVariable("MMBOT_WOLFRAM_APPID");
    var isDetailed = msg.Match[2].HasValue();
    var question = msg.Match[3];
    
    if (!appId.HasValue())
    {
	msg.Send("Key MMBOT_WOLFRAM_APPID must have a value first");
    }
    
    msg.Http("http://api.wolframalpha.com/v2/query")
       .Query(new
        {
            appid = appId,
            input = question,
	    	format = "plaintext"
        })
	.Headers(new Dictionary<string, string>
        {
            {"Accept-Language", "en-us,en;q=0.5"},
            {"Accept-Charset", "utf-8"},
            {"User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:2.0.1) Gecko/20100101 Firefox/4.0.1"}
        })
	.GetXml((err, res, body) => {
		if (err != null)
		{
			msg.Send("Unable to query wolfram");
		}
		else
		{			
			var nodes = body.SelectNodes("/queryresult/pod");
			bool resultSent = false;		
			foreach (XmlNode node in nodes)
			{
			    var title = node.Attributes["title"].Value;
			    if ((!isDetailed && title.ToLower() == "result") || isDetailed)
			    {
				    var subnode = node.FirstChild;
				    if (subnode != null)
				    {
				    	var data = subnode.FirstChild != null && subnode.FirstChild.FirstChild != null ? subnode.FirstChild.FirstChild.Value : "";
				    	msg.Send(string.Format("{0} : {1}", title, data));
				    	resultSent = true;
				    }
				}
			}
			if (!resultSent)
			{
				msg.Send("Could not understand question");
			}
		}
	});
});