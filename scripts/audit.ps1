((Invoke-WebRequest http://localhost:7071/api/Main/option-worth-history).Content 
    | ConvertFrom-Json)."1" 
    | ForEach-Object { [pscustomobject] 
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
    | convertto-csv