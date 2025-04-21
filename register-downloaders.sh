#!/bin/bash

# Base URL for downloadorchestrator
BASE_URL="http://localhost:8080/Downloader/Add"

add_downloader() {
    local downloader=$1
    local data=$2
    
    echo "Adding downloader: $downloader"
    response=$(curl -s -w "%{http_code}" -X POST "${BASE_URL}?downloader=${downloader}" \
        -H "Content-Type: application/json" \
        -d "$data")
    
    if [ "${response: -3}" != "200" ]; then
        echo "Error adding $downloader downloader"
        exit 1
    fi
    echo "Successfully added $downloader"
    echo "-------------------"
}

# Metadata parser - Processes image data with OCR and regex filtering
add_downloader "metadataparser" '{
    "downloadUrl": "http://sdihttp.jonaskaad.com/chart.png",
    "backUpUrl": "",
    "parser": "metadataparser",
    "filters": [
        {
            "name": "ocr-filter",
            "parameters": {
                "startX": "21.683421280821783",
                "startY": "15.751422076198782",
                "endX": "279.130063315068483",
                "endY": "725.043190946062082"
            }
        },
        {
            "name": "regex-filter",
            "parameters": {
                "regex": "(VALID|valid)"
            }
        }
    ],
    "name": "metadataparser",
    "pollingRate": "0 */22 * * *",
    "secretName": "testing"
}'

# Python parser - Processes TAF data from met.no
add_downloader "pythonparser" '{
    "downloadUrl": "https://api.met.no/weatherapi/tafmetar/1.0/taf.txt?icao=EKCH",
    "backUpUrl": "",
    "parser": "pythonparser",
    "filters": [],
    "name": "pythonparser",
    "pollingRate": "0 */8 * * *",
    "secretName": ""
}'

# METAR parser - Processes METAR data from met.no
add_downloader "metarparser" '{
    "downloadUrl": "https://api.met.no/weatherapi/tafmetar/1.0/metar.txt?icao=EKOD",
    "backUpUrl": "",
    "parser": "metarparser",
    "filters": [],
    "name": "metarparser",
    "pollingRate": "0 */11 * * *",
    "secretName": "testing"
}'

# AUSOT parser - Processes Australian flight track data
add_downloader "ausotparser" '{
    "downloadUrl": "https://www.airservicesaustralia.com/flextracks/text.asp?ver=1",
    "backUpUrl": "",
    "parser": "ausotparser",
    "filters": [],
    "name": "ausotparser",
    "pollingRate": "0 */6 * * *",
    "secretName": ""
}'