curl -X POST "http://localhost:8080/Downloader/Add?downloader=metadataparser" \
  -H "Content-Type: application/json" \
  -d '{
  "downloadUrl": "http://sdihttp.jonaskaad.com/chart.png",
  "backUpUrl": "",
  "parser": "metadataparser",
  "filters": [
    {
      "name": "ocrfilter",
      "parameters": {
        "startX": "21.683421280821783",
        "startY": "15.751422076198782",
        "endX": "279.130063315068483",
        "endY": "725.043190946062082"
      }
    },
    {
      "name": "regexfilter",
      "parameters": {
        "regex": "(VALID|valid)"
      }
    }
  ],
  "name": "metadataparser",
  "pollingRate": "0 */22 * * *",
  "secretName": "testing"
}'

curl -X POST "http://localhost:8080/Downloader/Add?downloader=pythonparser" \
  -H "Content-Type: application/json" \
  -d '{
  "downloadUrl": "https://api.met.no/weatherapi/tafmetar/1.0/taf.txt?icao=EKCH",
  "backUpUrl": "",
  "parser": "pythonparser",
  "filters": [],
  "name": "pythonparser",
  "pollingRate": "0 */8 * * *",
  "secretName": ""
}'

curl -X POST "http://localhost:8080/Downloader/Add?downloader=metarparser" \
  -H "Content-Type: application/json" \
  -d '{
  "downloadUrl": "https://api.met.no/weatherapi/tafmetar/1.0/metar.txt?icao=EKOD",
  "backUpUrl": "",
  "parser": "metarparser",
  "filters": [],
  "name": "metarparser",
  "pollingRate": "0 */11 * * *",
  "secretName": "testing"
}'

curl -X POST "http://localhost:8080/Downloader/Add?downloader=ausotparser" \
  -H "Content-Type: application/json" \
  -d '{
  "downloadUrl": "https://www.airservicesaustralia.com/flextracks/text.asp?ver=1",
  "backUpUrl": "",
  "parser": "ausotparser",
  "filters": [],
  "name": "ausotparser",
  "pollingRate": "0 */6 * * *",
  "secretName": ""
}'