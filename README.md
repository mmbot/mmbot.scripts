mmbot.scripts
=============

This is the script catalog for [mmbot](https://github.com/mmbot/mmbot) the C# port of of GitHub's Hubot automation bot. 

[Browse the catalog](http://mmbot.github.io/mmbot.scripts/catalog.html)

## Installing Scripts
After setting up mmbot you will start with the core scripts which includes MMBotScripts.  This script gives you the ability to query and download from the catalog.  Additionally, the Help script gives you the ability to view installed scripts, their details and associated commands.  These are the associated commands from the MMBotScripts and Help scripts.

### Scripts from the mmbot.scripts Catalog:
`mmbot scripts (query) [detailed] - lists scripts in the mmbot Scripts repository filtered by (query)`

`mmbot download script (name) - downloads a script by (name) from the mmbot Scripts repository`

### Loaded Scripts:
`mmbot list scripts - Displays a list of all the loaded script files`

`mmbot help - Displays all of the help commands that mmbot knows about.`

`mmbot help <query> - Displays all help commands that match <query>.`

`mmbot man <query> - Displays the details for the script that matches <query>.`

Downloading a script loads the script into mmbot as well so it can be immediately used.

Verify any requirements by reviewing the detailed information of the script with either `mmbot scripts (query) detailed` or `mmbot man <query>` if already loaded.  Requirements may require you to install a package using nuget or scriptcs before the script can be used.

## Contributing
To contribute a new script or fix an existing one:
* Raise an issue and tag it with either "new script" or "bug"
* Fork the repo
* Submit a Pull Request

## Updating the Catalog
To update the catalog just push the changed scripts to master. Thanks to the good people at JetBrains and CodeBetter the CI server will re-generate the catalog for us :)