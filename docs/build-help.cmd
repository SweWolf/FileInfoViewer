@echo off
rem Builds help.html from help.md: one self-contained file with the style from help.css.
rem Needs Pandoc (winget install JohnMacFarlane.Pandoc). Run it after every change to help.md.
cd /d "%~dp0"
pandoc help.md -o help.html --standalone --embed-resources --css=help.css --metadata pagetitle="File Info Viewer Help"
if errorlevel 1 (echo Pandoc failed. & exit /b 1)
echo help.html created.
