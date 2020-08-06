Remove-Item *.css
Remove-Item *.css.map

cd ..
dotnet restore
Remove-Item -Force -Recurse .\_resources\ -ErrorAction SilentlyContinue
#..\_copyContent.ps1 UISet.ShapeSet
cd .\wwwroot\

& sassc -m "FlareSelect.scss" "FlareSelect.css"

if (-not $?)
{
    exit 2
}
