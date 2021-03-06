#!/usr/local/bin/pwsh

Push-Location $PSScriptRoot
try {
    if(!(Test-Path ./events/.git)){
        $username = Read-Host -Prompt "Git user name? "
        $password = ConvertFrom-SecureString (Read-Host -Prompt "Git password? " -AsSecureString) -AsPlainText

        git clone "https://$($username):$($password)@gitlab.com/future-fund/event-data.git" ./events

        $user = Read-Host -Prompt "Username for commit messages? "
        $email = Read-Host -Prompt "Email for commit messages? "

        git config user.name $user
        git config user.email $email
    }

    . ./src/update-database.ps1
    
    docker compose up --build -d

} finally {
    Pop-Location
}
