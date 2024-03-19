param(
    [string]$Branch = "Main",
    [string]$Currency = "EUR",
    [switch]$Local
)

if($Local){
    $Server = "http://localhost:7070"
}else {
    $Server = "https://g4g-calculator.azurewebsites.net"
}

$charities =  ((Invoke-WebRequest "$Server/api/$Branch/charities").Content
    | ConvertFrom-Json) 
$data = ((Invoke-WebRequest "$Server/api/$Branch/aggregated-donations-and-transfers").Content 
    | ConvertFrom-Json)
    $years = $data.psobject.Properties | Sort-Object Name

write-output "Charity,Donated,$($years | % { $_.Name } | Join-String -Separator ","),Transferred,$($years | % { $_.Name } | Join-String -Separator ",")"
foreach($charityProp in $charities.psobject.Properties){
    $line = """$($charityProp.Value.Name)"",," 
    foreach($year in $years){
        $line = $line + """$($year.Value.$($charityProp.Name).Donated.Amounts.$Currency)""" 
        $line = $line + "," 
    }
    $line = $line + ","
    foreach($year in $years){
        $line = $line + """$($year.Value.$($charityProp.Name).Transferred.Amounts.$Currency)""" 
        $line = $line + "," 
    }
    write-output $line
}