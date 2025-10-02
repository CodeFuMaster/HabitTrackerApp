# Sync API Testing Script
# Run this after starting the API server

$apiUrl = "http://localhost:5000"
$deviceId = "test-device-$(Get-Random)"

Write-Host "🧪 HabitTracker Sync API Testing" -ForegroundColor Cyan
Write-Host "================================`n" -ForegroundColor Cyan

# Test 1: Health Check
Write-Host "Test 1: Health Check" -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "$apiUrl/health" -Method Get
    Write-Host "✅ Status: $($health.status)" -ForegroundColor Green
    Write-Host "   Server: $($health.server)" -ForegroundColor Gray
    Write-Host "   Version: $($health.version)`n" -ForegroundColor Gray
} catch {
    Write-Host "❌ Failed: $_`n" -ForegroundColor Red
    exit 1
}

# Test 2: Sync Ping
Write-Host "Test 2: Sync Ping" -ForegroundColor Yellow
try {
    $ping = Invoke-RestMethod -Uri "$apiUrl/api/sync/ping" -Method Get
    Write-Host "✅ Status: $($ping.status)" -ForegroundColor Green
    Write-Host "   Timestamp: $($ping.timestamp)`n" -ForegroundColor Gray
} catch {
    Write-Host "❌ Failed: $_`n" -ForegroundColor Red
}

# Test 3: Sync Status
Write-Host "Test 3: Sync Status" -ForegroundColor Yellow
try {
    $status = Invoke-RestMethod -Uri "$apiUrl/api/sync/status" -Method Get
    Write-Host "✅ Online: $($status.isOnline)" -ForegroundColor Green
    Write-Host "   Total Records: $($status.totalSyncRecords)" -ForegroundColor Gray
    Write-Host "   Recent Changes (24h): $($status.recentChanges24h)`n" -ForegroundColor Gray
} catch {
    Write-Host "❌ Failed: $_`n" -ForegroundColor Red
}

# Test 4: Upload Test Change
Write-Host "Test 4: Upload Test Change" -ForegroundColor Yellow
try {
    $testChange = @{
        id = 0
        tableName = "Habits"
        recordId = 999
        operation = "INSERT"
        timestamp = (Get-Date).ToUniversalTime().ToString("o")
        data = '{"Id":999,"Name":"Test Habit from API Test","IsActive":true,"CreatedDate":"2025-10-02T12:00:00Z","LastModifiedDate":"2025-10-02T12:00:00Z"}'
        deviceId = $deviceId
        synced = $false
    }
    
    $changes = @($testChange)
    $json = $changes | ConvertTo-Json -Depth 10
    
    $result = Invoke-RestMethod -Uri "$apiUrl/api/sync/receive-changes" -Method Post `
        -ContentType "application/json" -Body $json
    
    Write-Host "✅ Success: $($result.success)" -ForegroundColor Green
    Write-Host "   Applied Changes: $($result.appliedChanges)" -ForegroundColor Gray
    if ($result.errors) {
        Write-Host "   Errors: $($result.errors)" -ForegroundColor Red
    }
    Write-Host ""
} catch {
    Write-Host "❌ Failed: $_" -ForegroundColor Red
    Write-Host "   Response: $($_.Exception.Response)`n" -ForegroundColor Gray
}

# Test 5: Retrieve Changes
Write-Host "Test 5: Retrieve Changes" -ForegroundColor Yellow
try {
    $since = (Get-Date).AddHours(-1).ToUniversalTime().ToString("o")
    $changes = Invoke-RestMethod -Uri "$apiUrl/api/sync/changes-since/$since" -Method Get
    Write-Host "✅ Retrieved Changes: $($changes.Count)" -ForegroundColor Green
    if ($changes.Count -gt 0) {
        Write-Host "   Latest Change:" -ForegroundColor Gray
        Write-Host "   - Table: $($changes[0].tableName)" -ForegroundColor Gray
        Write-Host "   - Operation: $($changes[0].operation)" -ForegroundColor Gray
        Write-Host "   - Device: $($changes[0].deviceId)" -ForegroundColor Gray
    }
    Write-Host ""
} catch {
    Write-Host "❌ Failed: $_`n" -ForegroundColor Red
}

# Test 6: Upload Multiple Changes
Write-Host "Test 6: Upload Multiple Changes (Batch)" -ForegroundColor Yellow
try {
    $changes = @(
        @{
            id = 0
            tableName = "Categories"
            recordId = 101
            operation = "INSERT"
            timestamp = (Get-Date).ToUniversalTime().ToString("o")
            data = '{"Id":101,"Name":"Test Category","Description":"From API Test"}'
            deviceId = $deviceId
            synced = $false
        },
        @{
            id = 0
            tableName = "DailyHabitEntries"
            recordId = 201
            operation = "INSERT"
            timestamp = (Get-Date).ToUniversalTime().ToString("o")
            data = '{"Id":201,"HabitId":999,"Date":"2025-10-02","IsCompleted":true}'
            deviceId = $deviceId
            synced = $false
        }
    )
    
    $json = $changes | ConvertTo-Json -Depth 10
    $result = Invoke-RestMethod -Uri "$apiUrl/api/sync/receive-changes" -Method Post `
        -ContentType "application/json" -Body $json
    
    Write-Host "✅ Success: $($result.success)" -ForegroundColor Green
    Write-Host "   Applied Changes: $($result.appliedChanges)" -ForegroundColor Gray
    Write-Host ""
} catch {
    Write-Host "❌ Failed: $_`n" -ForegroundColor Red
}

# Test 7: Check Updated Status
Write-Host "Test 7: Check Updated Status" -ForegroundColor Yellow
try {
    $status = Invoke-RestMethod -Uri "$apiUrl/api/sync/status" -Method Get
    Write-Host "✅ Online: $($status.isOnline)" -ForegroundColor Green
    Write-Host "   Total Records: $($status.totalSyncRecords)" -ForegroundColor Gray
    Write-Host "   Last Change: $($status.lastChangeTimestamp)`n" -ForegroundColor Gray
} catch {
    Write-Host "❌ Failed: $_`n" -ForegroundColor Red
}

# Summary
Write-Host "`n================================" -ForegroundColor Cyan
Write-Host "Testing Complete!" -ForegroundColor Cyan
Write-Host "================================`n" -ForegroundColor Cyan

Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Test SignalR connection with a MAUI client"
Write-Host "2. Test multi-device sync scenario"
Write-Host "3. Test offline/online transitions"
Write-Host "4. Test conflict resolution`n"

Write-Host "Device ID used for testing: $deviceId" -ForegroundColor Gray
