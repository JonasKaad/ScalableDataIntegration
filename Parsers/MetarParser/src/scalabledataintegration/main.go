package main

import (
	"context"
	"github.com/JonasKaad/ScalableDataIntegration/Common/go/server"
	"github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/parser"
	"log"
)

type MetarParserImpl struct {
	parser.UnimplementedParserServer
}

func (m *MetarParserImpl) ParseCall(ctx context.Context, req *parser.ParseRequest) (*parser.ParseResponse, error) {

	log.Printf("Received parse request with format: %s", req.Format)

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

//// https://api.met.no/weatherapi/tafmetar/1.0/metar.txt?icao=EKCH
//func main() {
//	p := parser.New()
//	report, _ := p.Parse("EKCH 061620Z 32009KT 290V360 9999 OVC082/// 09/M04 Q1025 NOSIG=")
//	fmt.Println(report)
//}
