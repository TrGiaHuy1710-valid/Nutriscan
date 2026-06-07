# Idea tích hợp FoodValidate-service cho NutriScanAI

## Mục tiêu

NutriScanAI hiện đang trộn khá nhiều logic frontend, ASP.NET MVC backend và Flask OCR trong cùng một luồng. Hướng tối ưu là tách rõ:

- Frontend chỉ lo trải nghiệm người dùng, trạng thái màn hình và gọi API.
- Backend NutriScan đóng vai trò API gateway/BFF, quản lý user profile, lịch sử scan, thống kê, đề xuất và điều phối các service ngoài.
- OCR service chỉ nhận ảnh và trả text/dữ liệu dinh dưỡng trích xuất.
- FoodValidate-service nhận dữ liệu OCR đã trích xuất, kiểm chứng món/thực phẩm, chuẩn hóa thông tin và trả điểm tin cậy/cảnh báo.

Điểm quan trọng nhất: người dùng không cần biết phía sau có bao nhiêu service. Họ chỉ cần một trải nghiệm mượt: quét nhanh, thấy kết quả dễ hiểu, biết kết quả đáng tin hay không, và có lý do quay lại mỗi ngày.

## Vấn đề hiện tại

1. Frontend gọi trực tiếp các endpoint MVC và render nhiều HTML bằng JavaScript template string.
2. ASP.NET controller vừa proxy OCR, vừa parse JSON, vừa lưu database.
3. Python OCR trả `raw_text` chưa đúng nghĩa text gốc, làm mất dữ liệu để validate/debug.
4. Chưa có lớp adapter để tích hợp service mới như FoodValidate-service.
5. Luồng người dùng sau khi scan còn mỏng: scan xong là lưu, nhưng chưa có feedback đủ thông minh để họ tin và quay lại.

## Kiến trúc đề xuất

```text
Frontend App
    |
    | REST/JSON
    v
NutriScan API Gateway / BFF (ASP.NET Core)
    |
    |-- User/Profile/History/Stats database
    |
    |-- OcrClient -> Python OCR service
    |
    |-- FoodValidateClient -> FoodValidate-service
    |
    `-- RecommendationEngine -> meal/workout/daily insights
```

Backend ASP.NET nên là điểm vào duy nhất cho frontend. Frontend không gọi trực tiếp Flask OCR hay FoodValidate-service. Như vậy:

- Dễ đổi OCR/FoodValidate endpoint mà không sửa UI.
- Dễ thêm retry, timeout, logging, fallback.
- Dễ bảo vệ API key hoặc config nội bộ.
- Dễ version API contract.

## Luồng scan tối ưu

```text
User chọn/chụp ảnh
    -> Frontend POST /api/scans/analyze
    -> Backend gửi ảnh sang OCR service
    -> OCR trả raw_text + nutrition candidates
    -> Backend gửi raw_text + nutrition candidates sang FoodValidate-service
    -> FoodValidate trả normalized food + confidence + warnings + suggestions
    -> Backend lưu ScanRecord đầy đủ
    -> Frontend hiển thị kết quả + mức tin cậy + hành động tiếp theo
```

Endpoint frontend chỉ cần gọi:

```http
POST /api/scans/analyze
Content-Type: multipart/form-data
```

Response đề xuất:

```json
{
  "scanId": 123,
  "status": "validated",
  "product": {
    "name": "Sữa chua ít đường",
    "brand": "Vinamilk",
    "servingSize": "100g"
  },
  "nutrition": {
    "calories": 95,
    "protein": 3.4,
    "carbs": 14.0,
    "fat": 2.8,
    "sugar": 10.0,
    "sodium": 45.0
  },
  "validation": {
    "confidence": 0.87,
    "level": "high",
    "source": "FoodValidate-service",
    "warnings": [
      "Đường hơi cao so với mục tiêu giảm cân"
    ],
    "suggestions": [
      "Có thể dùng nửa khẩu phần hoặc chọn loại không đường"
    ]
  },
  "dailyImpact": {
    "caloriePercent": 5,
    "proteinPercent": 4,
    "carbPercent": 6,
    "fatPercent": 4
  }
}
```

## Vai trò của FoodValidate-service

FoodValidate-service không nên chỉ là "có hợp lệ không". Nó nên trở thành lớp làm kết quả OCR đáng tin hơn.

Nên nhận:

```json
{
  "rawText": "Nutrition facts...",
  "fileName": "label_scan.jpg",
  "ocrNutrition": {
    "calories": 95,
    "protein": 3.4,
    "carb": 14,
    "fat": 2.8
  },
  "userContext": {
    "goalType": "Lose",
    "dailyCalorieTarget": 1850,
    "allergies": [],
    "dietTags": []
  }
}
```

Nên trả:

```json
{
  "isFoodLabel": true,
  "confidence": 0.87,
  "normalizedName": "Sữa chua ít đường",
  "brand": "Vinamilk",
  "servingSize": "100g",
  "nutrition": {
    "calories": 95,
    "protein": 3.4,
    "carbs": 14,
    "fat": 2.8,
    "sugar": 10,
    "sodium": 45
  },
  "flags": [
    {
      "type": "high_sugar",
      "severity": "medium",
      "message": "Đường hơi cao so với mục tiêu hiện tại"
    }
  ],
  "alternatives": [
    "Sữa chua không đường",
    "Sữa chua Hy Lạp ít béo"
  ]
}
```

## Cách tách code backend

Trong ASP.NET nên tách thành các service nhỏ:

```text
Services/
  Ocr/
    IOcrClient.cs
    PythonOcrClient.cs
    OcrResult.cs
  FoodValidation/
    IFoodValidateClient.cs
    FoodValidateClient.cs
    FoodValidationResult.cs
  Scans/
    IScanAnalysisService.cs
    ScanAnalysisService.cs
