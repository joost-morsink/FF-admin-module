param (
    $Branch = "Main"
)
$culture = [cultureinfo]::CurrentCulture
try {
    [cultureinfo]::CurrentCulture = [cultureinfo]::InvariantCulture
    $charities = (iwr https://g4g-calculator.azurewebsites.net/api/$Branch/charities).Content | ConvertFrom-Json
    $att = (iwr https://g4g-calculator.azurewebsites.net/api/$Branch/amounts-to-transfer).Content | ConvertFrom-Json
    $lines = New-Object -TypeName System.Collections.ArrayList
    foreach($key in ($att | Get-Member -Type NoteProperty))
    {
        $id = $key.Name
        $charity = $charities.$id
        foreach($entry in ($att.$id.Amounts | Get-Member -Type NoteProperty)){
            $curr = $entry.Name
            $amount = $att.$id.Amounts.$curr
            $line = @{Id=$id;Name=$charity.Name;BankName=$charity.Bank.Name;BankBic=$charity.Bank.Bic;BankAccount=$charity.Bank.Account;Currency=$curr;Amount=$amount.ToString()}
            [void] $lines.Add($line)
        }
    }

    $lines | Select-object -Property Id,Name,BankName,BankBic,BankAccount,Currency,Amount | ConvertTo-Csv
} finally {
    [cultureinfo]::CurrentCulture = $culture
}
