param ([string]$OutputPath = $( throw "-username is required." ), [string]$FetchSource = "https://download.sysinternals.com/files/Handle.zip", [string]$Filename = "handle.exe")

$HandleZipPath = "Handle.zip"
$HandleDestinationPath = "Handle"

Invoke-WebRequest -Uri $FetchSource -OutFile $HandleZipPath
Expand-Archive -Path $HandleZipPath -DestinationPath $HandleDestinationPath
Move-Item -Path "$HandleDestinationPath/$Filename" -Destination $OutputPath -Force

Write-Output "Successfuly fetched $Filename to $OutputPath"
