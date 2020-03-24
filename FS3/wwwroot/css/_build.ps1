Get-ChildItem -Recurse -Filter *.css | Remove-Item
Get-ChildItem -Recurse -Filter *.css.map | Remove-Item

sassc Base.scss Base.css

#foreach ($theme in $(Get-ChildItem "..\..\_resources\UISet.ColorSet\Themes\" -Directory))
#{
#    Write-Output "Building stylesheet: $($theme.FullName)"
#    & sassc -m "Style.$($theme.Name).scss" ".\Build\Style.$($theme.Name).css"
#
#    if (-not $?)
#    {
#        exit 2
#    }
#}
