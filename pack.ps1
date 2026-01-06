[CmdletBinding()]
param (
    [Parameter()]
    [bool]
    $install = $false
)

$location = Get-Location
Write-Host 'Compressing files...'

$filePahts = @('WebApp/css/app.css', 'WebApp/css/docs.css', 'WebApp/css/markdown.css', 'WebApp/css/keyboard_arrow_right.svg',
    'WebApp/js/blogs.js', 'WebApp/js/docs.js', 'WebApp/js/index.js', 'WebApp/js/markdown.js',
    'WebApp/favicon.ico'
)

$zipFilePath = Join-Path $location './src/Share/template/web.zip'

# delete old zip file
if (Test-Path $zipFilePath) {
    Remove-Item $zipFilePath
}

$zip = [System.IO.Compression.ZipFile]::Open($zipFilePath, [System.IO.Compression.ZipArchiveMode]::Create)
foreach ($filePath in $filePahts) {
    $destName = $filePath.Replace('WebApp/', '');
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $(Join-Path $location $filePath), $destName)
}

$zip.Dispose()
Write-host 'Compressing files done.'

Write-Host 'Packing new version...'
dotnet build  ./src/BuildSite -c release 
dotnet pack  ./src/BuildSite -c release --no-build -o ./nupkg

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