@ECHO OFF
SETLOCAL
SET FILENAME=DressChecklists
IF EXIST %FILENAME%.exe DEL %FILENAME%.exe
IF EXIST %FILENAME%.cs csc /t:exe %FILENAME%.cs ApplicationManager.cs Command.cs Commands.cs CommandRule.cs Parser.cs Validator.cs
ENDLOCAL