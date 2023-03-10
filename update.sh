#!/bin/bash
cd ../Common
git pull
cd ../Api
git pull
service olab46api stop
dotnet clean OLab4WebApi.sln
dotnet build -c Release OLab4WebApi.sln
service olab46api start
service olab46api status
cd ../Player
git pull
npm install
npm run-script build:prod
npm run-script build:dev