module github.com/JonasKaad/ScalableDataIntegration/Parsers/MetarParser/src/scalabledataintegration

go 1.23.3

require (
	github.com/JonasKaad/ScalableDataIntegration/Common/go/server v0.0.0
	github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/parser v0.0.0
	github.com/eugecm/gometar v0.0.0-20191013165408-b5d7ae8fbd5e
	github.com/joho/godotenv v1.5.1
)

require (
	github.com/Azure/azure-sdk-for-go/sdk/azcore v1.18.0 // indirect
	github.com/Azure/azure-sdk-for-go/sdk/azidentity v1.9.0 // indirect
	github.com/Azure/azure-sdk-for-go/sdk/internal v1.11.1 // indirect
	github.com/Azure/azure-sdk-for-go/sdk/storage/azblob v1.6.0 // indirect
	github.com/AzureAD/microsoft-authentication-library-for-go v1.4.2 // indirect
	github.com/JonasKaad/ScalableDataIntegration/GeneratedClients/go/filter v0.0.0 // indirect
	github.com/golang-jwt/jwt/v5 v5.2.2 // indirect
	github.com/google/uuid v1.6.0 // indirect
	github.com/gopherjs/gopherjs v1.17.2 // indirect
	github.com/jtolds/gls v4.20.0+incompatible // indirect
	github.com/kylelemons/godebug v1.1.0 // indirect
	github.com/moul/http2curl v1.0.0 // indirect
	github.com/parnurzeal/gorequest v0.3.0 // indirect
	github.com/pkg/browser v0.0.0-20240102092130-5ac0b6a4141c // indirect
	github.com/pkg/errors v0.9.1 // indirect
	github.com/sirupsen/logrus v1.9.3 // indirect
	github.com/smarty/assertions v1.15.0 // indirect
	golang.org/x/crypto v0.37.0 // indirect
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
