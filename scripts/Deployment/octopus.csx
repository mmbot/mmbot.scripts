/**
* <description>
*    Manages deployments in Octopus Deploy.
* </description>
*
* 
* <notes>
*    Requires an install of Octopus Deploy that can be reached over HTTP  
*    Allows for querying Octopus info and triggering builds.
*    You need to get an Api key from your profile page in the Octopus Portal
* </notes>
* 
* <commands>
*     mmbot get octopus environments - List of environments;
*     mmbot get octopus environment Foo - Get details of the 'Foo' environment;
*     mmbot get octopus projects - List of projects;
*     mmbot get octopus project Bar - Get details of the 'Bar' project;
*     mmbot what is deployed - Get details of the deployments in each environment and project;
*     mmbot what is deployed to Prod - Get details of the deployments in the Prod environment;
*     mmbot deploy My Project 1.0.1 to Prod - Deploy the 1.0.1 release of My Project to the production environment;
* </commands>
*
* <author>
*    petegoo
* </author>
*
* <configuration>
*    MMBOT_OCTOPUS_URL;
*    MMBOT_OCTOPUS_APIKEY;
* </configuration>
*/

var robot = Require<Robot>();

// Get and validate config
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

    // Show a list of environments
    robot.Respond(@"(show|get) octo(pus)?(\s|-)?environments", msg => {
        Octopus(msg, "/api/environments/all")
            .GetJson((err, res, body) => {
                if(err != null) {
                    msg.Send("There was an issue contacting Octopus. " + err.Message);
                    throw err;
                }

                var names = string.Join("\r\n", 
                                from environment in body
                                select environment["Name"].ToString());

                msg.Send(names);
            });
    });

    // Show a specific environment
    robot.Respond(@"(show|get) octo(pus)?(\s|-)?environment (.*)", msg => {

        var environment = GetEnvironment(msg, msg.Match[4]);

        if(environment == null){
            msg.Send(string.Format("Could not find Octopus environment '{0}'", msg.Match[4]));
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Id: "            + environment["Id"].ToString());
        sb.AppendLine("Name: "          + environment["Name"].ToString());
        sb.AppendLine("Description: "   + environment["Description"].ToString());
        sb.AppendLine("Last Modified: " + environment["LastModifiedBy"].ToString() + " (" + environment["LastModifiedOn"].ToString() + ")");

        msg.Send(sb.ToString());
    });

    // Show a list of projects
    robot.Respond(@"(show|get) octo(pus)?(\s|-)?projects", msg => {
        Octopus(msg, "/api/projects/all")
            .GetJson((err, res, body) => {
                if(err != null) {
                    msg.Send("There was an issue contacting Octopus. " + err.Message);
                    throw err;
                }

                var names = string.Join("\r\n", 
                                from project in body
                                select project["Name"].ToString());

                msg.Send(names);
            });
    });

    // Show a specific project
    robot.Respond(@"(show|get) octo(pus)?(\s|-)?project (.*)", msg => {
        var project = GetProject(msg, msg.Match[4]);

        if(project == null){
            msg.Send(string.Format("Could not find Octopus project '{0}'", msg.Match[4]));
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Id: "            + project["Id"].ToString());
        sb.AppendLine("Name: "          + project["Name"].ToString());
        sb.AppendLine("Description: "   + project["Description"].ToString());
        sb.AppendLine("Disabled: "      + project["IsDisabled"].ToString());
        sb.AppendLine("Last Modified: " + project["LastModifiedBy"].ToString() + " (" + project["LastModifiedOn"].ToString() + ")");

        msg.Send(sb.ToString());

    });

    // Show what is deployed right now
    robot.Respond(@"what is deployed( to ([^\?]*))?(\?)?", msg => {

        var environmentFilter = msg.Match[2];

        Octopus(msg, "api/dashboard")
        .GetJson((err, res, body) => {
            if(err != null){
                msg.Send("There was an issue contacting Octopus. " + err.Message);
                throw err;
            }            

            var dashboard = string.Join("\r\n",
                               (from environment in body["Environments"]
                                from project in body["Projects"] 
                                from item in body["Items"]
                                where item["EnvironmentId"].ToString() ==  environment["Id"].ToString()
                                   && item["ProjectId"].ToString() ==  project["Id"].ToString()
                                   && (string.IsNullOrEmpty(environmentFilter) || string.Equals(environmentFilter, environment["Name"].ToString(), StringComparison.InvariantCultureIgnoreCase))
                                select new {
                                    Environment = environment["Name"].ToString(),
                                    Release = string.Format("  {0} : {1} - {2} - {3}", 
                                        project["Name"].ToString(),
                                        item["ReleaseVersion"].ToString(),
                                        item["State"].ToString(),
                                        item["CompletedTime"].ToString())
                                })
                                .GroupBy(s => s.Environment)
                                .Select(g => g.Key + "\r\n" + string.Join("\r\n", g.Select(i => i.Release))));

            if(string.IsNullOrWhiteSpace(dashboard)){
                if(!string.IsNullOrEmpty(environmentFilter) && !body["Environments"].ToArray().Any(e => string.Equals(e["Name"].ToString(), environmentFilter, StringComparison.InvariantCultureIgnoreCase))){
                    msg.Send(string.Format("No environments found matching the name '{0}'", environmentFilter));
                    return;
                }
                msg.Send("No deployments found");
            }
            msg.Send(dashboard);
        });        
    });

    // Start a deployment
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

        string taskUrl = null;
        Octopus(msg, "api/deployments")
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

                    taskUrl = ((JToken)res.Json().Result)["Links"]["Task"].ToString();
                }
            ).Wait();

        if(taskUrl == null){
            return;
        }

        msg.Send(string.Format("Started the deployment of {0}, version {1} to the environment {2}",
            project,
            version,
            environment));

        Task.Run(() => {
            try {
                bool complete = false;
                int count = 0;
                int errorCount = 0;
                List<string> consumedActivityLogs = new List<string>();
                string result = "";
                string taskWebUrl = "";

                while(!complete && count < 432000 && errorCount < 50){
                    count++;
                    Task.Delay(1000).Wait();
                    Octopus(msg, taskUrl.TrimStart('/') + "/details?verbose=false")
                            .GetJson((err, res, body) => {
                                if(err != null || res.StatusCode != HttpStatusCode.OK){
                                    errorCount++;
                                }
                                else {                                    
                                    try{
                                        var activityLogs = FlattenActivityLog(body["ActivityLog"]).Except(consumedActivityLogs).ToArray();
                                        consumedActivityLogs.AddRange(activityLogs);

                                        if(activityLogs.Any()){
                                            msg.Send(activityLogs.ToArray());

                                            msg.Send("   " + body["Progress"]["EstimatedTimeRemaining"].ToString());
                                        }

                                        taskWebUrl = body["Links"]["Web"].ToString();

                                        if(body["Task"]["IsCompleted"].ToString().ToLower() == "true"){
                                            complete = true;
                                            result = body["Task"]["State"].ToString();
                                        }
                                    }
                                    catch(Exception ex){
                                        errorCount += 10;
                                        robot.Logger.Error(ex);
                                    }
                                }
                            }).Wait();
                }

                if(!complete){
                    msg.Send("Could not continue tracking deployment. Please go to the Octopus server portal\r\n" + octopusUrl + taskWebUrl.TrimStart('/'));
                }
                else{
                    msg.Send(string.Format("The deployment of {0}, version {1} to the environment {2} is complete.\r\nThe result was {3}\r\n{4}",
                        project,
                        version,
                        environment,
                        result,
                        octopusUrl + taskWebUrl.TrimStart('/')));
                }
            }
            catch(Exception ex){
                robot.Logger.Error(ex);
            }
        });
    });
}

