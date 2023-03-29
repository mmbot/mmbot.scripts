/**
* <description>
*    Announces the Football World Cup   scores
* </description>
*
* <commands>
*    mmbot show all the scores - Show all the football World Cup 2014 scores.;
*    mmbot stream live scores on/off - Switch on/off streaming of live scores into the current room.;
* </commands>
* 
* <notes>
*    Requires some changes made in mmbot version 0.9.8 so you may have to update
* </notes>
* 
* <author>
*    petegoo
* </author>
*/

var robot = Require<Robot>();

var streamRooms = robot.Brain.Get<List<Tuple<string,string>>>("futbol_live_streams").Result ?? new List<Tuple<string, string>>();

if(streamRooms.Count() > 0){
    robot.Logger.Info("Found " + streamRooms.Count() + " world cup streaming rooms");
    RefreshLiveStream();
}

var liveScores = new Dictionary<int, string>();

var apiUrl = "http://live.mobileapp.fifa.com/api/wc/matches";

static Random _random = new Random(DateTime.Now.Millisecond);

IDisposable scheduler;

robot.Respond(@"show all the scores", msg => {

    msg.Http(apiUrl).GetJson((err, res, body) => {
            var groups = GetMatches().Where(m => m["b_Started"].Value<bool>()).GroupBy(m => m["c_Phase_en"].ToString());
            
            if(!groups.Any()){
                return;
            }

            var sb = new StringBuilder();
            foreach(var group in groups){
                sb.AppendLine(group.Key);
                foreach(var match in group){
                    sb.AppendLine(" " + GetScore(match));
                }
                sb.AppendLine();
            }

            msg.Send(sb.ToString().Trim());
    });
});

robot.Respond(@"stream live scores on", msg => {
    if(!streamRooms.Any(s => s.Item1.ToLower() == msg.Message.User.AdapterId && s.Item2.ToLower() == msg.Message.User.Room)) {
        streamRooms.Add(Tuple.Create(msg.Message.User.AdapterId, msg.Message.User.Room));
        robot.Brain.Set("futbol_live_streams", streamRooms);
    }
    RefreshLiveStream();
    msg.Send("Live scores will now be reported as they happen in this room");
});

robot.Respond(@"stream live scores off", msg => {
    var subscription = streamRooms.FirstOrDefault(s => s.Item1.ToLower() == msg.Message.User.AdapterId.ToLower() && s.Item2.ToLower() == msg.Message.User.Room.ToLower());
    if(subscription == null) {
        msg.Send("Not live streaming scores");
        return;
    }

    streamRooms.Remove(subscription);
    robot.Brain.Set("futbol_live_streams", streamRooms);
    RefreshLiveStream();
    msg.Send("Live scores will no longer be reported in this room");
});

robot.RegisterCleanup(() => {if(scheduler != null) {scheduler.Dispose(); } });

public void RefreshLiveStream() {
    if(!streamRooms.Any() && scheduler != null) {
        // Make sure we are not streaming
        scheduler.Dispose();
        scheduler = null;
        return;
    }

    scheduler = robot.ScheduleRepeat(TimeSpan.FromSeconds(5), CheckScores);
}

public void CheckScores() {
    var currentScores = GetMatches().Where(m => m["b_Live"].Value<bool>());
    foreach(var match in currentScores) {
        var matchId = match["n_MatchID"].Value<int>();

        var scoreSummary = string.Format("{0}:{1} {2}", 
            match["n_HomeGoals"].ToString(), 
            match["n_AwayGoals"].ToString(), 
            match["b_Finished"].Value<bool>() ? "full time" : string.Empty
        );

        if(!liveScores.ContainsKey(matchId)) {
            // Report match started
            AnnounceLiveStreamEvent(string.Format("Match just started! - {0} vs {1}", match["c_HomeTeam_en"], match["c_AwayTeam_en"]));

            if(match["n_HomeGoals"].Value<int>() > 0 || match["n_AwayGoals"].Value<int>() > 0) {
                // We already have a score so announce it
                AnnounceLiveStreamGoal(GetScore(match));
            }
            liveScores.Add(matchId, scoreSummary);
            return;
        }

        if(liveScores[matchId] != scoreSummary) {
            AnnounceLiveStreamGoal(GetScore(match));
            liveScores[matchId] = scoreSummary;
        }
    }
}

public void AnnounceLiveStreamGoal(string message){
    AnnounceLiveStreamEvent("Goal!!!!\r\n" + message + "\r\n" + goalGifs.ElementAt(_random.Next(goalGifs.Count())));
}

public void AnnounceLiveStreamEvent(string message) {
    foreach(var sub in streamRooms ) {
        robot.Speak(sub.Item1, sub.Item2, message);
    }
}

public IEnumerable<JToken> GetMatches() {
    var result = robot.Http(apiUrl).GetJson().Result;
    return ((JToken)result)["data"]["group"];
}

public string GetScore(JToken match){
    return string.Format("{0} {1}:{2} {3} - {4} vs {5} ({6})", 
        match["c_HomeNatioShort"].ToString(), 
        match["n_HomeGoals"].ToString(), 
        match["n_AwayGoals"].ToString(), 
        match["c_AwayNatioShort"].ToString(),
        match["c_HomeTeam_en"].ToString(),
        match["c_AwayTeam_en"].ToString(),
        match["b_Finished"].Value<bool>() ? "full time" : match["c_Minute"].ToString() + " minutes"
        );
}


var goalGifs = new string[]{
    "http://media.giphy.com/media/ATY37AqcQR65q/giphy.gif",
    "http://media.giphy.com/media/IyDY4BgeQJqRG/giphy.gif",
    "http://media.giphy.com/media/FJQ4PNoOjojx6/giphy.gif",
    "http://media.giphy.com/media/102VWyoe7XoapW/giphy.gif",
    "http://media.giphy.com/media/HgQ6BSpDU7hS0/giphy.gif",
    "http://media.giphy.com/media/1nuHv3fsNT4Va/giphy.gif",
    "http://media.giphy.com/media/lP620Df2JjN4c/giphy.gif",
    "http://media.giphy.com/media/lazywk9H243NS/giphy.gif",
    "http://media2.giphy.com/media/novTohzhl4pu8/giphy.gif",
    "http://media.giphy.com/media/z16yViTXsJGM0/giphy.gif",
    "http://media.giphy.com/media/2yE9Pngru1APu/giphy.gif",
    "http://media.giphy.com/media/wNHm5hX2lwki4/giphy.gif",
    "http://media1.giphy.com/media/qxIGPz6UWsEzC/giphy.gif",
    "http://media.giphy.com/media/eJJUSMw3yAWHe/giphy.gif",
    "http://media.giphy.com/media/Vx4njfM5eRPtm/giphy.gif",
    "http://media3.giphy.com/media/SqJfmCsBgMlEs/200.gif",
    "http://media.giphy.com/media/ZU4RfkaryvodO/giphy.gif"
};
