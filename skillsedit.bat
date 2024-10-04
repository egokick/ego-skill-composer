@echo off
start "" "python" server.py
timeout /t 3 > nul
start "" "http://localhost:5000"
pause
