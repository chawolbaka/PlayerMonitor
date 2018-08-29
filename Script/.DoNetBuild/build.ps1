$RunDir = Split-Path -Parent $MyInvocation.MyCommand.Definition;
#Build编译工具
msbuild BuildTools\Source\BuildFromGithub\BuildFromGithub.csproj /t:Build /p:Configuration=Release /p:TargetFramework=v4.5
mkdir tmp/soucre_github
#编译依赖
git clone https://github.com/chawolbaka/MinecraftProtocol.git tmp/soucre_github
cp -r "BuildFiles\packages\" "tmp"
.\BuildTools\BuildFromGithub.exe "$RunDir\tmp\soucre_github\MinecraftProtocol" "$RunDir\BuildFiles\MinecraftProtocol" "$RunDir\tmp\Build" "$RunDir\BuildFiles\MinecraftProtocol\MinecraftProtocol.csproj"
msbuild tmp\Build\MinecraftProtocol.csproj /t:Build /p:Configuration=Release /p:TargetFramework=v4.5
#开始编译本体
mkdir bin
cp tmp\Build\bin\Release\MinecraftProtocol.dll bin
Remove-Item -Path .\tmp\Build -Recurse -Force
Remove-Item -Path .\tmp\soucre_github -Recurse -Force
mkdir .\tmp\soucre_github
git clone https://github.com/chawolbaka/PlayersMonitor.git tmp/soucre_github
.\BuildTools\BuildFromGithub.exe "$RunDir\tmp\soucre_github\PlayersMonitor-Console" "$RunDir\BuildFiles\PlayersMonitor" "$RunDir\tmp\Build" "$RunDir\BuildFiles\PlayersMonitor\PlayersMonitor.csproj"
#复制刚刚编译的依赖文件进去
cp .\bin\MinecraftProtocol.dll  .\tmp\Build\Dependence\
msbuild .\tmp\Build\PlayersMonitor.csproj  /p:Configuration=Release
#编译完成,开始清理文件&取出编译好的文件
cp -r .\tmp\Build\bin\Release\*  .\bin
Remove-Item -Path .\tmp -Recurse -Force
