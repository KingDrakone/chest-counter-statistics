@echo off
chcp 65001 >nul
title Compilar o Contador Fortnite
cd /d "%~dp0fonte"

where dotnet >nul 2>nul
if errorlevel 1 (
  echo Falta o .NET 10 SDK.
  echo Descarrega em: https://dotnet.microsoft.com/download/dotnet/10.0  ^(".NET SDK", Windows x64^)
  echo Depois volta a executar COMPILAR.bat
  pause
  exit /b 1
)

echo A compilar ^(1 a 2 minutos na primeira vez^)...
dotnet publish src\ChestCounter\ChestCounter.csproj -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true ^
  -p:EnableCompressionInSingleFile=true -p:DebugType=none -o dist
if errorlevel 1 (
  echo.
  echo Falhou. Copia a mensagem de erro acima e envia-a.
  pause
  exit /b 1
)

copy /y "%~dp0fonte\dist\ChestCounter.exe" "%~dp0ChestCounter.exe" >nul
if errorlevel 1 (
  echo Nao consegui copiar o ChestCounter.exe. Esta aberto? Fecha-o e tenta outra vez.
  pause
  exit /b 1
)

echo.
echo Pronto: ChestCounter.exe esta nesta pasta, ao lado do baus.ini.
echo Duplo clique no ChestCounter.exe para abrir o contador.
pause
