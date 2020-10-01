echo off
cls

echo.
echo.
echo.
echo Preparing temporary output folder...
mkdir "StringTypeEnumConverter\bin\nuget" 2> nul
echo Deleting any existing *.nupkg files...
del "StringTypeEnumConverter\bin\nuget\*.nupkg"
echo done.
echo.
echo.
echo.
echo *************** STEP 1/4 *********************************
echo **********************************************************
echo.
echo.
echo Restoring nuget packages...
.nuget\nuget restore "StringTypeEnumConverter\packages.config" -NonInteractive -PackagesDirectory "packages"
echo.
echo.
echo.
echo *************** STEP 2/4 *********************************
echo **********************************************************
echo Compiling solution using msbuild.exe...
msbuild StringTypeEnumConverter.sln -p:Configuration=Release /nologo /m /t:Rebuild
echo.
echo.
echo.
echo *************** STEP 3/4 *********************************
echo **********************************************************
echo.
echo.
echo Creating nuget package using nuget.exe...
.nuget\nuget pack "StringTypeEnumConverter\StringTypeEnumConverter.csproj" -IncludeReferencedProjects -OutputDirectory "StringTypeEnumConverter\bin\nuget\\" -properties Configuration=Release
echo.
echo.
echo.
