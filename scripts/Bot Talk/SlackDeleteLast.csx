/**
* <description>
*     Deletes the last message posted by the bot in the current Slack channel. Useful for those special gifs it finds.
* </description>
*
* <commands>
*     mmbot undo|abort|delete|remove - Delete the last message posted by the bot in the current Slack channel. 
* </commands>
* 
* <author>
*     petegoo
* </author>
*
* <configuration>
*    MMBOT_SLACK_USERTOKEN;
* </configuration>
*/

var robot = Require<Robot>();

robot.Respond(@"(undo|abort|delete|remove)", msg => {

    msg.Http("https://slack.com/api/search.messages")
       .Query(new {
            token = robot.GetConfigVariable("MMBOT_SLACK_USERTOKEN"),
            query = string.Format("in:{0} from:{1}", msg.Message.User.Room, robot.Name),
            sort = "timestamp",
            count = 10,
            pretty = 1
        })
       .GetJson((err, res, body) => {
            try{
                if(err != null || !body["ok"].Value<bool>()){
                    msg.Send("Something went wrong. " + (body != null ? body["error"] : ""));
                    return;
                }
                var matches = body["messages"]["matches"];

                DeleteLastMessage(msg, matches.Children());
            }
            catch(Exception ex){
                msg.Send(ex.ToString());
            }
        });   
});

void DeleteLastMessage(MMBot.IResponse<TextMessage> msg, IEnumerable<JToken> matches){

    var match = matches.First();
    var timestamp = match["ts"].ToString();
    var channelId = match["channel"]["id"].ToString();

    msg.Http("https://slack.com/api/chat.delete")
       .Query(new {
            token = robot.GetConfigVariable("MMBOT_SLACK_USERTOKEN"),
            ts = timestamp,
            channel = channelId
        })
       .GetJson((err2, res2, body2) => {
            if(err2 != null || !body2["ok"].Value<bool>()){
                DeleteLastMessage(msg, matches.Skip(1));
            }
        });
}
