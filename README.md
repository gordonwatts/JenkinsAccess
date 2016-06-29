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