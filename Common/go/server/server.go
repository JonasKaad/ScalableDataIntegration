package server

import (
	"fmt"
	"github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/filter"
	"github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/parser"
	"google.golang.org/grpc"
	"log"
	"net"
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
