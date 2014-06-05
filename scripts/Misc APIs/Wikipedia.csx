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
			text = GetFirstDecentParagraph(body2.DocumentNode.InnerHtml);
			msg.Send(address);
			msg.Send(text);
		});
	});
});

private string GetFirstDecentParagraph(string htmlDump) {
	string text = "";
	int paragraphCounter = 0;
	while (text.Length < 140) { //Because if twitter can do it, so can I.
		text = htmlDump.Substring(htmlDump.IndexOf("<p>", paragraphCounter), htmlDump.IndexOf("</p>", paragraphCounter) - htmlDump.IndexOf("<p>", paragraphCounter));
		paragraphCounter = htmlDump.IndexOf("</p>", paragraphCounter) + 1;
		text = Regex.Replace(text, "<[^>]*>", string.Empty);
	}
	return text;
}
