# Project instructions

Unity 2022.3 project; gameplay code lives in `Assets/Script`.

- Never build or compile: no `dotnet build`, `msbuild`, `csc`, `Unity.exe -batchmode`, no
  `FrameworkPathOverride`, no retargeting the generated `.csproj`s, no polling `Editor.log`.
- Instead, **tell the user to recompile** and stop. They report any `error CS` output.
- No build, lint, or test tooling here.