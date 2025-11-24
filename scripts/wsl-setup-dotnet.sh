#!/usr/bin/env bash
set -euo pipefail

if [ "$(id -u)" -ne 0 ]; then
  echo "This script must be run as root. Use: sudo ./wsl-setup-dotnet.sh"
  exit 1
fi

# Create system profile script so non-login shells see DOTNET
cat > /etc/profile.d/dotnet.sh <<'EOF'
export DOTNET_ROOT=/usr/share/dotnet
export PATH=$DOTNET_ROOT:$PATH
EOF
chmod +x /etc/profile.d/dotnet.sh

# Ensure symlink
ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet

# Persist DOTNET_ROOT in /etc/environment for non-interactive contexts
if ! grep -q '^DOTNET_ROOT=' /etc/environment 2>/dev/null; then
  echo 'DOTNET_ROOT=/usr/share/dotnet' >> /etc/environment
fi

echo "WSL .NET environment configured."
echo "Next: from Windows run 'wsl --shutdown' then reopen Ubuntu. Verify with 'wsl -d Ubuntu -- dotnet --info'"