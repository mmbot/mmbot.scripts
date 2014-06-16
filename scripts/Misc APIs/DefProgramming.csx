/**
* <description>
*     Displays a random quote from def programming
* </description>
*
* <configuration>
*
* </configuration>
*
* <commands>
*     mmbot def programming - returns a random programming quote;
* </commands>
* 
* <notes>
*     ported from https://github.com/github/hubot-scripts/blob/master/src/scripts/defprogramming.coffee
* </notes>
* 
* <author>
*     jamessantiago
* </author>
*/

var robot = Require<Robot>();
private _rand = new Random();

robot.Respond(@"def programming", msg =>
{
    msg.Http(string.Format("http://www.defprogramming.com/random/?r={0}", _rand.Next()))
        .GetHtml((err, res, htmlDoc) => {
            if (err != null)
            {
                msg.Send("Could not retrieve");
            }
            else
            {
                try {
                    var quote = htmlDoc.DocumentNode.SelectNodes("//q[@class='jumbotron']/p").First().FirstChild.InnerText;
                    var author = htmlDoc.DocumentNode.SelectNodes("//span[@class='quote-highlight-author-name']").First().FirstChild.InnerText;
                    var topic = htmlDoc.DocumentNode.SelectNodes("//span[@class='quote-highlight-tags']/a").First().FirstChild.InnerText;
                    msg.Send(string.Format("\"{0}\" - {1} on {2}", quote, author, topic));
                }
                catch
                {
                    msg.Send("Could not retrieve");
                }
            }
            
    });
});