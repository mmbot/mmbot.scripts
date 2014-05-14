

var robot = Require<Robot>();

var octopusUrl = robot.GetConfigVariable("MMBOT_OCTOPUS_URL");
var octopusApiKey = robot.GetConfigVariable("MMBOT_OCTOPUS_APIKEY");
var octopusVerb = robot.GetConfigVariable("MMBOT_OCTOPUS_VERB") ?? "deploy";

bool invalidConfiguration = false;

if(octopusUrl == null) {
    robot.Logger.Error("The MMBOT_OCTOPUS_URL configuration variable is not set");
    invalidConfiguration = true;
}

if(octopusApiKey == null) {
    robot.Logger.Error("The MMBOT_OCTOPUS_APIKEY configuration variable is not set");
    invalidConfiguration = true;
}

if(octopusUrl != null && !octopusUrl.EndsWith("/")) {
    octopusUrl += "/";
}

if(!invalidConfiguration) {

    robot.Respond(string.Format(@"{0} (.+) (\d+\.\d+(\.\d+)*) to (.+)", octopusVerb), msg => {

        var project = msg.Match[1];
        var version = msg.Match[2];
        var environment = msg.Match[4];

        var projectId = GetProjectId(msg, project);

        if(projectId == null){
            msg.Send(string.Format("Could not find Octopus project '{0}'", project));
        }

        var environmentId = GetEnvironmentId(msg, environment);

        if(environmentId == null){
            msg.Send(string.Format("Could not find Octopus environment '{0}'", environment));
        }

        var releaseId = GetReleaseId(msg, projectId, version);

        if(releaseId == null){
            msg.Send(string.Format("There was no release for project '{0}'' matching version '{1}", project, version));
        }

        msg.Http(octopusUrl + "api/deployments")
           .Headers(new Dictionary<string, string>{
                {"X-Octopus-ApiKey", octopusApiKey}
            })
           .Post(
                JToken.FromObject(new {
                    ReleaseId = releaseId,
                    EnvironmentId = environmentId
                }),
                (err, res) => {
                    if(err != null){
                        msg.Send("There was an error starting the deployment. " + err.Message);
                    }
                    if(res.StatusCode != HttpStatusCode.Created){
                        msg.Send("There was an error starting the deployment." + res.StatusCode.ToString());
                    }
                }
            ).Wait();

        msg.Send(string.Format("Started the deployment of {0}, version {1} to the environment {2}",
            project,
            version,
            environment));
    });
}

public string GetProjectId(MMBot.IResponse<TextMessage> msg, string projectName) {

    string projectId = null;

    msg.Http(octopusUrl + "api/projects/all")
       .Headers(new Dictionary<string, string>{
            {"X-Octopus-ApiKey", octopusApiKey}
        })
       .GetJson((err, res, body) => {
            if(err != null){
                msg.Send("There was an issue contacting Octopus. " + err.Message);
                throw err;
            }
            
            projectId = (from proj in body
                        where string.Equals(proj["Name"].ToString(), projectName, StringComparison.InvariantCultureIgnoreCase)
                        select proj["Id"].ToString()).FirstOrDefault();

        }).Wait();

    return projectId;

}

public string GetEnvironmentId(MMBot.IResponse<TextMessage> msg, string environmentName){
    string environmentId = null;

    msg.Http(octopusUrl + "api/environments/all")
       .Headers(new Dictionary<string, string>{
            {"X-Octopus-ApiKey", octopusApiKey}
        })
       .GetJson((err, res, body) => {
            if(err != null){
                msg.Send("There was an issue contacting Octopus. " + err.Message);
                throw err;
            }
            
            environmentId = (from environ in body
                            where string.Equals(environ["Name"].ToString(), environmentName, StringComparison.InvariantCultureIgnoreCase)
                            select environ["Id"].ToString()).FirstOrDefault();

        }).Wait();


    return environmentId;
}

public string GetReleaseId(MMBot.IResponse<TextMessage> msg, string projectId, string version){

    string releaseId = null;

    msg.Http(octopusUrl + string.Format("api/projects/{0}/releases/{1}", projectId, version))
       .Headers(new Dictionary<string, string>{
            {"X-Octopus-ApiKey", octopusApiKey}
        })
       .GetJson((err, res, body) => {
            if(res.StatusCode == HttpStatusCode.NotFound){
                return;
            }
            if(err != null){
                msg.Send("There was an issue contacting Octopus. " + err.Message);
                throw err;
            }
            
            releaseId = body["Id"].ToString();

        }).Wait();

    return releaseId;
}

