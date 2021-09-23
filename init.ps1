#!/usr/local/bin/pwsh

if(!(Test-Path ./events/.git)){
    $username = Read-Host -Prompt "Git user name? "
    $password = Read-Host -Prompt "Git password? " -MaskInput

    git clone "https://$($username):$($password)@gitlab.com/future-fund/event-data.git" ./events
}

docker compose up -d
