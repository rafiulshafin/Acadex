#!/bin/bash
set -e

echo "==================================================="
echo "          ACADEX SYSTEM LAUNCHER"
echo "  Role-Based Assignment & Submission Management"
echo "==================================================="
echo ""

# Check Docker installation
if ! command -v docker &> /dev/null; then
    echo "[ERROR] Docker is not installed or not in PATH."
    exit 1
fi

# Detect docker compose or docker-compose
if docker compose version &> /dev/null; then
    COMPOSE_CMD="docker compose"
elif command -v docker-compose &> /dev/null; then
    COMPOSE_CMD="docker-compose"
else
    echo "[ERROR] Neither 'docker compose' nor 'docker-compose' found."
    exit 1
fi

echo "[1/3] Building and starting containers via $COMPOSE_CMD..."
$COMPOSE_CMD up -d --build

echo ""
echo "[2/3] Waiting for services to become healthy..."
sleep 5

echo ""
echo "[3/3] Launching web application..."
URL="http://localhost:3001"

if command -v xdg-open &> /dev/null; then
    xdg-open "$URL"
elif command -v open &> /dev/null; then
    open "$URL"
else
    echo "Please open $URL in your web browser."
fi

echo ""
echo "==================================================="
echo "              ACADEX IS LIVE!"
echo "==================================================="
echo " Frontend UI:  http://localhost:3000"
echo " Backend API:  http://localhost:5000/swagger"
echo ""
echo " Demo Credentials:"
echo "  - Admin:   admin@acadex.edu   / Admin@12345"
echo "  - Teacher: teacher@acadex.edu / Teacher@12345"
echo "  - Student: student@acadex.edu / Student@12345"
echo "==================================================="
