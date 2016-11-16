# Push the package if the version number has changed.
#
[CmdletBinding()]
param()

# Get the current package numbers
$current = $(nuget list -source https://www.myget.org/F/gwatts-powershell/api/v3/index.json PSJenkinsAccessCommands)
$version = $current.Split()[1]
Write-Verbose "MyGet has version $version of Package PSJenkinsAccessCommands"

# Get the current contents of the nuspec file
$nuspec = Get-Content .\PSJenkinsAccessCommands.nuspec | Select-String $version
if ($nuspec.Count -gt 0) {
	Write-Verbose "Version $version is already on the myget server. Will not push."
} else {
    rm .\*.nupkg
	nuget pack .\PSJenkinsAccessCommands.nuspec
	nuget push .\PSJenkinsAccessCommands.*.nupkg -Source https://www.myget.org/F/gwatts-powershell/api/v2/package
}
