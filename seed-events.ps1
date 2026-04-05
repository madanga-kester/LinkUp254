# ==================== SEED EVENTS SCRIPT (Debug Version) ====================
param(
    [Parameter(Mandatory=$true)][string]$Email,
    [Parameter(Mandatory=$true)][string]$Password
)

$API_BASE = "http://localhost:5260/api"

Write-Host "Starting event seed (debug mode)..." -ForegroundColor Cyan

# ==================== STEP 1: LOGIN ====================
Write-Host "Authenticating..." -ForegroundColor Yellow
$loginBody = @{ email = $Email; password = $Password } | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$API_BASE/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    if (-not $loginResponse.isSuccess) { throw "Login failed: $($loginResponse.message)" }
    $token = $loginResponse.token
    Write-Host "Authenticated successfully" -ForegroundColor Green
} catch {
    Write-Host "Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# ==================== STEP 2: SHOW AVAILABLE INTERESTS ====================
Write-Host "Fetching interests..." -ForegroundColor Yellow
$interests = (Invoke-RestMethod -Uri "$API_BASE/interest/all").value
Write-Host "Available interests (ID: Name):" -ForegroundColor White
$interests | ForEach-Object { Write-Host "  $($_.id): $($_.name) ($($_.category))" }

# ==================== STEP 3: USE VERIFIED INTEREST IDS ====================
# These IDs are confirmed to exist based on your earlier interest list output
$VALID_INTERESTS = @{
    1 = "Live Music"
    2 = "DJ Sets" 
    3 = "Open Mic Night"
    4 = "Jazz & Blues"
    5 = "Street Food"
    6 = "Wine Tasting"
    7 = "Coffee Culture"
    8 = "Vegan Eats"
    9 = "Startup Meetups"
    10 = "AI & Machine Learning"
    11 = "Web Development"
    12 = "Cybersecurity"
    17 = "Yoga"
    18 = "Meditation"
    19 = "Hiking"
    20 = "Cycling"
    13 = "Contemporary Art"
    14 = "Photography Walks"
    15 = "Street Art"
    16 = "Pottery Classes"
    21 = "Rooftop Bars"
    22 = "Comedy Clubs"
    23 = "Karaoke"
    24 = "Dance Parties"
}

# ==================== STEP 4: DEFINE TEST EVENTS (With Verified IDs) ====================
$testEvents = @(
    @{
        Title = "Nairobi Tech Meetup"
        Description = "AI and startup talks in Nairobi"
        City = "Nairobi"
        Country = "Kenya"
        Location = "iHub, Kilimani"
        StartDate = "2026-05-15T18:00:00Z"
        EndDate = "2026-05-15T21:00:00Z"
        Price = 0
        ImageUrl = "https://via.placeholder.com/400x200?text=Tech"
        InterestIds = @(9, 10, 11)  # Startup Meetups, AI & ML, Web Dev
    },
    @{
        Title = "Sunset Jazz Night"
        Description = "Live jazz with skyline views"
        City = "Nairobi"
        Country = "Kenya"
        Location = "The Alchemist, Westlands"
        StartDate = "2026-05-17T19:00:00Z"
        EndDate = "2026-05-17T23:00:00Z"
        Price = 1500
        ImageUrl = "https://via.placeholder.com/400x200?text=Jazz"
        InterestIds = @(4, 21, 1)  # Jazz & Blues, Rooftop Bars, Live Music
    },
    @{
        Title = "Street Food Festival"
        Description = "Taste East African cuisine"
        City = "Nairobi"
        Country = "Kenya"
        Location = "Uhuru Gardens"
        StartDate = "2026-05-24T11:00:00Z"
        EndDate = "2026-05-24T20:00:00Z"
        Price = 500
        ImageUrl = "https://via.placeholder.com/400x200?text=Food"
        InterestIds = @(5, 7, 8)  # Street Food, Coffee Culture, Vegan Eats
    },
    @{
        Title = "Morning Yoga Karura"
        Description = "Guided yoga in nature"
        City = "Nairobi"
        Country = "Kenya"
        Location = "Karura Forest"
        StartDate = "2026-05-18T07:00:00Z"
        EndDate = "2026-05-18T08:30:00Z"
        Price = 800
        ImageUrl = "https://via.placeholder.com/400x200?text=Yoga"
        InterestIds = @(17, 18, 19)  # Yoga, Meditation, Hiking
    },
    @{
        Title = "Art Exhibition New Voices"
        Description = "Emerging East African artists"
        City = "Nairobi"
        Country = "Kenya"
        Location = "GoDown Arts Centre"
        StartDate = "2026-06-01T10:00:00Z"
        EndDate = "2026-06-01T18:00:00Z"
        Price = 300
        ImageUrl = "https://via.placeholder.com/400x200?text=Art"
        InterestIds = @(13, 14, 15)  # Contemporary Art, Photography Walks, Street Art
    }
)

# ==================== STEP 5: CREATE EVENTS WITH DEBUG OUTPUT ====================
Write-Host "`nCreating $($testEvents.Count) events..." -ForegroundColor Yellow

foreach ($ev in $testEvents) {
    Write-Host "`nTrying: $($ev.Title)" -ForegroundColor White
    Write-Host "  Interest IDs: $($ev.InterestIds -join ', ')"
    
    try {
        $body = $ev | ConvertTo-Json -Depth 10 -Compress
        Write-Host "  Request body (first 200 chars): $($body.Substring(0, [Math]::Min(200, $body.Length)))..."
        
        $response = Invoke-RestMethod -Uri "$API_BASE/events" `
            -Method Post `
            -Headers @{ 
                Authorization = "Bearer $token"
                "Content-Type" = "application/json"
            } `
            -Body $body
        
        if ($response.isSuccess) {
            Write-Host "  SUCCESS: $($response.message)" -ForegroundColor Green
        } else {
            Write-Host "  FAILED: $($response.message)" -ForegroundColor Red
        }
    }
    catch {
        # Print detailed error
        $errorMsg = $_.Exception.Message
        if ($_.Exception.Response) {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $responseBody = $reader.ReadToEnd()
            $reader.Close()
            $errorMsg += "`n  Response body: $responseBody"
        }
        Write-Host "  ERROR: $errorMsg" -ForegroundColor Red
    }
    
    Start-Sleep -Milliseconds 300
}

Write-Host "`nDone! Check your frontend at http://localhost:8080" -ForegroundColor Cyan