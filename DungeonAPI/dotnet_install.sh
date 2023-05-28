# Dotnet install
# https://learn.microsoft.com/ko-kr/dotnet/core/install/linux-scripted-manual#manual-install
wget https://dot.net/v1/dotnet-install.sh -O dotnet-installer.sh
sudo chmod +x ./dotnet-installer.sh
# ./dotnet-installer.sh --version latest
./dotnet-installer.sh --channel 7.0

# .NET CLI tab complete func
echo "function _dotnet_bash_complete()
{
  local cur=\"${COMP_WORDS[COMP_CWORD]}\" IFS=$'\n'
  local candidates

  read -d '' -ra candidates < <(dotnet complete --position \"${COMP_POINT}\" \"${COMP_LINE}\" 2>/dev/null)

  read -d '' -ra COMPREPLY < <(compgen -W \"${candidates[*]:-}\" -- \"$cur\")
}

complete -f -F _dotnet_bash_complete dotnet" >> ~/.bashrc

# command export
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools