/**
* <description>
*    Teach mmbot a canned response to a regular expression
* </description>
*
* <commands>
*    mmbot when i say &lt;question&gt; you say &lt;response&gt; - teach mmbot to use a response to a command that matches the question regex;
*    mmbot what did I tell you to say - ask mmbot to list the canned responses;
*    mmbot forget what I told you to say &lt;index&gt; - tell mmbot to forget a canned response;
* </commands>
* 
* <author>
*    petegoo
* </author>
*/

var robot = Require<Robot>();

robot.Respond(@"when i say (.*) you say (.*)", msg => {
	var matcher = msg.Match[1];
	var response = msg.Match[2];

	robot.Respond(matcher, msg2 => msg2.Send(response));

	var savedResponses = robot.Brain.Get<Dictionary<string, string>>("WhenISay").Result ?? new Dictionary<string, string>(); 
	
	savedResponses[matcher] = response;

	robot.Brain.Set("WhenISay", savedResponses);

	msg.Send(msg.Random(new[]{"Got it boss!", "No worries", "I'll try!", "Affirmative", "Let's do it!"}));
});

// Setup the previously saved responses when the script loads
var previouslySavedResponses = robot.Brain.Get<Dictionary<string, string>>("WhenISay").Result ?? new Dictionary<string, string>(); 

foreach(var kvp in previouslySavedResponses){
	robot.Respond(kvp.Key, msg2 => msg2.Send(kvp.Value));
}

robot.Respond(@"what did i tell you to say\??", msg => {
	var savedResponses = robot.Brain.Get<Dictionary<string, string>>("WhenISay").Result ?? new Dictionary<string, string>(); 

	int count = 1;
	var sb = new StringBuilder();

	if(!savedResponses.Any()){
		msg.Send("You haven't told me to say anything yet.");
	}

	foreach(var kvp in savedResponses){
		sb.AppendLine(string.Format("{0}: {1} => {2}", count, kvp.Key,kvp.Value));
		count++;
	}
	msg.Send(sb.ToString());
});

robot.Respond(@"forget what I told you to say (\d+)", msg => {
	var savedResponses = robot.Brain.Get<Dictionary<string, string>>("WhenISay").Result ?? new Dictionary<string, string>(); 

	int i = int.Parse(msg.Match[1]);

	if(i > savedResponses.Count()){
		msg.Send("Don't have " + i + " saved responses");
		return;
	}

	var regex = savedResponses.Keys.ElementAt(i-1);
	savedResponses.Remove(regex);

	robot.RemoveListener(regex);

	robot.Brain.Set("WhenISay", savedResponses);
	msg.Send(msg.Random(new[]{"forgotten boss", "forget what ;)", "consider it done"}));


});