#!/bin/bash
# setup_sdk.sh - 安装 .NET 10 SDK 到用户目录（无需 sudo）
# 适用于 Ubuntu/Debian 云环境

set -e  # 遇到错误立即退出

echo "📦 开始安装 .NET 10 SDK..."

# 下载 dotnet-install 脚本
if [ -f "./dotnet-install.sh" ]; then
    echo "✅ dotnet-install.sh 已存在"
else
    echo "⬇️ 下载 dotnet-install.sh..."
    wget -q https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh
    chmod +x dotnet-install.sh
fi

# 安装 .NET 10 SDK 到用户目录
echo "🔧 安装 .NET 10 SDK..."
./dotnet-install.sh --channel 10.0 --install-dir "$HOME/.dotnet"

# 添加到当前会话的 PATH
export PATH="$HOME/.dotnet:$PATH"

# 验证安装
echo "✅ 验证安装..."
dotnet --version

echo "🎉 .NET 10 SDK 安装完成！"
echo "💡 提示：新终端需要执行 'source ~/.bashrc' 或重新打开终端"
