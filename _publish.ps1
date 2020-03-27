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

cd .\FS3\wwwroot\
.\_build.ps1
cd ..\..\

cd .\FT3\wwwroot\
.\_build.ps1
cd ..\..\

dotnet pack -c Debug -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg

if (!(Test-Path .\_published\))
{
    md .\_published\
}

Get-ChildItem -Directory | foreach {
    if (Test-Path "$( $_.FullName )\bin")
    {
        Get-ChildItem "$($_.FullName)\bin\" -Depth 1  | ? { ($_.FullName -like "*.nupkg") -or ($_.FullName -like "*.snupkg") } | foreach {
            Write-Output "Copying package: $( $_.Name )"
            Copy-Item $_.FullName .\_published\
        }
    }
}

Remove-Item -Force -Recurse -ErrorAction Ignore $HOME\.nuget\packages\flarelib\
Remove-Item -Force -Recurse -ErrorAction Ignore $HOME\.nuget\packages\flareselect\
Remove-Item -Force -Recurse -ErrorAction Ignore $HOME\.nuget\packages\flaretables\
Remove-Item -Force -Recurse -ErrorAction Ignore $HOME\.nuget\packages\fs3\
Remove-Item -Force -Recurse -ErrorAction Ignore $HOME\.nuget\packages\ft3\
