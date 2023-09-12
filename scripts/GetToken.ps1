param(
	[switch] $EventStore,
	[switch] $ModelCache
)

Function Get-Payload([string] $scope) {
return "client_id=d0485879-0573-42d6-979d-dc466cd2f352                                                               
&grant_type=client_credentials
&client_secret=$env:G4G_SECRET
&scope=$scope"
}

if($EventStore){
	$payload = Get-Payload '5ed30cd1-6792-4496-84ca-7064af2a725e/.default'
} elseif($ModelCache) {
	$payload = Get-Payload 'cf4161ea-a1c4-4181-b28f-911083c24201/.default'
}

((iwr https://login.microsoftonline.com/636e74b1-5728-4546-ab61-28ebb862634b/oauth2/v2.0/token -method Post -Body $payload).Content | ConvertFrom-Json).access_token
