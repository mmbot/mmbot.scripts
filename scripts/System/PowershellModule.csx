/**
* <description>
*    Executes a powershell script
* </description>
*
* <configuration>
*    MMBOT_POWERSHELL_SCRIPTSPATH
* </configuration>
*
* <commands>
*    mmbot psm (script) (commands)
* </commands>
* 
* <notes>
*     Requires the MMBot.Powershell nuget package;
*     Specify the powershell scripts folder using the MMBOT_POWERSHELL_SCRIPTSPATH key in the ini file;
*     Powershell scripts must be .psm1 (modules) to be executed;
*     Only scripts inside the scripts folder may be executed using this script;
*     Output objects must either support a ToString method or be a string to display properly;
* </notes>
* 
* <author>
*     jamessantiago
* </author>
*/



using MMBot.Powershell;
var robot = Require<Robot>();

robot.Respond(@"(psm|powershell module) (.*)", msg =>
{
    var command = msg.Match[2];
    try
    {
        foreach (string result in robot.ExecutePowershellModule(command))
	{
	    msg.Send(result);
	}
    }
    catch (Exception)
    {
        msg.Send("erm....issues, move along");
    }
    
});