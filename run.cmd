@echo off
rem Starts the Assignment Management API (delegates to run.ps1, which
rem stops any stale process holding the dev ports before starting).
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run.ps1"
