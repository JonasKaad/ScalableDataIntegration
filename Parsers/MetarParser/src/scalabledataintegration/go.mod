module github.com/JonasKaad/ScalableDataIntegration/Parsers/MetarParser/pkg/scalabledataintegration

go 1.23.3

require (
	github.com/JonasKaad/ScalableDataIntegration/Common/go/server v0.0.0-00010101000000-000000000000
	github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/parser v0.0.0-00010101000000-000000000000
)

require (
	github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/filter v0.0.0-00010101000000-000000000000 // indirect
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
