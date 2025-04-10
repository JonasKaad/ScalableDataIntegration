package main

import (
	"fmt"

	"github.com/eugecm/gometar/metar/parser"
)

// https://api.met.no/weatherapi/tafmetar/1.0/metar.txt?icao=EKCH
func main() {
	p := parser.New()
	report, _ := p.Parse("EKCH 061620Z 32009KT 290V360 9999 OVC082/// 09/M04 Q1025 NOSIG=")
	fmt.Println(report)
}
