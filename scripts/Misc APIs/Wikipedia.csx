/**
* <description>
*    Queries wikipedia.org
* </description>
*
* <commands>
*    mmbot wiki me &lt;query&gt; - responds with Wikipedia's most likely page.
* </commands>
* 
* <notes>
*    Ported from https://github.com/github/hubot-scripts/blob/master/src/scripts/wikipedia.coffee
*    Needs work to add the paragraph section.
* </notes>
* 
* <author>
*    tharax
* </author>
*/

using System.Web;

var robot = Require<Robot>();

robot.Respond(@"(wikipedia|wiki)( me)? (.*)$", msg =>
{

	var query = msg.Match[3];

	msg.Http("https://en.wikipedia.org/w/api.php")
	.Query(new Dictionary<string, string>
	{
		{"action", "query"},
		{"format", "json"},
		{"list", "search"},
		{"srlimit", "1"},
		{"srsearch", query}
	})
	.GetJson((err, res, body) =>
	{
		var json = body["query"];
		var link = "";

		if (json["searchinfo"].SelectToken("totalhits").ToString() == "0")
		{
			if (json["searchinfo"].SelectToken("suggestion") == null)
			{
				msg.Send("I can't find anything remotely close to that.");
				return;
			}
			else
			{
				link = json["searchinfo"].SelectToken("suggestion").ToString();
			}
		}
		else
		{
			link = json["search"][0].SelectToken("title").ToString();
		}
		msg.Send(string.Format("http://en.wikipedia.org/wiki/{0}", Uri.EscapeDataString(link)));
	});
});