dotnet build --configuration Release
if errorlevel 1 (
   exit /b %errorlevel%
)
dotnet lambda package
if errorlevel 1 (
   exit /b %errorlevel%
)
sls deploy --stage dev
