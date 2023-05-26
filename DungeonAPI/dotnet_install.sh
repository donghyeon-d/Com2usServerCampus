# 닷넷 설치
# https://learn.microsoft.com/ko-kr/dotnet/core/install/linux-scripted-manual#manual-install
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
sudo chmod +x ./dotnet-install.sh
./dotnet-install.sh --version latest
./dotnet-install.sh --channel 7.0

# .NET CLI 탭 완성 기능
echo "function _dotnet_bash_complete()
{
  local cur=\"${COMP_WORDS[COMP_CWORD]}\" IFS=$'\n'
  local candidates

  read -d '' -ra candidates < <(dotnet complete --position \"${COMP_POINT}\" \"${COMP_LINE}\" 2>/dev/null)

  read -d '' -ra COMPREPLY < <(compgen -W \"${candidates[*]:-}\" -- \"$cur\")
}

complete -f -F _dotnet_bash_complete dotnet" >> ~/.bashrc

# 환경변수 설정
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools