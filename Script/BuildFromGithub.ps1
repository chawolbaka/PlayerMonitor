$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition;
mkdir .temp
git clone https://github.com/chawolbaka/MinecraftProtocol.git .temp
dotnet publish .temp/MinecraftProtocol/MinecraftProtocol.csproj --configuration Release --output $ScriptPath
Remove-Item -Path .temp -Recurse -Force