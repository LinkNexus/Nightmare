{
  description = "Nightmare: .NET 10 parser and CLI";

  # Pinned, not tracking a branch: a project shell exists so its toolchain
  # moves only when someone moves it. Same revision as the machine's
  # system flake, so everything here is already in the store.
  inputs.nixpkgs.url = "github:NixOS/nixpkgs/8edc0c72e3a38faf5434e40d1f19431125fe4b30";

  outputs = {
    self,
    nixpkgs,
  }: let
    forEachSystem =
      nixpkgs.lib.genAttrs
      ["aarch64-darwin" "x86_64-darwin" "x86_64-linux" "aarch64-linux"];
  in {
    devShells = forEachSystem (system: let
      pkgs = nixpkgs.legacyPackages.${system};
    in {
      default = pkgs.mkShell {
        # Every csproj here targets net10.0.
        nativeBuildInputs = [pkgs.dotnet-sdk_10];

        # Point DOTNET_ROOT at THIS shell's SDK.
        #
        # Not redundant: zsh/rc.d/70-env.zsh in the dotfiles sets
        # DOTNET_ROOT=/run/current-system/sw/share/dotnet as a global
        # default. Without overriding it, the shell pairs this project's
        # `dotnet` binary with the *global* SDK's root -- the mismatch
        # behind "the installed dotnet and the runtime have conflicting
        # versions". It stays invisible while the two happen to be the
        # same build and breaks as soon as they are not.
        DOTNET_ROOT = "${pkgs.dotnet-sdk_10}/share/dotnet";

        # NuGet packages stay with NuGet and the csproj files; this shell
        # declares the SDK only.
      };
    });
  };
}
