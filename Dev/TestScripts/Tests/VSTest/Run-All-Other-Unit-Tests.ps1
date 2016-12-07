﻿if ([string]::IsNullOrEmpty($PSScriptRoot)) {
	$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}
$SolutionDir = (Get-Item $PSScriptRoot ).parent.parent.parent.FullName
# Read playlists and args.
$TestList = ""
if ($Args.Count -gt 0) {
    $TestList = $Args.ForEach({ "," + $_ })
} else {
    Get-ChildItem "$PSScriptRoot" -Filter *.playlist | `
    Foreach-Object{
	    [xml]$playlistContent = Get-Content $_.FullName
	    if ($playlistContent.Playlist.Add.count -gt 0) {
	        foreach( $TestName in $playlistContent.Playlist.Add) {
		        $TestList += "," + $TestName.Test.SubString($TestName.Test.LastIndexOf(".") + 1)
	        }
	    } else {        
            if ($playlistContent.Playlist.Add.Test -ne $null) {
                $TestList = " /Tests:" + $playlistContent.Playlist.Add.Test.SubString($playlistContent.Playlist.Add.Test.LastIndexOf(".") + 1)
            } else {
	            Write-Host Error parsing Playlist.Add from playlist file at $_.FullName
	            Continue
            }
        }
    }
}
if ($TestList.StartsWith(",")) {
	$TestList = $TestList -replace "^.", " /Tests:"
}

# Create assemblies list.
$TestAssembliesList = ''
foreach ($file in Get-ChildItem $SolutionDir -Filter Warewolf.*.Tests ) {
	if ($file.Name -ne "Warewolf.Studio.ViewModels.Tests.dll") {
		$TestAssembliesList = $TestAssembliesList + " `"" + $file.FullName + "\bin\Debug\" + $file.Name + ".dll`""
	}
}
foreach ($file in Get-ChildItem $SolutionDir -Filter Dev2.*.Tests ) {
	if ($file.Name -ne "Dev2.Activities.Designers.Tests.dll" -and $file.Name -ne "Dev2.Activities.Tests.dll") {
		$TestAssembliesList = $TestAssembliesList + " `"" + $file.FullName + "\bin\Debug\" + $file.Name + ".dll`""
	}
}

# Create full VSTest argument string.
$FullArgsList = $TestAssembliesList + " /logger:trx " + $TestList

# Display full command including full argument string.
Write-Host `"$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe`"$FullArgsList

# Write full command including full argument string.
Out-File -LiteralPath $PSScriptRoot\RunTests.bat -Encoding default -InputObject `"$env:vs140comntools..\IDE\CommonExtensions\Microsoft\TestWindow\VSTest.console.exe`"$FullArgsList