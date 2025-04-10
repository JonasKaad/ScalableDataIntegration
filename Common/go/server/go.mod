module github.com/JonasKaad/ScalableDataIntegration/Common/go/server

go 1.23.3

require (
	github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/filter v0.0.0
	github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/parser v0.0.0
	google.golang.org/grpc v1.58.3
)

require (
	github.com/golang/protobuf v1.5.3 // indirect
	golang.org/x/net v0.36.0 // indirect
	golang.org/x/sys v0.30.0 // indirect
	golang.org/x/text v0.22.0 // indirect
	google.golang.org/genproto/googleapis/rpc v0.0.0-20230711160842-782d3b101e98 // indirect
	google.golang.org/protobuf v1.31.0 // indirect
)

replace github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/parser => ./../../../GeneratedClients/go/parser

replace github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/filter => ./../../../GeneratedClients/go/filter
