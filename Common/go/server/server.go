package server

import (
	"bytes"
	"fmt"
	"github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/filter"
	"github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/parser"
	"github.com/parnurzeal/gorequest"
	"google.golang.org/grpc"
	"log"
	"net"
	"net/http"
	"os"
	"strings"
	"time"
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

// RegisterParserService registers a parser service implementation
func (s *Server) RegisterParserService(impl ParserImpl) {
	s.parserImpl = impl
	parser.RegisterParserServer(s.grpcServer, impl)
	log.Println("Parser service implementation registered")
}

// RegisterFilterService registers a filter service implementation
func (s *Server) RegisterFilterService(impl FilterImpl) {
	s.filterImpl = impl
	filter.RegisterFilterServer(s.grpcServer, impl)
	log.Println("Filter service implementation registered")
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

/* - Service Registry - */

// SendHeartbeat Sends a heartbeat to the service registry
func SendHeartbeat(heartbeatTypeOptional ...string) ([]error, bool) {
	heartbeatType := "Parser"
	if len(heartbeatTypeOptional) > 0 {
		heartbeatType = heartbeatTypeOptional[0]
	}

	baseurl := os.Getenv("BASE_URL")
	parserName := os.Getenv("PARSER_NAME")
	url := fmt.Sprintf("%s/%s/%s/heartbeat", baseurl, heartbeatType, parserName)

	request := gorequest.New()
	resp, _, errs := request.Post(url).
		Type("json").
		Send(nil).
		End()
	if errs != nil {
		if resp != nil {
			log.Printf("Heartbeat failed. Status: %s", resp.Status)
		}
		return errs, false
	}
	if resp.StatusCode == http.StatusOK {
		log.Printf("Heartbeat sent. Status: %s", resp.Status)
		return nil, true
	} else {
		log.Printf("Heartbeat failed. Status: %s", resp.Status)
		return nil, false
	}
}

var retryInterval = 60

// HeartbeatScheduler sets up a scheduler to send heartbeats to the service registry or register the service again if
// it fails to get a response on the heartbeat
func HeartbeatScheduler(registerTypeOptional ...string) {
	registerType := "Parser"
	if len(registerTypeOptional) > 0 {
		registerType = registerTypeOptional[0]
	}

	alive := true
	var errors []error = nil

	for {
		time.Sleep(time.Duration(retryInterval) * time.Second)
		if alive {
			errors, alive = SendHeartbeat()
			if errors != nil {
				log.Printf("Error sending heartbeat: %v", errors)
			}
		} else {
			alive = RegisterService(registerType)
			if !alive {
				log.Printf("Registering failed. Retrying in %d seconds...", retryInterval)
			}
		}
	}
}

// RegisterService registers the service with the service registry endpoint
func RegisterService(registerTypeOptional ...string) bool {
	registerType := "Parser"
	if len(registerTypeOptional) > 0 {
		registerType = registerTypeOptional[0]
	}

	baseurl := os.Getenv("BASE_URL")
	parserName := os.Getenv("PARSER_NAME")
	url := fmt.Sprintf("%s/%s/%s/register", baseurl, registerType, parserName)

	sendBody := fmt.Sprintf(`{"url":"%s"}`, os.Getenv("PARSER_URL"))
	request := gorequest.New()
	resp, _, errs := request.Post(url).
		Type("json").
		Send(sendBody).
		End()

	if errs != nil {
		log.Printf("Error registering parser: %v", errs)
		return false
	}

	if resp.StatusCode == http.StatusOK {
		log.Printf("Successfully registered %s of type: %s", parserName, strings.ToLower(registerType))
		return true
	}

	log.Printf("Registering failed with status code: %d", resp.StatusCode)
	return false
}

// Initialize registers the service and starts the heartbeat scheduler
func Initialize() {
	for {
		var registered = RegisterService()
		if registered {
			log.Println("Registering succeeded")
			go HeartbeatScheduler()
			return
		} else {
			log.Printf("Registering failed. Retrying in %d seconds...", retryInterval)
			time.Sleep(time.Duration(retryInterval) * time.Second)
		}
	}
}

/* -- Data Processing -- */

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

/* -- Azure -- */
