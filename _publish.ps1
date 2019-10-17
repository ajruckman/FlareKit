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

dotnet pack -c Debug

if (!(Test-Path .\_published\))
{
    md .\_published\
}
rm -Force .\_published\*

cp .\FlareLib\bin\Debug\FlareLib.*.nupkg .\_published\
cp .\FlareSelect\bin\Debug\FlareSelect.*.nupkg .\_published\
cp .\FlareTables\bin\Debug\FlareTables.*.nupkg .\_published\

Remove-Item -Force -Recurse -ErrorAction Ignore $HOME\.nuget\packages\flarelib\
Remove-Item -Force -Recurse -ErrorAction Ignore $HOME\.nuget\packages\flareselect\
Remove-Item -Force -Recurse -ErrorAction Ignore $HOME\.nuget\packages\flaretables\