public IEnumerable<string> FlattenActivityLog(JToken rootLogItem){
    return new string[]{ string.Format("{0}:{1}", 
        rootLogItem["Status"].ToString(),
        rootLogItem["Name"].ToString()
         ) }
        .Concat(
            rootLogItem["Children"].ToArray()
                .Where(child => child["ShowAtSummaryLevel"].ToString().ToLower() == "true")
                .SelectMany(child => FlattenActivityLog(child)).ToArray());

}

public JToken GetProject(MMBot.IResponse<TextMessage> msg, string projectName) {

    JToken project = null;

    Octopus(msg, "api/projects/all")
       .GetJson((err, res, body) => {
            if(err != null){
                msg.Send("There was an issue contacting Octopus. " + err.Message);
                throw err;
            }
            
            project = (from proj in body
                        where string.Equals(proj["Name"].ToString(), projectName, StringComparison.InvariantCultureIgnoreCase)
                        select proj).FirstOrDefault();

        }).Wait();

    return project;
}

public string GetProjectId(MMBot.IResponse<TextMessage> msg, string projectName) {

    var project = GetProject(msg, projectName);

    return project == null ? null : project["Id"].ToString();
}

public HttpWrapper Octopus(MMBot.IResponse<TextMessage> msg, string apiPath) {
    return msg.Http(octopusUrl + apiPath)
       .Headers(new Dictionary<string, string>{
            {"X-Octopus-ApiKey", octopusApiKey}
        });
}

public JToken GetEnvironment(MMBot.IResponse<TextMessage> msg, string environmentName){
    JToken environment = null;

    Octopus(msg, "api/environments/all")
       .GetJson((err, res, body) => {
            if(err != null){
                msg.Send("There was an issue contacting Octopus. " + err.Message);
                throw err;
            }
            
            environment = (from environ in body
                           where string.Equals(environ["Name"].ToString(), environmentName, StringComparison.InvariantCultureIgnoreCase)
                           select environ).FirstOrDefault();

        }).Wait();

    return environment;
}

public string GetEnvironmentId(MMBot.IResponse<TextMessage> msg, string environmentName){
    string environmentId = null;

    var environment = GetEnvironment(msg, environmentName);

    return environment == null ? null : environment["Id"].ToString();
}

public string GetReleaseId(MMBot.IResponse<TextMessage> msg, string projectId, string version){

    string releaseId = null;

    Octopus(msg, string.Format("api/projects/{0}/releases/{1}", projectId, version))
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

