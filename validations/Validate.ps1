param(
  [string]$Configuration = "Release"
)
push-location $PsScriptRoot\..\src\web
try {
    [void] (dotnet build FfAdmin.sln -c $Configuration)
    $dlls = gci -Directory | % { if(Test-Path "$($_.FullName)/bin/$Configuration") {gci "$($_.FullName)/bin/$Configuration/*/$($_.Name).dll" }} 
    asma analyze $dlls > $PSScriptRoot/code.ttl
    cd ../../wiki
    giki rdf-export -l -g . > $PSScriptRoot/wiki.ttl
}finally {
    pop-location
}