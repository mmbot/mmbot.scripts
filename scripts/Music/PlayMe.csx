/**
* <description>
*     Talks to TradeMe's PlayMe 
* </description>
*
* <commands>
*     mmbot whats that song? - asks mmbot what the currently playing song is
* </commands>
* 
* <notes>
*    EXPERIMENTAL - requires changes to PlayMe
* </notes>
* 
* <author>
*     petegoo
* </author>
*/

var robot = Require<Robot>();

var baseUri = robot.GetConfigVariable("MMBOT_PLAYME_URL");

if(baseUri != null && !baseUri.EndsWith("/")){
    baseUri = baseUri + "/";
}

robot.Respond(@"what'?s that song\??", msg => {
    msg.Http(baseUri + "Queue/CurrentTrack")
        .GetJson((err, res, body) => {
        try{
            if(err != null){
                throw err;
            }

            if(body == null || body["Id"] == null){
                msg.Send("Nothin', you're hearing things. Weirdo!");
                return;
            }

            var track = body["Track"]["Name"].ToString();
            var artist = string.Join(", ", body["Track"]["Artists"].Select(a => a["Name"].ToString()));
            var album = body["Track"]["Album"]["Name"].ToString();

            msg.Send(track, artist, album);
        }
        catch(Exception ex){
            msg.Send("Don't know, something went horribly wrong: " + err.Message);
            return;
        }
    });
});
