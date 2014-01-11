---
layout: default
title: MMBot Script Catalog
---

# MMBot Scripts

## AchievementUnlocked

### Description
Creates an anchievement image with user's gravatar pic and achievement text

### Configuration


### Commands
`mmbot achievement <achievement> [achiever's gravatar email]`
`mmbot award <achievement> [achiever's gravatar email]`

### Notes
Ported from https://github.com/github/hubot-scripts/blob/master/src/scripts/achievement_unlocked.coffee

### Author
dkarzon

### Download Link
[Download AchievementUnlocked](http://petegoo.github.io/mmbot.scripts/scripts/Fun/AchievementUnlocked.csx)


## Ascii

### Description
Creates an ascii art representation of input text

### Configuration


### Commands
`mmbot ascii me <query> - Returns ASCII art of the query text.`

### Notes
Ported from https://github.com/rbwestmoreland/Jabbot/blob/master/Jabbot.Sprockets.Community/AsciiSprocket.cs

### Author
dkarzon

### Download Link
[Download Ascii](http://petegoo.github.io/mmbot.scripts/scripts/Fun/Ascii.csx)


## Cats

### Description
Brings cats

### Configuration


### Commands
`mmbot cat me <number> - Returns a number of cat pictures.`
`mmbot cat me - Returns a cat picture.`
`mmbot cat gif <number> - Returns a number of cat gifs.`
`mmbot cat gif - Returns a cat gif.`

### Notes


### Author
dkarzon

### Download Link
[Download Cats](http://petegoo.github.io/mmbot.scripts/scripts/Images/Cats.csx)


## DefProgramming

### Description
Displays a random quote from def programming

### Configuration


### Commands
`mmbot def programming - returns a random programming quote`

### Notes
ported from https://github.com/github/hubot-scripts/blob/master/src/scripts/defprogramming.coffee

### Author
jamessantiago

### Download Link
[Download DefProgramming](http://petegoo.github.io/mmbot.scripts/scripts/Misc%20APIs/DefProgramming.csx)


## ExchangeRates

### Description
Creates an anchievement image with user's gravatar pic and achievement text

### Configuration


### Commands
`mmbot rate from <currency> to <currency> - Gets current exchange rate between two currencies.`

### Notes
Currencies are defined by their ISO code: http://en.wikipedia.org/wiki/ISO_4217

### Author
jamessantiago

### Download Link
[Download ExchangeRates](http://petegoo.github.io/mmbot.scripts/scripts/Money/ExchangeRates.csx)


## Giphy

### Description
Interfaces with the Giphy API

### Configuration
MMBOT_GIPHY_APIKEY

### Commands
`mmbot gif me <query> - Returns an animated gif matching the requested search term.`

### Notes
Ported from https://github.com/github/hubot-scripts/blob/master/src/scripts/giphy.coffee

### Author
PeteGoo

### Download Link
[Download Giphy](http://petegoo.github.io/mmbot.scripts/scripts/Images/Giphy.csx)


## GithubNotifications

### Description
Sets up announcements when github events occur like a push or an issue is created

### Configuration
MMBOT_ROUTER_PORT    MMBOT_ROUTER_HOSTNAME    MMBOT_ROUTER_ENABLED    MMBOT_GITHUB_USERNAME    MMBOT_GITHUB_PASSWORD

### Commands
`mmbot set repo alert (push|issues|pull request) on owner/repo - Sets up an alert to announce in the room when an event happens on github     mmbot remove repo alert (push|issues|pull request|*) on owner/repo - Removes a github alert     mmbot list [all] repo alerts - Lists all the github repo alerts that have been setup. all will list thos for all rooms`

### Notes
Uses the router. Needs to have the router correctly configured. For information on event types see http://developer.github.com/v3/activity/events/types/    You must install the Octokit package for this script to run (type "nuget install Octokit -o packages" from your installation directory).

### Author
petegoo

### Download Link
[Download GithubNotifications](http://petegoo.github.io/mmbot.scripts/scripts/Source%20Control/GithubNotifications.csx)


## GithubStatus

### Description


### Configuration


### Commands


### Notes


### Author


### Download Link
[Download GithubStatus](http://petegoo.github.io/mmbot.scripts/scripts/Source%20Control/GithubStatus.csx)


## GoogleImages

### Description


### Configuration


### Commands


### Notes


### Author


### Download Link
[Download GoogleImages](http://petegoo.github.io/mmbot.scripts/scripts/Images/GoogleImages.csx)


## Map

### Description


### Configuration


### Commands


### Notes


### Author


### Download Link
[Download Map](http://petegoo.github.io/mmbot.scripts/scripts/Misc%20APIs/Map.csx)


## Powershell

### Description
Executes a powershell command

### Configuration


### Commands
`mmbot ps <command> - Executes a powershell command`

### Notes
Requires the MMBot.Powershell nuget package
Output objects must either support a ToString method or be a string to display properly
It is recommended to use the PowershellModule script instead of this one to control what is executed

### Author
jamessantiago

### Download Link
[Download Powershell](http://petegoo.github.io/mmbot.scripts/scripts/System/Powershell.csx)


## PowershellModule

### Description
Executes a powershell script

### Configuration
MMBOT_POWERSHELL_SCRIPTSPATH

### Commands
`mmbot psm (script) (commands)`

### Notes
Requires the MMBot.Powershell nuget package
Specify the powershell scripts folder using the MMBOT_POWERSHELL_SCRIPTSPATH key in the ini file
Powershell scripts must be .psm1 (modules) to be executed
Only scripts inside the scripts folder may be executed using this script
Output objects must either support a ToString method or be a string to display properly

### Author
jamessantiago

### Download Link
[Download PowershellModule](http://petegoo.github.io/mmbot.scripts/scripts/System/PowershellModule.csx)


## Pug

### Description


### Configuration


### Commands


### Notes


### Author


### Download Link
[Download Pug](http://petegoo.github.io/mmbot.scripts/scripts/Images/Pug.csx)


## Rules

### Description


### Configuration


### Commands


### Notes


### Author


### Download Link
[Download Rules](http://petegoo.github.io/mmbot.scripts/scripts/Bot%20Talk/Rules.csx)


## Spot

### Description


### Configuration


### Commands


### Notes


### Author


### Download Link
[Download Spot](http://petegoo.github.io/mmbot.scripts/scripts/Music/Spot.csx)


## TeamCity

### Description


### Configuration


### Commands


### Notes


### Author


### Download Link
[Download TeamCity](http://petegoo.github.io/mmbot.scripts/scripts/Source%20Control/TeamCity.csx)


## Translate

### Description


### Configuration


### Commands


### Notes


### Author


### Download Link
[Download Translate](http://petegoo.github.io/mmbot.scripts/scripts/Misc%20APIs/Translate.csx)


## Urban

### Description


### Configuration


### Commands


### Notes


### Author


### Download Link
[Download Urban](http://petegoo.github.io/mmbot.scripts/scripts/Misc%20APIs/Urban.csx)


## whenisay

### Description


### Configuration


### Commands


### Notes


### Author


### Download Link
[Download whenisay](http://petegoo.github.io/mmbot.scripts/scripts/Bot%20Talk/whenisay.csx)


## WhoIs

### Description
Defines a person

### Configuration


### Commands
`who am I - returns what you are known as`
`who is <user> - returns what a user is known as`
`<user> is <definition> - defines a person`

### Notes
Similar to https://github.com/github/hubot/blob/master/src/scripts/roles.coffee

### Author
jamessantiago

### Download Link
[Download WhoIs](http://petegoo.github.io/mmbot.scripts/scripts/Bot%20Talk/WhoIs.csx)


## Xkcd

### Description


### Configuration


### Commands


### Notes


### Author


### Download Link
[Download Xkcd](http://petegoo.github.io/mmbot.scripts/scripts/Images/Xkcd.csx)


## Youtube

### Description


### Configuration


### Commands


### Notes


### Author


### Download Link
[Download Youtube](http://petegoo.github.io/mmbot.scripts/scripts/Video/Youtube.csx)



