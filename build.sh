#!/bin/bash
cd ../Common
find . -type d -name bin -ls -exec rm -Rf {} \; > /dev/null
find . -type d -name obj -ls -exec rm -Rf {} \; > /dev/null
git pull
cd ../TurkTalkSvc
git pull
service olab46ttalk stop
find . -type d -name bin -ls -exec rm -Rf {} \;
find . -type d -name obj -ls -exec rm -Rf {} \;
cd TurkTalkSvc
ln -s /opt/olab46/ttalk bin
dotnet clean TurkTalkSvc.sln
dotnet build -c Release TurkTalkSvc.sln
service olab46ttalk start
service olab46ttalk status
