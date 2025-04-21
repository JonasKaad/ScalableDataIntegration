#!/bin/bash
export ACCESS_TOKEN=$(az account get-access-token --resource=https://vault.azure.net | jq -r .accessToken)
docker-compose up --build -d