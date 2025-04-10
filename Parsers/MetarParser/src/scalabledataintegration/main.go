package main

import (
	"fmt"
	filterPb "github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/filter"
	parserPb "github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/parser"
	"google.golang.org/grpc"
	"log"
	"net"
)

type metarParserServer struct {
	parserPb.UnimplementedParserServer
	parser *parserPb.ParseRequest
}

type filterServerImpl struct {
	filterPb.UnimplementedFilterServer
	filter *filterPb.FilterRequest
}

func main() {
	// Create a gRPC server
	lis, err := net.Listen("tcp", ":50051")
	if err != nil {
		log.Fatalf("failed to listen: %v", err)
	}

	s := grpc.NewServer()

	// Register your service implementation
	parserPb.RegisterParserServer(s, &metarParserServer{})
	filterPb.RegisterFilterServer(s, &filterServerImpl{})

	fmt.Println("gRPC server listening on :50051")
	if err := s.Serve(lis); err != nil {
		log.Fatalf("failed to serve: %v", err)
	}
}

//// https://api.met.no/weatherapi/tafmetar/1.0/metar.txt?icao=EKCH
//func main() {
//	p := parser.New()
//	report, _ := p.Parse("EKCH 061620Z 32009KT 290V360 9999 OVC082/// 09/M04 Q1025 NOSIG=")
//	fmt.Println(report)
//}
