---
layout: default
title: MMBot Script Catalog
---

# MMBot Scripts

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



