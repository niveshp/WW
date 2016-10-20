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
		        $TestList += " /test:" + $TestName.Test.SubString($TestName.Test.LastIndexOf(".") + 1)
	        }
	    } else {        
            if ($playlistContent.Playlist.Add.Test -ne $null) {
                $TestList = " /test:" + $playlistContent.Playlist.Add.Test.SubString($playlistContent.Playlist.Add.Test.LastIndexOf(".") + 1)
            } else {
	            Write-Host Error parsing Playlist.Add from playlist file at $_.FullName
	            Continue
            }
        }
    }
}

# Create assemblies list.
$TestAssembliesList = ''
foreach ($file in Get-ChildItem $SolutionDir -Filter Warewolf.*.Tests ) {
    $TestAssembliesList = $TestAssembliesList + " /testcontainer:`"" + $file.FullName + "\bin\Debug\" + $file.Name + ".dll`""
}
foreach ($file in Get-ChildItem $SolutionDir -Filter Dev2.*.Tests ) {
    $TestAssembliesList = $TestAssembliesList + " /testcontainer:`"" + $file.FullName + "\bin\Debug\" + $file.Name + ".dll`""
}

# Create full VSTest argument string.
$FullArgsList = $TestAssembliesList + " /resultsfile:TestResults\UnitTestResults.trx " + $TestList

# Display full command including full argument string.
Write-Host `"$env:vs140comntools..\IDE\MSTest.exe`"$FullArgsList

# Run VSTest with full argument string.
Start-Process -FilePath "$env:vs140comntools..\IDE\MSTest.exe" -ArgumentList @($FullArgsList) -verb RunAs -WorkingDirectory $SolutionDir -Wait

# Write failing tests playlist.
[string]$testResultsFolder = $SolutionDir + "\TestResults"
if (Test-Path $testResultsFolder\*.trx) {
    Write-Host Writing all test failures in `"$testResultsFolder`" to a playlist file

    Get-ChildItem "$testResultsFolder" -Filter *.trx | Rename-Item -NewName {$_.name -replace ' ','_' }

    $PlayList = "<Playlist Version=`"1.0`">"
    Get-ChildItem "$testResultsFolder" -Filter *.trx | `
    Foreach-Object{
	    [xml]$trxContent = Get-Content $_.FullName
	    if ($trxContent.TestRun.Results.UnitTestResult.count -le 0) {
		    Write-Host Error parsing TestRun.Results.UnitTestResult from trx file at $_.FullName
		    Continue
	    }
	    foreach( $TestResult in $trxContent.TestRun.Results.UnitTestResult) {
		    if ($TestResult.outcome -eq "Passed") {
			    Continue
		    }
		    if ($trxContent.TestRun.TestDefinitions.UnitTest.TestMethod.count -le 0) {
			    Write-Host Error parsing TestRun.TestDefinitions.UnitTest.TestMethod from trx file at $_.FullName
			    Continue
		    }
		    foreach( $TestDefinition in $trxContent.TestRun.TestDefinitions.UnitTest.TestMethod) {
			    if ($TestDefinition.name -eq $TestResult.testName) {
				    $PlayList += "<Add Test=`"" + $TestDefinition.className + "." + $TestDefinition.name + "`" />"
			    }
		    }
	    }
    }
    $PlayList += "</Playlist>"
    $OutPlaylistPath = $testResultsFolder + "\TestFailures.playlist"
    $PlayList | Out-File -LiteralPath $OutPlaylistPath -Encoding utf8 -Force
    Write-Host Playlist file written to `"$OutPlaylistPath`".
}