@echo off
title Acadex - Single Tap Launcher
echo ===================================================
echo           ACADEX SYSTEM LAUNCHER
echo   Role-Based Assignment & Submission Management
echo ===================================================
echo.

:: Check Docker installation
where docker >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] Docker is not installed or not in your system PATH.
    echo Please install Docker Desktop and start the Docker daemon first.
    pause
    exit /b 1
)

:: Determine compose command
set COMPOSE_CMD=docker compose
docker compose version >nul 2>nul
if %errorlevel% neq 0 (
    set COMPOSE_CMD=docker-compose
)

echo [1/3] Spinning up PostgreSQL, Backend API, and Next.js Frontend containers using %COMPOSE_CMD%...
%COMPOSE_CMD% up -d --build

if %errorlevel% neq 0 (
    echo [ERROR] Docker compose failed to start.
    pause
    exit /b 1
)

echo.
echo [2/3] Containers deployed successfully!
echo Waiting for services to initialize...
timeout /t 5 /nobreak >nul

echo.
echo [3/3] Opening Acadex in your default web browser...
start http://localhost:3000

echo.
echo ===================================================
echo               ACADEX IS LIVE!
echo ===================================================
echo  Frontend UI:  http://localhost:3000
echo  Backend API:  http://localhost:5000/swagger
echo.
echo  Demo Credentials:
echo   - Admin:   admin@acadex.edu   / Admin@12345
echo   - Teacher: teacher@acadex.edu / Teacher@12345
echo   - Student: student@acadex.edu / Student@12345
echo ===================================================
echo Press any key to stop and shut down containers...
pause >nul

echo Shutting down Acadex containers...
%COMPOSE_CMD% down
echo Goodbye!
