((Invoke-WebRequest http://localhost:7070/api/Main/option-worth-history).Content 
    | ConvertFrom-Json)."1" 
    | ForEach-Object { 
        @{
            EventType=$_.EventType
            Timestamp=$_.Timestamp.ToString("yyyy-MM-dd HH:mm:ssK")
            OldCash=$_.Old.Cash
            OldInvested=$_.Old.Invested
            OldUnentered=$_.Old.Unentered
            OldCumulativeInterest=$_.Old.CumulativeInterest
            NewCash=$_.New.Cash
            NewInvested=$_.New.Invested
            NewUnentered=$_.New.Unentered
            NewCumulativeInterest=$_.New.CumulativeInterest } 
        } 
   | ForEach-Object { [PSCustomObject] $_ }
   | Select-Object EventType, TimeStamp, OldCash, OldInvested, OldUnentered, OldCumulativeInterest, NewCash, NewInvested, NewUnentered, NewCumulativeInterest
   | ConvertTo-Csv