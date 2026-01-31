# Build Script Error Fix

## 问题说明

如果您遇到以下错误：
```
'net-framework' 不是内部或外部命令
'rectory' 不是内部或外部命令
```

这是由于**文件编码或换行符问题**导致的。

## 解决方案

### 方案 1：使用修复后的脚本（推荐）

我已经重新创建了两个脚本，移除了可能导致编码问题的内容：
- ✅ 移除了 UTF-8 BOM 头
- ✅ 移除了所有中文注释（改用英文）
- ✅ 简化了命令语法
- ✅ 使用标准批处理语法

### 方案 2：手动创建脚本

如果修复后的脚本仍有问题，请手动创建以下内容：

#### build.bat

```batch
@echo off
setlocal enabledelayedexpansion

echo Jvedio Build Script
echo.

where msbuild >nul 2>nul
if errorlevel 1 (
    echo MSBuild not found
    echo Please install Visual Studio or .NET Framework SDK
    pause
    exit /b 1
)

echo Restoring NuGet packages...
cd /d "%~dp0Jvedio"
nuget restore packages.config

echo Cleaning...
msbuild Jvedio.csproj /t:Clean /p:Configuration=Release /v:m /nologo

echo Building...
msbuild Jvedio.csproj /t:Build /p:Configuration=Release /v:m /nologo
if errorlevel 1 (
    echo Build failed
    pause
    exit /b 1
)

echo Preparing release...
cd /d "%~dp0"
xcopy "Jvedio\bin\Release\*" "Release\" /E /I /Y /Q
copy ".env.example" "Release\.env" /Y

echo Done!
pause
```

#### start.bat

```batch
@echo off
echo Jvedio Start Script
echo.

if not exist ".env" (
    echo Creating .env file...
    copy ".env.example" ".env"
    notepad .env
)

start Jvedio.exe
```

### 方案 3：检查文件编码

使用记事本或 VS Code 打开脚本文件，确保：
1. 保存为 **ANSI** 或 **UTF-8 without BOM**
2. 换行符为 **CRLF**（Windows 标准）

**VS Code 设置：**
- 右下角点击编码 → "Save with Encoding" → 选择 "UTF-8"
- 右下角点击 CRLF → 确保是 CRLF

**记事本设置：**
- 另存为 → 编码选择 "ANSI"
- 记事本默认使用 CRLF

### 方案 4：使用 PowerShell 脚本（替代方案）

如果批处理脚本一直有问题，可以使用 PowerShell：

#### build.ps1

```powershell
Write-Host "Jvedio Build Script" -ForegroundColor Cyan
Write-Host ""

# Check MSBuild
$msbuild = Get-Command msbuild -ErrorAction SilentlyContinue
if (-not $msbuild) {
    Write-Host "MSBuild not found" -ForegroundColor Red
    exit 1
}

# Build
Write-Host "Building..."
$projectPath = ".\Jvedio\Jvedio.csproj"
& msbuild $projectPath /t:Build /p:Configuration=Release /v:m

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed" -ForegroundColor Red
    exit 1
}

# Copy files
Write-Host "Copying files..."
if (Test-Path "Release") { Remove-Item "Release" -Recurse -Force }
Copy-Item ".\Jvedio\bin\Release\*" -Destination "Release" -Recurse -Force
Copy-Item ".env.example" -Destination "Release\.env"

Write-Host "Build complete!" -ForegroundColor Green
```

运行方式：
```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

## 快速测试

测试脚本是否正常：
```batch
@echo off
echo Test
pause
```

如果这个简单的脚本也有问题，说明是系统环境问题。

## 常见问题

### Q1: 为什么会出现 'net-framework' 错误？

A: 这是因为脚本中的某些长文本行被错误分割了，通常是：
- 文件使用了 UTF-8 BOM 编码
- 换行符格式不正确（应该是 CRLF）

### Q2: 如何验证脚本编码？

A: 使用以下命令检查：
```batch
file build.bat
```

或者用十六进制编辑器查看开头是否有 `EF BB BF`（BOM 头）。

### Q3: PowerShell 脚本为什么更可靠？

A: PowerShell 对编码和换行符更宽容，不需要严格遵循 CRLF。

## 手动编译（如果所有脚本都失败）

如果所有自动化脚本都失败，可以手动执行：

1. **恢复 NuGet 包**
   ```batch
   cd Jvedio
   nuget restore packages.config
   ```

2. **编译项目**
   ```batch
   msbuild Jvedio.csproj /t:Build /p:Configuration=Release
   ```

3. **复制文件**
   ```batch
   cd ..
   mkdir Release
   xcopy Jvedio\bin\Release\* Release\ /E /I /Y
   copy .env.example Release\.env
   ```

4. **配置并运行**
   ```batch
   cd Release
   notepad .env
   Jvedio.exe
   ```

## 技术支持

如果问题仍未解决：
1. 提供错误截图
2. 提供 Windows 版本
3. 提供脚本文件的十六进制内容（前 16 字节）

---

**已修复的脚本**：
- ✅ build.bat - 移除编码问题
- ✅ start.bat - 简化语法

**建议**：优先使用修复后的脚本，如果仍有问题，使用 PowerShell 脚本或手动编译。
