# How to build

First, set the correct configuration in config.yml.

Then, run:

dotnet publish .\Megup\Megup.csproj --runtime linux-x64 -p:PublishSingleFile=true --self-contained true -c Release
