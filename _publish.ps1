Clean-DotNETProject
dotnet pack -c Debug
cp .\FlareLib\bin\Debug\FlareLib.*.nupkg .\_published\
cp .\FlareSelect\bin\Debug\FlareSelect.*.nupkg .\_published\
cp .\FlareTables\bin\Debug\FlareTables.*.nupkg .\_published\
Remove-Item -Force -Recurse $HOME\.nuget\packages\flarelib\
Remove-Item -Force -Recurse $HOME\.nuget\packages\flareselect\
Remove-Item -Force -Recurse $HOME\.nuget\packages\flaretables\
