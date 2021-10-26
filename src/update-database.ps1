Push-Location $PSScriptRoot
try {
    $dbScript = "./web/FfAdminWeb/database.sql"
    Get-Content ./database/structure.sql | Set-Content -Path $dbScript
    Get-Content ./database/import.sql | Add-Content -Path $dbScript
    Get-Content ./database/calculation.sql | Add-Content -Path $dbScript
    Get-Content ./database/process.sql | Add-Content -Path $dbScript
    Get-Content ./database/export.sql | Add-Content -Path $dbScript
}finally {
    Pop-Location
}