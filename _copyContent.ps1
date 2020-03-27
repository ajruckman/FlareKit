# Copies resources from specified packages in the local NuGet cache to the '_resources' directory.
# Necessary because 'content' and 'contentFiles' are no longer copied to 'obj' on package restore.
# See: 
# - https://github.com/NuGet/Home/issues/6659
# - https://github.com/dotnet/project-system/issues/3042

param(
    [string[]] $Packages
)

$libraries = $(Get-Content .\obj\project.assets.json | ConvertFrom-Json).libraries.psobject.Members

Remove-Item -Force -Recurse .\_resources\ -ErrorAction SilentlyContinue

foreach ($package in $Packages) {
    $found = $false
    
    foreach ($library in $libraries) {
        if ($library.Name.StartsWith($package)) {
            $libraryName = $library.Name.Replace('/', '\')
            
            $cachePath = "$env:USERPROFILE\.nuget\packages\$($libraryName)"
            if (-not $(Test-Path $cachePath)) {
                throw "Package '$($libraryName)' was not found in '$env:USERPROFILE\.nuget' package cache"
            }

            $resources = "$cachePath\EmbeddedResources\"
            if (-not $(Test-Path $resources)) {
                throw "Package '$($libraryName)' does not contain an EmbeddedResources folder"
            }

            Write-Output "Found package '$($libraryName)' resources in '$resources'"

            Copy-Item -Recurse -Path "$resources" -Destination ".\_resources\$package"

            $found = $true
            break
        }
    }

    if (-not $found) {
        throw "Package '$package' was not found in project project.assets.json"
    }
}