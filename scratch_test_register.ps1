$ticks = [DateTime]::UtcNow.Ticks
$testEmail = "test_autoproof_$ticks@gmail.com"

$payload = @{
    hoTen = "Test User AutoProof"
    email = $testEmail
    matKhau = "123456aA@"
    xacNhanMatKhau = "123456aA@"
} | ConvertTo-Json

$urls = @("http://localhost:5000/api/v1/auth/register", "http://localhost:5186/api/v1/auth/register")

foreach ($url in $urls) {
    Write-Host "`n=========================================="
    Write-Host "Testing Register API on: $url"
    Write-Host "Payload: $payload"
    Write-Host "=========================================="

    try {
        $response = Invoke-WebRequest -Uri $url -Method POST -Body $payload -ContentType "application/json" -UseBasicParsing
        Write-Host "`n[PROOF SUCCESS] HTTP STATUS CODE: " $response.StatusCode
        Write-Host "[PROOF SUCCESS] RESPONSE BODY:"
        Write-Host $response.Content
        break
    } catch {
        Write-Host "[RESULT] Attempt on $url failed: " $_.Exception.Message
        if ($_.Exception.Response) {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $body = $reader.ReadToEnd()
            Write-Host "[RESPONSE BODY]: " $body
        }
    }
}
