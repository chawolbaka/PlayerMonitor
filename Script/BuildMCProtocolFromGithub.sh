#!/bin/bash
mkdir .temp
git clone https://github.com/chawolbaka/MinecraftProtocol.git .temp
dotnet publish .temp/MinecraftProtocol/MinecraftProtocol.csproj --configuration Release --output ../../../PlayersMonitor-Console/Dependence
rm -rf .temp