```

Controller chỉ còn:

```csharp
[HttpPost("api/scans/analyze")]
public async Task<IActionResult> AnalyzeScan(IFormFile file)
{
    var result = await _scanAnalysisService.AnalyzeAsync(file);
    return Ok(result);
}
```

`ScanAnalysisService` chịu trách nhiệm:

1. Validate file.
2. Gọi OCR.
3. Gọi FoodValidate.
4. Merge kết quả.
5. Lưu database.
6. Trả DTO cho frontend.

## Cách tách frontend

Frontend hiện đang nằm trong Razor views. Có thể tách dần mà không cần rewrite toàn bộ ngay.

Phase đầu:

- Tạo `wwwroot/js/api/nutriscan-api.js` để gom toàn bộ `fetch`.
- Tạo `wwwroot/js/pages/nutrition-page.js` cho logic scan.
- Razor chỉ giữ HTML structure và import JS.
- Không render dữ liệu user/server bằng `innerHTML`; dùng `textContent` hoặc helper escape.

Ví dụ module API:

```js
export async function analyzeScan(file) {
  const formData = new FormData();
  formData.append("file", file);

  const response = await fetch("/api/scans/analyze", {
    method: "POST",
    body: formData
  });

  if (!response.ok) {
    throw new Error("Không thể phân tích ảnh");
  }

  return response.json();
}
```

Phase sau:

- Nếu app lớn hơn, chuyển frontend sang React/Vue/Svelte hoặc Blazor WebAssembly.
- Nếu vẫn dùng Razor, giữ pattern "Razor render shell, JS render state".
- Tách CSS theo component/màn hình.

## Trải nghiệm giữ chân người dùng

Ý tưởng tốt nhất không chỉ là quét nhãn. Điểm giữ chân nằm ở "daily companion": mỗi lần mở app, người dùng thấy hôm nay mình đang tiến gần mục tiêu hơn.

### 1. Kết quả scan phải có mức tin cậy

Sau khi scan, hiển thị:

- Tên thực phẩm đã chuẩn hóa.
- Dinh dưỡng chính.
- Mức tin cậy: Cao / Cần kiểm tra / Không chắc.
- Lý do cảnh báo ngắn gọn.
- Nút "Sửa nhanh" nếu OCR sai.

Điều này làm người dùng tin app hơn vì app không giả vờ lúc nào cũng đúng.

### 2. Mỗi scan nên trả lời câu hỏi "món này có hợp với tôi không?"

Không chỉ hiển thị calories. Hãy hiển thị tác động theo mục tiêu:

- "Phù hợp với mục tiêu giảm cân hôm nay."
- "Đạm tốt, nhưng đường hơi cao."
- "Nếu ăn món này, bạn còn khoảng 620 kcal trong ngày."

FoodValidate-service là nơi tạo dữ liệu nền, backend là nơi cá nhân hóa theo profile.

### 3. Tạo vòng quay lại hằng ngày

Nên có màn hình home dạng daily dashboard:

- Streak theo ngày có scan.
- Calorie/macro còn lại.
- Gợi ý bữa tiếp theo dựa trên phần còn thiếu.
- Lịch sử món hay ăn.
- "Scan lại món quen" trong 1 chạm.

Ví dụ:

```text
Hôm nay bạn còn 520 kcal.
Bạn đang thiếu khoảng 32g protein.
Gợi ý: ức gà + sữa chua không đường, hoặc trứng + salad.
```

### 4. Tạo trạng thái "món quen"

Người dùng thường ăn lặp lại. Sau mỗi scan nên có:

- Lưu vào món quen.
- Thêm nhanh vào hôm nay.
- Chỉnh khẩu phần: 0.5x, 1x, 1.5x, 2x.

Đây là tính năng rất giữ chân vì lần sau người dùng không cần scan lại.

### 5. Tạo feedback sau scan

Sau scan thành công:

- Nếu confidence cao: lưu tự động.
- Nếu confidence trung bình: hỏi "Kết quả này đúng chưa?" với 2-3 trường sửa nhanh.
- Nếu confidence thấp: cho upload/chụp lại, nhưng vẫn giữ OCR text để debug.

## Data model nên mở rộng

`ScanRecord` nên có thêm:

```text
NormalizedProductName
Brand
ServingSize
Sugar
Sodium
ValidationConfidence
ValidationLevel
ValidationWarningsJson
ValidationSource
RawOcrText
CorrectedByUser
MealType
ServingMultiplier
```

Nên tách thêm bảng:

```text
FavoriteFoods
DailyFoodEntries
FoodCorrections
```

Lý do: scan record là sự kiện OCR, còn daily entry là "người dùng thật sự ăn món này". Không nên đồng nhất scan với intake, vì người dùng có thể scan để tham khảo nhưng không ăn.

## Chiến lược fallback

FoodValidate-service không nên làm app chết nếu service lỗi.

Luồng fallback:

1. OCR thành công, FoodValidate lỗi.
2. Backend lưu scan với `validation.level = "unverified"`.
3. Frontend hiển thị "Đã nhận diện, chưa kiểm chứng".
4. Người dùng vẫn có thể sửa/lưu.
5. Backend có thể retry validation sau bằng background job.

Timeout đề xuất:

- OCR: 30-60 giây.
- FoodValidate: 3-8 giây.
- Nếu FoodValidate quá timeout, trả kết quả OCR trước để UI không chờ quá lâu.

## Cấu hình backend

Không hard-code URL trong controller. Dùng `appsettings.json`:

```json
{
  "Services": {
    "Ocr": {
      "BaseUrl": "http://localhost:5000",
      "TimeoutSeconds": 60
    },
    "FoodValidate": {
      "BaseUrl": "http://localhost:5010",
      "TimeoutSeconds": 8
    }
  }
}
```

Đăng ký typed clients:

```csharp
builder.Services.AddHttpClient<IOcrClient, PythonOcrClient>();
builder.Services.AddHttpClient<IFoodValidateClient, FoodValidateClient>();
builder.Services.AddScoped<IScanAnalysisService, ScanAnalysisService>();
```

## Roadmap triển khai

### Phase 1: Dọn đường tích hợp

- Sửa API scan thành `/api/scans/analyze`.
- Tách `OcrClient`.
- Lưu đúng `raw_text` thật từ OCR.
- Tách JS gọi API vào file riêng.
- Fix render an toàn, tránh `innerHTML` với dữ liệu user.

### Phase 2: Gắn FoodValidate-service

- Tạo `IFoodValidateClient`.
- Định nghĩa DTO input/output.
- Gọi FoodValidate sau OCR.
- Merge kết quả validate vào response.
- Lưu confidence, warnings, normalized nutrition vào DB.

### Phase 3: Cá nhân hóa giữ chân

- Tách scan record và daily intake entry.
- Thêm "món quen" và "thêm nhanh hôm nay".
- Thêm daily insight: còn thiếu gì, nên ăn gì tiếp.
- Thêm correction flow để người dùng sửa OCR/validation.

### Phase 4: Nâng trải nghiệm

- Loading theo từng bước: Đang đọc ảnh -> Đang kiểm chứng -> Đang cá nhân hóa.
- Kết quả có confidence badge.
- Gợi ý thay thế theo mục tiêu.
- Streak và lịch sử món quen ở home.

## Kết luận

Hướng tối ưu là giữ ASP.NET Core làm API gateway/BFF, tách OCR và FoodValidate-service thành clients riêng, còn frontend chỉ gọi một API ổn định. Về sản phẩm, không nên dừng ở "quét ra calories"; NutriScanAI nên trở thành trợ lý hằng ngày: kiểm chứng món ăn, giải thích món đó có hợp mục tiêu không, và giúp nhập lại món quen thật nhanh.

Nếu làm đúng hướng này, app sẽ dễ mở rộng service mới, dễ debug, và quan trọng hơn là người dùng có lý do quay lại vì mỗi lần mở app đều nhận được một quyết định ăn uống rõ ràng hơn.
