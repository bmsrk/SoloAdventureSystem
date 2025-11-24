#!/usr/bin/env bash
set -euo pipefail

# Per-user installer for .NET 10 (no sudo required)
INSTALL_DIR="$HOME/.dotnet"
CHANNEL="10.0"

mkdir -p "$INSTALL_DIR"
wget -qO /tmp/dotnet-install.sh https://dot.net/v1/dotnet-install.sh
chmod +x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --channel "$CHANNEL" --install-dir "$INSTALL_DIR"

# Add to profile
if ! grep -q 'DOTNET_ROOT' ~/.profile 2>/dev/null; then
  echo "export DOTNET_ROOT=\$HOME/.dotnet" >> ~/.profile
fi
if ! grep -q 'PATH=\$HOME/.dotnet' ~/.profile 2>/dev/null; then
  echo "export PATH=\$HOME/.dotnet:\$PATH" >> ~/.profile
fi

echo "Installed .NET $CHANNEL to $INSTALL_DIR. Run 'source ~/.profile' or open a new shell and run 'dotnet --info' to verify."