package main

import (
	"context"
	"encoding/json"
	"github.com/JonasKaad/ScalableDataIntegration/Common/go/server"
	"github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/parser"
	"github.com/eugecm/gometar/metar"
	goMetarParser "github.com/eugecm/gometar/metar/parser"
	_ "github.com/joho/godotenv/autoload"
	"log"
	"os"
	"strings"
)

type MetarParserImpl struct {
	parser.UnimplementedParserServer
}

func (m *MetarParserImpl) ParseCall(_ context.Context, req *parser.ParseRequest) (*parser.ParseResponse, error) {
	data, _ := server.GetData(req.GetRawData(), req.Format)
	if len(data) == 0 {
		errMsg := "No data received"
		log.Print(errMsg)
		return &parser.ParseResponse{
			Success: false,
			ErrMsg:  &errMsg,
		}, nil
	}
	metarParser := goMetarParser.New()
	count := 0
	allReports := make([]*metar.Report, 0)
	for _, fullDataString := range data {
		metarDataSplitNewline := strings.Split(fullDataString, "\n")
		for _, metarLine := range metarDataSplitNewline {
			metarLine = strings.TrimSpace(metarLine)
			if len(metarLine) == 0 || metarLine == "magic" {
				continue
			}

			report, err := metarParser.Parse(metarLine)

			if err != nil {
				errMsg := "Error parsing METAR data: " + err.Error()
				log.Print(errMsg)
				return &parser.ParseResponse{
					Success: false,
					ErrMsg:  &errMsg,
				}, nil
			}

			allReports = append(allReports, &report)
			count++
		}
	}

	jsonData, err := json.MarshalIndent(allReports, "", "  ")
	if err != nil {
		errMsg := "Error converting reports to JSON: " + err.Error()
		log.Print(errMsg)
	}

	outputPath := "output.txt"
	err = os.WriteFile(outputPath, jsonData, 0644)
	if err != nil {
		log.Printf("Error writing output file: %v", err)
	}
	log.Printf("Saved %d METAR reports to %s", count, outputPath)

	log.Printf("Sending heartbeat")
	server.RegisterParser()

	return &parser.ParseResponse{
		Success: true,
	}, nil
}

func main() {
	srv := server.NewServer(server.ServerConfig{
		Port:                50051,
		EnableParserService: true,
		EnableFilterService: false,
	})

	srv.RegisterParserService(&MetarParserImpl{})

	log.Fatal(srv.Start())
}
