/**
* <description>
*    Allows annoucements to be made to a room by calling a URL.
* </description>
*
* 
* <notes>
*    Requires a router to be configured. 
*    Send requests to e.g. /announce/JabbrAdapter/MyRoom/?message=Hello%20Room
*    Will allow replacement params in the message text:
*      e.g. send request to /announce/JabbrAdapter/MyRoom/?message=Hello%20{name}
*      the script will look for a "name" parameter in the querystring, form body or json body
* </notes>
* 
* <author>
*    petegoo
* </author>
*/
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
    //robot.Logger.Info("Webhook Parameters:\r\n" + string.Join("\r\n", parameters.Select(kvp => "    " + kvp.Key + ":" + kvp.Value )));
    var parameterMatches = Regex.Matches(message, @"[^{}]+(?=\})");
    foreach(Match match in parameterMatches) {
        //robot.Logger.Info("Found parameter " + match.Value);
        string value;
        if(parameters.TryGetValue(match.Value.TrimStart('{').TrimEnd('}'), out value)) {
            //robot.Logger.Info("replacing parameter " + match.Value + " with " + value );
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
            return GenerateJsonDictionary((JObject)json);
        }
    }
    return query;
}

private IDictionary<string, string> GenerateJsonDictionary(JObject parent, string prefix = null)
{
    if (!string.IsNullOrEmpty(prefix))
    {
        prefix = prefix + ".";
    }
    else
    {
        prefix = string.Empty;
    }

    var props = parent.Properties().Where(p => p.Value is JValue).Select(jp => new KeyValuePair<string, string>(string.Concat(prefix, jp.Name), jp.Value.ToString()));

    return props
        .Concat(
            parent.Properties().Where(p => p.Value is JObject)
            .SelectMany(p => GenerateJsonDictionary(p.Value as JObject, string.Concat(prefix, p.Name))))
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
}