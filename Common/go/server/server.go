package server

import (
	"bytes"
	"fmt"
	"github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/filter"
	"github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/parser"
	"google.golang.org/grpc"
	"log"
	"net"
	"strings"
	"unicode/utf8"
)

// ServerConfig holds configuration for the gRPC server
type ServerConfig struct {
	Port                int
	EnableParserService bool
	EnableFilterService bool
}

// ParserImpl is an interface that must be implemented by parser service providers
type ParserImpl interface {
	parser.ParserServer
}

// FilterImpl is an interface that must be implemented by filter service providers
type FilterImpl interface {
	filter.FilterServer
}

// Server represents a gRPC server with parser and filter services
type Server struct {
	config     ServerConfig
	grpcServer *grpc.Server
	parserImpl ParserImpl
	filterImpl FilterImpl
}

// NewServer creates a new gRPC server with the given configuration
func NewServer(config ServerConfig) *Server {
	s := &Server{
		config:     config,
		grpcServer: grpc.NewServer(),
	}
	return s
}

func GetData(raw_data []byte, format_type string) ([]string, [][]byte) {
// GetData processes raw data and returns relevant data in string format or the raw data as byte slices
func GetData(rawData []byte, formatType string) ([]string, [][]byte) {
	// Split on "magic" and create a slice for each
	dataList := bytes.Split(rawData, []byte("magic"))

	var relevant [][]string
	var raw [][]byte

	// Process each section based on format type
	for _, data := range dataList {
		if len(data) == 0 {
			continue
		}

		if formatType == "str" {
			// Try to decode as UTF-8
			dataStr, err := utf8DecodeString(data)
			if err != nil {
				log.Printf("Warning: Could not decode part of data as UTF-8")
				raw = append(raw, data)
			} else {
				// Split by semicolon
				splitData := strings.Split(dataStr, ";")
				relevant = append(relevant, splitData)
			}
		} else if formatType == "img" {
			// TODO: Like in python, do some more checks to see if data is actually image
			relevant = append(relevant, []string{string(data)})
		} else {
			raw = append(raw, data)
		}
	}

	// flatten slices, like in the python parser
	var flattenedRelevant []string
	for _, sublist := range relevant {
		flattenedRelevant = append(flattenedRelevant, sublist...)
	}

	if len(flattenedRelevant) > 0 {
		return flattenedRelevant, raw
	} else {
		return []string{}, raw
	}
}

// Helper function for decoding
func utf8DecodeString(b []byte) (string, error) {
	if !utf8.Valid(b) {
		return "", fmt.Errorf("invalid UTF-8 sequence")
	}
	return string(b), nil
}

// RegisterParserService registers a parser service implementation
func (s *Server) RegisterParserService(impl ParserImpl) {
	s.parserImpl = impl
	parser.RegisterParserServer(s.grpcServer, impl)
	log.Println("Parser service registered")
}

// RegisterFilterService registers a filter service implementation
func (s *Server) RegisterFilterService(impl FilterImpl) {
	s.filterImpl = impl
	filter.RegisterFilterServer(s.grpcServer, impl)
	log.Println("Filter service registered")
}

// Start starts the gRPC server
func (s *Server) Start() error {
	addr := fmt.Sprintf(":%d", s.config.Port)
	lis, err := net.Listen("tcp", addr)
	if err != nil {
		return fmt.Errorf("failed to listen: %v", err)
	}

	log.Printf("Server listening on %s", addr)
	return s.grpcServer.Serve(lis)
}

// Stop gracefully stops the server
func (s *Server) Stop() {
	s.grpcServer.GracefulStop()
}
