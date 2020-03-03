function Kill-ASPDotNETWebserver
{
    $webserver = $( Get-NetTCPConnection -LocalPort 5000 -ErrorAction Ignore )
    if ($webserver)
    {
        Stop-Process -ErrorAction Ignore -Id $webserver.OwningProcess -Force
    }
}

function Clean-DotNETProject
{
    Get-ChildItem .\ -include bin, obj -Recurse | foreach ($_) {
        Write-Output $_.fullname
        Remove-Item $_.fullname -Force -Recurse
    }
}

Kill-ASPDotNETWebserver
Clean-DotNETProject
dotnet restore

cd .\FT3\
..\_copyContent.ps1 UISet.ColorSet
cd .\wwwroot\css\
Remove-Item *.css
Remove-Item *.css.map
.\_build.ps1
cd ../../..

dotnet pack -c Debug

if (!(Test-Path .\_published\))
{
    md .\_published\
}
rm -Force .\_published\*

Get-ChildItem -Directory | foreach {
	if (Test-Path "$($_.FullName)\bin") {
		Get-ChildItem "$($_.FullName)\bin\" -Depth 1 -Filter *.nupkg | foreach {
			Write-Output $_.Name
			Copy-Item $_.FullName .\_published\
		}
	}
}

Remove-Item -Force -Recurse -ErrorAction Ignore $HOME\.nuget\packages\flarelib\
Remove-Item -Force -Recurse -ErrorAction Ignore $HOME\.nuget\packages\flareselect\
Remove-Item -Force -Recurse -ErrorAction Ignore $HOME\.nuget\packages\flaretables\
Remove-Item -Force -Recurse -ErrorAction Ignore $HOME\.nuget\packages\ft3\
