:: This script creates a symlink to the game binaries to account for different installation directories on different systems.


set /p path="Please enter the folder location of your DedicatedServer64: "
cd %~dp0
rmdir IgniteSE1\DedicatedServer64 > nul 2>&1
mklink /J IgniteSE1\DedicatedServer64 "%path%"
if errorlevel 1 goto Error
echo Done!

echo You can now open the plugin without issue.
goto EndFinal

:Error
echo An error occured creating the symlink.
goto EndFinal

:EndFinal
pause
