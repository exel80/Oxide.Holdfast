@echo off
cls
:start
echo Starting server...

"Holdfast NaW.exe" -batchmode ^
-serveraddress 0.0.0.0 ^
-serverport 20100 ^
-lobbyaddress 94.130.66.231 ^
-lobbyport 7101 ^
-framerate 60 ^
-startserver ^
-serverConfigFilePath serverconfig.txt ^
-logFile logs/output_log.txt ^
-logArchivesDirectory logs ^
-bannedPlayersFilePath bannedplayers.txt

echo.
echo Restarting server...
timeout /t 10
echo.
goto start
