module github.com/JonasKaad/ScalableDataIntegration/Parsers/MetarParser/pkg/scalabledataintegration

go 1.23.3

require (
	github.com/JonasKaad/ScalableDataIntegration/Common/go/server v0.0.0
	github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/parser v0.0.0
	github.com/eugecm/gometar v0.0.0-20191013165408-b5d7ae8fbd5e
	github.com/joho/godotenv v1.5.1
)

require (
	github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/filter v0.0.0 // indirect
	github.com/elazarl/goproxy v1.7.2 // indirect
	github.com/moul/http2curl v1.0.0 // indirect
	github.com/parnurzeal/gorequest v0.3.0 // indirect
	github.com/pkg/errors v0.9.1 // indirect
	github.com/smartystreets/goconvey v1.8.1 // indirect
	golang.org/x/net v0.39.0 // indirect
	golang.org/x/sys v0.32.0 // indirect
	golang.org/x/text v0.24.0 // indirect
	google.golang.org/genproto/googleapis/rpc v0.0.0-20250409194420-de1ac958c67a // indirect
	google.golang.org/grpc v1.71.1 // indirect
	google.golang.org/protobuf v1.36.6 // indirect
)

replace github.com/JonasKaad/ScalableDataIntegration/Common/go/server => ../../../../Common/go/server

replace github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/parser => ../../../../GeneratedClients/go/parser

replace github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/filter => ../../../../GeneratedClients/go/filter
