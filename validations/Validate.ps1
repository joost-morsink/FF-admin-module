param(
  [string]$Configuration = "Release",
  [switch]$FetchOnly
)
push-location $PsScriptRoot\..\src\web
try {
    [void] (dotnet build FfAdmin.sln -c $Configuration)
    $dlls = gci -Directory | % { if(Test-Path "$($_.FullName)/bin/$Configuration") {gci "$($_.FullName)/bin/$Configuration/*/$($_.Name).dll" }} 
    archvalidator analyze -l $dlls > $PSScriptRoot/code.ttl
    cd ../../wiki
    giki rdf-export -l -g . > $PSScriptRoot/wiki.ttl
    if(!$FetchOnly){
        archvalidator run -s $PSScriptRoot/Validations.ks
    }
}finally {
    pop-location
}