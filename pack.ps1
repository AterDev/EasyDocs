[CmdletBinding()]
param (
    [Parameter()]
    [bool]
    $install = $false
)

$location = Get-Location
Write-Host 'Compressing files...'

$filePaths = @('WebSite/css/app.css', 'WebSite/css/docs.css', 'WebSite/css/markdown.css', 'WebSite/css/keyboard_arrow_right.svg',
    'WebSite/js/blogs.js', 'WebSite/js/docs.js', 'WebSite/js/index.js', 'WebSite/js/markdown.js', 'WebSite/js/products.js',
    'WebSite/favicon.ico'
)

$zipFilePath = Join-Path $location './src/Share/template/web.zip'

# delete old zip file
if (Test-Path $zipFilePath) {
    Remove-Item $zipFilePath
}

# Create a temporary directory to hold files with adjusted paths
$tempDir = New-Item -ItemType Directory -Path (Join-Path $location 'temp_zip') -Force
try {
    foreach ($filePath in $filePaths) {
        $destName = $filePath.Replace('WebSite/', '')
        $tempFilePath = Join-Path $tempDir $destName
        $sourceFilePath = Join-Path $location $filePath
        # Ensure the destination directory exists
        $destDir = Split-Path $tempFilePath -Parent
        New-Item -ItemType Directory -Path $destDir -Force | Out-Null
        Copy-Item -Path $sourceFilePath -Destination $tempFilePath -Force
    }
    
    # Compress the temporary directory contents into the zip file
    Compress-Archive -Path "$tempDir/*" -DestinationPath $zipFilePath -Force
}
finally {
    # Clean up temporary directory
    Remove-Item -Path $tempDir -Recurse -Force
}

Write-Host 'Compressing files done.'

Write-Host 'Packing new version...'
dotnet build ./src/BuildSite -c release 
dotnet pack ./src/BuildSite -c release --no-build -o ./nupkg

if ($install) {
    # get package name and version
    $VersionNode = Select-Xml -Path ./src/BuildSite/BuildSite.csproj -XPath '/Project//PropertyGroup/Version'
    $PackageNode = Select-Xml -Path ./src/BuildSite/BuildSite.csproj -XPath '/Project//PropertyGroup/PackageId'
    $Version = $VersionNode.Node.InnerText
    $PackageId = $PackageNode.Node.InnerText

    # uninstall old version
    Write-Host 'uninstall old version'
    dotnet tool uninstall -g $PackageId
 
    Write-Host 'install new version:'$PackageId $Version
    dotnet tool install -g --add-source ./src/BuildSite/nupkg $PackageId --version $Version
}
