using System.Text.RegularExpressions;
 
var robot = Require<Robot>();
 
robot.Router.Get("/announce/{adapterId}/{room}", context => {
    Announce(context);
});

robot.Router.Post("/announce/{adapterId}/{room}", context => {
    Announce(context);
});

void Announce(Microsoft.Owin.IOwinContext context) {
    try {
        robot.Logger.Info("Got an announce webhook call!!");
        var adapterId = context.Request.Params()["adapterId"];
        var room = context.Request.Params()["room"];
        var message = context.Request.Query["message"];
    
        if(message != null){
            // We were given a message in the querystring so process it
            var announcement = ProcessMessage(message, GetRequestParameters(context));
            robot.Speak(adapterId, room, announcement);
        }
    }
    catch(Exception ex) {
        robot.Logger.Error("Error parsing announce script", ex);
    }
}
 
 
string ProcessMessage(string message, IDictionary<string, string> parameters) {
    var parameterMatches = Regex.Matches(message, @"[^{}]+(?=\})");
    foreach(Match match in parameterMatches) {
        string value;
        if(parameters.TryGetValue(match.Value.TrimStart('{').TrimEnd('}'), out value)) {
            message = message.Replace("{" + match.Value + "}", value);
        }
        
    }
    return message;
}

IDictionary<string, string> GetRequestParameters(Microsoft.Owin.IOwinContext context) {
    var query = context.Request.Query.ToDictionary(kvp => kvp.Key, kvp => string.Join("", kvp.Value));

    var contentType = (context.Request.ContentType ?? string.Empty).ToLower();

    if(contentType == "application/x-www-form-urlencoded") {
        return context.Form().ToDictionary(kvp => kvp.Key, kvp => string.Join("", kvp.Value))
               .Concat(query).ToDictionary(kvp => kvp.Key, kvp =>kvp.Value);
    }

    if(contentType == "application/json") {
        var json = context.ReadBodyAsJson();
        if(json is JObject){
            return ((JObject)json).Properties().ToDictionary(p => p.Name, p => p.Value.ToString())
               .Concat(query).ToDictionary(kvp => kvp.Key, kvp =>kvp.Value);
        }
    }
    return query;
}