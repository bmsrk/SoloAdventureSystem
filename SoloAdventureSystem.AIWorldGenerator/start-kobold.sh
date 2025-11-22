#!/bin/bash
# Quick Start Script for KoboldCpp + AI World Generator
# This script helps you get started with local AI generation

echo "================================"
echo "KoboldCpp AI World Generator"
echo "Quick Start Helper"
echo "================================"
echo ""

# Check if KoboldCpp is running
echo "Checking for KoboldCpp..."
if curl -s http://localhost:5001/api/v1/model > /dev/null 2>&1; then
    echo "[OK] KoboldCpp is running at localhost:5001"
    echo ""
else
    echo "[!] KoboldCpp is NOT running"
    echo ""
    echo "Please start KoboldCpp first!"
    echo ""
    echo "Quick Start:"
    echo "  1. Download KoboldCpp: https://github.com/LostRuins/koboldcpp/releases"
    echo "  2. Download a model (e.g., Phi-3-mini.gguf)"
    echo "  3. Run: ./koboldcpp --model Phi-3-mini.gguf --port 5001"
    echo ""
    echo "See KOBOLDCPP_SETUP.md for detailed instructions."
    echo ""
    exit 1
fi

echo "Starting AI World Generator..."
echo ""
dotnet run --ui
