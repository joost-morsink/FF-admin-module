param(
  [string]$Configuration = "Release",
  [string]$Output = "$PSScriptRoot/../site"
)
$ErrorActionPreference = "Stop"

$wiki = Resolve-Path "$PSScriptRoot/../wiki"
$giki = Join-Path $wiki ".giki"

push-location "$PSScriptRoot/../src/web"
try {
    dotnet build FfAdmin.sln -c $Configuration
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }

    $dlls = gci -Directory | % { if(Test-Path "$($_.FullName)/bin/$Configuration") {gci "$($_.FullName)/bin/$Configuration/*/$($_.Name).dll" }}
    asma analyze $dlls > (Join-Path $giki "code.ttl")
    if ($LASTEXITCODE -ne 0) { throw "asma analyze failed" }

    # giki generates wiki.ttl in the .giki folder itself.
    giki export -l -g $wiki -o $Output
    if ($LASTEXITCODE -ne 0) { throw "giki export failed" }
}finally {
    pop-location
}
