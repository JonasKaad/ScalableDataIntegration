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
	var allReports []*metar.Report

	// Use defer to recover from any panic
	defer func() {
		if r := recover(); r != nil {
			log.Printf("Recovered from panic in METAR parsing: %v", r)
		}
	}()

	for _, fullDataString := range data {
		metarDataSplitNewline := strings.Split(fullDataString, "\n")
		for _, metarLine := range metarDataSplitNewline {
			metarLine = strings.TrimSpace(metarLine)
			if len(metarLine) == 0 || metarLine == "magic" {
				continue
			}

			var parsedReport *metar.Report
			func() {
				defer func() {
					if r := recover(); r != nil {
						log.Printf("Parsing failed for line '%s': %v", metarLine, r)
						parsedReport = nil
					}
				}()
				report, err := metarParser.Parse(metarLine)
				if err != nil {
					log.Printf("Error parsing line '%s': %v", metarLine, err)
					return
				}
				parsedReport = &report
			}()

			if parsedReport != nil {
				allReports = append(allReports, parsedReport)
			}
		}
	}

	if len(allReports) == 0 {
		errMsg := "No valid METAR reports could be parsed"
		return &parser.ParseResponse{
			Success: false,
			ErrMsg:  &errMsg,
		}, nil
	}

	jsonData, err := json.MarshalIndent(allReports, "", "  ")
	if err != nil {
		errMsg := "Error converting reports to JSON: " + err.Error()
		log.Print(errMsg)
		return &parser.ParseResponse{
			Success: false,
			ErrMsg:  &errMsg,
		}, nil
	}

	server.SaveData(req.RawData, jsonData)
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

	server.Initialize()
	log.Fatal(srv.Start())
}
