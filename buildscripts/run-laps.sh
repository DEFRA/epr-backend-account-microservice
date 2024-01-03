#!/usr/bin/env bash

cd src
docker-compose down --volumes --remove-orphans
echo "Starting database in detached mode..."
docker-compose -f docker-compose.yml -f docker-compose.laps.yml up accountsdb_sql -d

echo "Waiting for database..."
sleep 10

echo "Starting services..."
docker-compose -f docker-compose.yml -f docker-compose.laps.yml up backendaccountservice.api accounts_seed

echo "Stopping services..."
docker-compose down --volumes --remove-orphans