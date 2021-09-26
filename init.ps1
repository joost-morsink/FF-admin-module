#!/usr/local/bin/pwsh

Push-Location $PSScriptRoot
try {
    if(!(Test-Path ./events/.git)){
        $username = Read-Host -Prompt "Git user name? "
        $password = Read-Host -Prompt "Git password? " -MaskInput

        git clone "https://$($username):$($password)@gitlab.com/future-fund/event-data.git" ./events
    }

    Push-Location src/web
    docker build .
    docker compose up -d
    Pop-Location
} finally {
    Pop-Location
}
