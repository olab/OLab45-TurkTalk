#!/bin/bash
cd ../Common
find . -type d -name bin -ls -exec rm -Rf {} \; > /dev/null
find . -type d -name obj -ls -exec rm -Rf {} \; > /dev/null
git pull
cd ../Api
git pull
service olab46api stop
find . -type d -name bin -ls -exec rm -Rf {} \;
find . -type d -name obj -ls -exec rm -Rf {} \;
cd WebApiService
ln -s /opt/olab46/api bin
cd ..
dotnet clean OLab4WebApi.sln
dotnet build -c Release OLab4WebApi.sln
service olab46api start
service olab46api status
