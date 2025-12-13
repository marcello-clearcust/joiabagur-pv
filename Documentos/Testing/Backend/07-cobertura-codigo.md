# Cobertura de Código

[← Volver al índice](../../testing-backend.md)

## Configuración de Coverlet

```xml
<!-- En cada .csproj de tests -->
<PropertyGroup>
  <!-- Configuración de Coverlet -->
  <CollectCoverage>true</CollectCoverage>
  <CoverletOutputFormat>cobertura</CoverletOutputFormat>
  <CoverletOutput>./TestResults/coverage.cobertura.xml</CoverletOutput>
  
  <!-- Excluir código generado y tests -->
  <ExcludeByFile>**/Migrations/**/*.cs</ExcludeByFile>
  <ExcludeByAttribute>GeneratedCodeAttribute,CompilerGeneratedAttribute</ExcludeByAttribute>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="coverlet.collector" Version="6.0.2">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
  <PackageReference Include="coverlet.msbuild" Version="6.0.2">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

---

## Archivo de Configuración coverlet.runsettings

```xml
<?xml version="1.0" encoding="utf-8"?>
<!-- coverlet.runsettings -->
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat Code Coverage">
        <Configuration>
          <Format>cobertura,opencover</Format>
          
          <!-- Excluir archivos -->
          <ExcludeByFile>
            **/Migrations/*.cs,
            **/Program.cs,
            **/*.Designer.cs
          </ExcludeByFile>
          
          <!-- Excluir por atributo -->
          <ExcludeByAttribute>
            Obsolete,
            GeneratedCodeAttribute,
            CompilerGeneratedAttribute,
            ExcludeFromCodeCoverageAttribute
          </ExcludeByAttribute>
          
          <!-- Incluir solo estos assemblies -->
          <Include>
            [Joyeria.API]*,
            [Joyeria.Core]*,
            [Joyeria.Infrastructure]*
          </Include>
          
          <!-- Excluir tests y mocks -->
          <Exclude>
            [*Tests]*,
            [*]*.Migrations.*
          </Exclude>
          
          <!-- Umbral mínimo -->
          <Threshold>70</Threshold>
          <ThresholdType>line</ThresholdType>
          <ThresholdStat>total</ThresholdStat>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

---

## Comandos para Generar Reportes

```bash
# Ejecutar tests con cobertura
dotnet test backend/Joyeria.sln \
  --collect:"XPlat Code Coverage" \
  --settings coverlet.runsettings \
  --results-directory ./TestResults

# Instalar ReportGenerator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generar reporte HTML
reportgenerator \
  -reports:"./TestResults/**/coverage.cobertura.xml" \
  -targetdir:"./CoverageReport" \
  -reporttypes:"Html;HtmlSummary;Badges;TextSummary"

# Ver resumen en consola
cat ./CoverageReport/Summary.txt

# Abrir reporte HTML (Windows)
start ./CoverageReport/index.html

# Abrir reporte HTML (macOS)
open ./CoverageReport/index.html

# Abrir reporte HTML (Linux)
xdg-open ./CoverageReport/index.html
```

---

## Excluir Código de Cobertura

```csharp
using System.Diagnostics.CodeAnalysis;

namespace Joyeria.API;

// Excluir clase completa
[ExcludeFromCodeCoverage]
public class Program
{
    public static void Main(string[] args)
    {
        // ...
    }
}

// Excluir método específico
public class MyService
{
    public void ImportantMethod()
    {
        // Este código SÍ se mide
    }

    [ExcludeFromCodeCoverage]
    public void DebugOnlyMethod()
    {
        // Este código NO se mide
    }
}

// Excluir código condicional
public class ConfigHelper
{
    [ExcludeFromCodeCoverage]
    private static string GetEnvironmentSpecificConfig()
    {
        // Código que solo se ejecuta en ciertos entornos
        #if DEBUG
        return "debug-config";
        #else
        return "release-config";
        #endif
    }
}
```

---

## Verificación de Umbral Mínimo en CI

```yaml
# En GitHub Actions
coverage-gate:
  name: Coverage Gate
  runs-on: ubuntu-latest
  needs: coverage
  if: always() && needs.coverage.result == 'success'
  
  steps:
    - name: Download coverage report
      uses: actions/download-artifact@v4
      with:
        name: coverage-report
        path: ./CoverageReport

    - name: Check coverage threshold
      run: |
        # Extraer porcentaje de cobertura del reporte
        COVERAGE=$(grep -oP 'line-rate="\K[^"]+' ./CoverageReport/Cobertura.xml | head -1)
        COVERAGE_PERCENT=$(echo "$COVERAGE * 100" | bc)
        
        echo "Current coverage: ${COVERAGE_PERCENT}%"
        
        # Umbral mínimo: 70%
        THRESHOLD=70
        
        if (( $(echo "$COVERAGE_PERCENT < $THRESHOLD" | bc -l) )); then
          echo "❌ Coverage ${COVERAGE_PERCENT}% is below threshold ${THRESHOLD}%"
          exit 1
        else
          echo "✅ Coverage ${COVERAGE_PERCENT}% meets threshold ${THRESHOLD}%"
        fi
```

---

## Integración con Codecov

```yaml
# En GitHub Actions
- name: Upload to Codecov
  uses: codecov/codecov-action@v4
  with:
    files: ./CoverageReport/Cobertura.xml
    fail_ci_if_error: false
    verbose: true
  env:
    CODECOV_TOKEN: ${{ secrets.CODECOV_TOKEN }}
```

---

## Comentar Cobertura en PRs

```yaml
# En GitHub Actions
- name: Add coverage comment to PR
  uses: marocchino/sticky-pull-request-comment@v2
  if: github.event_name == 'pull_request'
  with:
    path: ./CoverageReport/Summary.md
```

