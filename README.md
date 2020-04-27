# PlayerMonitor (For Minecraft)
![](https://github.com/chawolbaka/PlayerMonitor/workflows/build/badge.svg)  
  
### 支持的服务端  
原版服务端  
Forge  
Spigot  
SpongeForge  
Catserver
  
##### 附加要求
如果是Spigot需要spigot.yml中的sample-count在0以上  
如果是Catserver需要满足以上条件并且版本在[81903c3-universal/51d82d9-async](https://github.com/Luohuayu/CatServer/releases/tag/20.03.12 "81903c3/51d82d9")以上  
如果安装了修改Motd的插件请放弃这个软件，基本上没有一个修改Motd的插件会保留玩家名  

## Building
[安装.NET Core SDK 2.1](https://dotnet.microsoft.com/download/dotnet-core/3.0)

    git clone https://github.com/chawolbaka/PlayerMonitor.git
    cd PlayerMonitor
    dotnet publish PlayerMonitor-Console\PlayerMonitor.csproj -c Release