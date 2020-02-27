Get-ChildItem -Recurse -Filter *.css | Remove-Item
Get-ChildItem -Recurse -Filter *.css.map | Remove-Item

Remove-Item -Force -Recurse .\colorset\ -ErrorAction Ignore
New-Item -Type Directory -Path .\colorset\ | Out-Null

$latest = (ConvertFrom-Json $(Invoke-WebRequest -UseBasicParsing -Uri https://api.nuget.org/v3-flatcontainer/colorset/index.json).Content).versions | Select-Object -Last 1
Write-Output "Found latest version of ColorSet: $latest"

$sourcePath = "$env:USERPROFILE\.nuget\packages\colorset\$latest\content\Themes\*"
Copy-Item -Path $sourcePath -Destination .\colorset\ -Recurse
