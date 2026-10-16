param(
  [string]$Configuration = "Release"
)
$dlls = gci $PSScriptRoot -Directory | % { gci "$($_.FullName)/bin/$Configuration/*/$($_.Name).dll" }
archvalidator analyze -l $dlls > $PSScriptRoot/../../wiki/structure.ttl

