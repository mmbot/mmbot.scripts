/**
* <description>
*     Define terms via Urban Dictionary.
* </description>
*
* <configuration>
*
* </configuration>
*
* <commands>
*     hubot what is &lt;term&gt;?         - Searches Urban Dictionary and returns definition;
*     hubot urban me &lt;term&gt;         - Searches Urban Dictionary and returns definition;
*     hubot urban define me &lt;term&gt;  - Searches Urban Dictionary and returns definition;
*     hubot urban example me &lt;term&gt; - Searches Urban Dictionary and returns example;
* </commands>
* 
* <notes>
*     Ported from https://github.com/github/hubot-scripts/blob/master/src/scripts/urban.coffee
* </notes>
* 
* <author>
*     petegoo
* </author>
*/


var robot = Require<Robot>();

robot.Respond(@"what ?is ([^\?]*)[\?]*", msg =>
{
    string query = msg.Match[1];

    msg.Http(string.Format("http://api.urbandictionary.com/v0/define?term={0}", query)).GetJson((err, res, body) => {
        if (body["list"].ToArray().Count() == 0)
        {
            msg.Send("I don't know what \"" + query + "\" is");
            return;
        }

        var entry = body["list"][0];
        msg.Send((string)entry["definition"]);

        //var sounds = res.sounds;
        //if (sounds != null && sounds.Count != 0)
        //{
        //    await msg.Send(string.Join(" ", ((JArray) sounds).Select(s => s.ToString())));
        //}
    });
});


robot.Respond(@"(urban)( define)?( example)?( me)? (.*)", msg =>
{
    string query = msg.Match[5];

    msg.Http(string.Format("http://api.urbandictionary.com/v0/define?term={0}", query)).GetJson((err, res, body) => {

        if (body["list"].ToArray().Count() == 0)
        {
            msg.Send("\"" + query + "\" not found");
            return;
        }
        var entry = body["list"][0];
        if (!string.IsNullOrWhiteSpace(msg.Match[3]))
        {
            msg.Send((string)entry["example"]);
        }
        else
        {
            msg.Send((string)entry["definition"]);
        }
        //var sounds = res.sounds;
        //if (sounds != null && sounds.Count != 0)
        //{
        //    await msg.Send(string.Join(" ", ((JArray)sounds).Select(s => s.ToString())));
        //}
    });
});

    