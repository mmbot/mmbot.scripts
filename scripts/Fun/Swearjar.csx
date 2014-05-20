/**
* <description>
*     Monitors for swearing
* </description>
*
* <configuration>
*
* </configuration>
*
* <commands>
*     mmbot swearjar - shows the swear stats of users
* </commands>
* 
* <author>
*     dkarzon
* </author>
*/

var robot = Require<Robot>();

robot.Respond("swearjar", msg =>
{
    var swearBrain = robot.Brain.Get<List<SwearUser>>("swearbot").Result ?? new List<SwearUser>();

    foreach (var s in swearBrain)
    {
        if (s.Swears == null) continue;

        msg.Send(string.Format("{0} has sworn {1} times!", s.User, s.Swears.Count));
    }
});

robot.Hear(@".*(fuck|shit|cock|crap).*", (msg) => 
{
    //Someone said a swear!
    var theSwear = msg.Match[1];
    var userName = msg.Message.User.Name;

    //load the swears from the Brain
    var swearBrain = robot.Brain.Get<List<SwearUser>>("swearbot").Result ?? new List<SwearUser>();

    var swearUser = swearBrain.FirstOrDefault(s => s.User == userName);
    if (swearUser == null)
    {
        swearUser = new SwearUser { User = userName };
        swearBrain.Add(swearUser);
    }
    if (swearUser.Swears == null)
    {
        swearUser.Swears = new List<SwearStat>();
    }
    swearUser.Swears.Add(new SwearStat{Swear = theSwear, Date = DateTime.Now});

    robot.Brain.Set("swearbot", swearBrain);

    msg.Send("SWEAR JAR!");
});

public class SwearStat
{
    public string Swear { get; set; }
    public DateTime Date { get; set; }
}
public class SwearUser
{
    public string User { get; set; }
    public List<SwearStat> Swears { get; set; }
}