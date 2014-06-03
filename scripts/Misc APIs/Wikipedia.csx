/*
* <description>
*    Queries wikipedia.org
* </description>
*
* <commands>
*    mmbot wiki me <query> - responds with Wikipedia's most likely page.
* </commands>
* 
* <notes>
*    Ported from https://github.com/github/hubot-scripts/blob/master/src/scripts/wikipedia.coffee
* </notes>
* 
* <author>
*    tharax
* </author>
*/
using System.Text.RegularExpressions;

var robot = Require<Robot>();

robot.Respond(@"(wikipedia|wiki)( me)? (.*)$", msg =>
{

	var query = msg.Match[3];
	string address = null;
	string text = null;

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
		address = string.Format("http://en.wikipedia.org/wiki/{0}", link.Replace(" ", "_"));
		msg.Http(address).GetHtml((err2, res2, body2) => {
			text = body2.DocumentNode.InnerHtml;
			text = text.Substring(text.IndexOf("<p>"), text.IndexOf("</p>") - text.IndexOf("<p>"));
			text = Regex.Replace(text, "<{1}.*?>{1}", "");
			msg.Send(address);
			msg.Send(text);
		});
	});
});