param(
    [string]$Branch = "Main",
    [string]$Server = "https://g4g-calculator.azurewebsites.net",
    [switch]$Local
)
if($Local){
    $Server = "http://localhost:7070"
}
((Invoke-WebRequest "$Server/api/$Branch/option-worth-history").Content 
    | ConvertFrom-Json)."1" 
    | ForEach-Object { 
        @{
            EventType=$_.EventType
            Timestamp=$_.Timestamp.ToString("yyyy-MM-dd HH:mm:ssK")
            OldCash=$_.Old.Cash
            OldInvested=$_.Old.Invested
            OldUnentered=$_.Old.Unentered
            OldCumulativeInterest=$_.Old.CumulativeInterest
            OldIdealValue=$_.Old.IdealValue
            OldRealValue=$_.Old.Value
            NewCash=$_.New.Cash
            NewInvested=$_.New.Invested
            NewUnentered=$_.New.Unentered
            NewCumulativeInterest=$_.New.CumulativeInterest 
            NewIdealValue=$_.New.IdealValue
            NewRealValue=$_.New.Value
      } 
        } 
   | ForEach-Object { [PSCustomObject] $_ }
   | Select-Object EventType, TimeStamp, OldCash, OldInvested, OldUnentered, OldCumulativeInterest, OldIdealValue, OldRealValue, NewCash, NewInvested, NewUnentered, NewIdealValue, NewRealValue, NewCumulativeInterest
   | ConvertTo-Csv