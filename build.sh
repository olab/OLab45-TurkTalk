#!/bin/bash
cd ../Common
find . -type d -name bin -ls -exec rm -Rf {} \; > /dev/null
find . -type d -name obj -ls -exec rm -Rf {} \; > /dev/null
git pull
cd ../TurkTalkSvc
git pull
service olab46ttalk.$1 stop
find . -type d -name bin -ls -exec rm -Rf {} \;
find . -type d -name obj -ls -exec rm -Rf {} \;
cd TurkTalkSvc
if ! test -d ./bin; then
  ln -s /opt/olab46/$1/ttalk bin
fi
dotnet clean TurkTalkSvc.sln
dotnet build -c $1 TurkTalkSvc.sln
service olab46ttalk.$1 start
service olab46ttalk.$1 status
