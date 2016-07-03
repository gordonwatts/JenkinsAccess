# JenkinsAccess
Library and PowerShell to access Jenkins jobs and artifacts

Introduction
============

This is a .NET library to access Jenkins job infomration from a library and from PowerShell. Use cases:

    - Access artifact files for processing [lib]
	- Access the job log files to extract information from them []
	- Invoke parameterized builds []

Status
======

This library has just been created, it will take a while.

Development
===========

Built with VS2015.

Debugging
=========

In order to debug the PowerShell commands, configuration the library PSJenkinsAccess' property Debug page so that:

	Start up program: C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe
	Parameters: -noexit -command "& {import-module .\PSJenkinsAccess.dll}"

This will import your module when you start the PowerShell in the debugger with just a click of "start debugging".

Installation
============

Windows 10:

There is a onetime setup you must do in order to declare the myget feed where these commands are published to
one-get:

  Register-PSRepository -name "atlas-myget" -source https://www.myget.org/F/gwatts-powershell/api/v2 -InstallationPolicy Trusted

That done, you should now be able to locate the module for installation:

   find-module PSJenkinsAccessCommands

And if you are happy with the response, install it:

   find-module PSJenkinsAccessCommands | Install-Module -scope CurrentUser

You can also run that from an admin console, and leave off the Scope. After it is installed, to get the most
recent version, use:

   Update-Module PSJenkinsAccessCommands


Building the powershell Module for distribution
===========

Make sure the version numbers in the nuspec and psd1 file's track. MAKE SURE TO BUILD IN Release MODE!!!

	nuget pack .\PSJenkinsAccessCommands.nuspec
	nuget push .\PSJenkinsAccessCommands.XXXX.nupkg -Source https://www.myget.org/F/gwatts-powershell/api/v2/package

Note that the first command will generate lots of warnings - that is because you are packing
up a command, not a library! Obviously, the second line only makes sense if you are pushing to my
myget feed. :-)

