/**
* <description>
*     Executes a powershell command
* </description>
*
* <commands>
*     mmbot rate from &lt;currency&gt; to &lt;currency&gt; - Gets current exchange rate between two currencies.;
* </commands>
* 
* <notes>
*    Requires the MMBot.Powershell nuget package;
*    Output objects must either support a ToString method or be a string to display properly;
*    It is recommended to use the PowershellModule script instead of this one to control what is executed;
* </notes>
* 
* <author>
*     jamessantiago
* </author>
*/

using MMBot.Powershell;

var robot = Require<Robot>();

robot.Respond(@"(ps|powershell) (.*)", msg =>
{
    var command = msg.Match[2];
    try
    {
        foreach (string result in robot.ExecutePowershellCommand(command))
	{
	    msg.Send(result);
	}	
    }
    catch (Exception)
    {
        msg.Send("erm....issues, move along");
    }
    
});