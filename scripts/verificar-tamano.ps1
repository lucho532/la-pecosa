# Comprueba el límite de la constitución §2.3: ningún archivo de código escrito a mano supera las
# 250 líneas. Termina con error y lista cada archivo que lo incumple.

$ErrorActionPreference = 'Stop'

$maximo = 250
$raiz = Split-Path -Parent $PSScriptRoot
$carpetas = @('backend/src', 'backend/pruebas', 'frontend/src')
$extensiones = @('.cs', '.ts', '.tsx', '.css')
$ignoradas = @('Migraciones', 'bin', 'obj', 'node_modules')

$excedidos = @()

foreach ($carpeta in $carpetas) {
    $ruta = Join-Path $raiz $carpeta
    if (-not (Test-Path $ruta)) { continue }

    $archivos = Get-ChildItem -Path $ruta -Recurse -File | Where-Object {
        $partes = $_.FullName.Substring($raiz.Length) -split '[\\/]'
        ($extensiones -contains $_.Extension) -and
            -not ($partes | Where-Object { $ignoradas -contains $_ })
    }

    foreach ($archivo in $archivos) {
        $lineas = @(Get-Content -LiteralPath $archivo.FullName).Count
        if ($lineas -gt $maximo) {
            $excedidos += [pscustomobject]@{
                Lineas  = $lineas
                Archivo = $archivo.FullName.Substring($raiz.Length + 1)
            }
        }
    }
}

if ($excedidos.Count -gt 0) {
    Write-Host "Archivos con más de $maximo líneas (constitución §2.3):"
    $excedidos | Sort-Object Lineas -Descending | ForEach-Object {
        Write-Host ("  {0,5}  {1}" -f $_.Lineas, $_.Archivo)
    }
    exit 1
}

Write-Host "Correcto: ningún archivo supera las $maximo líneas."
exit 0
