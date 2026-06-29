# Cloud

Ứng dụng quản lý bán hàng / khách hàng: giao diện **Angular (PWA)** và **API ASP.NET Core** với **SQL Server**, xác thực **JWT**, phân quyền theo **vai trò–mã quyền** chi tiết, kèm **báo cáo tổng hợp** và **gia hạn hợp đồng tự động** bằng tác vụ nền.

---

## Mục lục

1. [Kiến trúc tổng quan](#kiến-trúc-tổng-quan)
2. [Nghiệp vụ (domain) — chi tiết](#nghiệp-vụ-domain--chi-tiết)
3. [Chức năng theo tầng giao diện](#chức-năng-theo-tầng-giao-diện)
4. [Cấu trúc thư mục](#cấu-trúc-thư-mục-rút-gọn)
5. [Yêu cầu môi trường & chạy dự án](#yêu-cầu-môi-trường)
6. [Bảo mật & tài liệu API](#bảo-mật--tài-liệu-api)

---

## Kiến trúc tổng quan

| Thành phần | Công nghệ | Mô tả |
|------------|-----------|--------|
| **Web** | Angular 21, TypeScript, SCSS, Bootstrap 5, `@ng-bootstrap/ng-bootstrap` | SPA; build `web/dist/mini-crm`. Production bật **Service Worker** (`ngsw-config.json`). `manifest.webmanifest` đặt tên ứng dụng *Cloud*. |
| **API** | .NET 8, EF Core 8, SQL Server | REST, Swagger, Serilog, **Quartz** (job nền), EPPlus, DinkToPdf, QRCoder, SkiaSharp, v.v. |
| **Tầng nghiệp vụ** | `api/Application` | Auth, khách hàng, hợp đồng, lịch sử hợp đồng, user/role, báo cáo, master data khách hàng, audit. |
| **Dữ liệu** | `api/Core` | `AppDbContext`, thực thể, **migrations** EF (assembly: `Core`). |

Luồng tổng thể: người dùng đăng nhập → nhận JWT → gọi API theo quyền; dữ liệu chính lưu **SQL Server**; file ảnh / tệp tương tác phục vụ qua thư mục `Data` ánh xạ ra `/Files`, `/avatars`, `/customer-avatars`.

---

## Nghiệp vụ (domain) — chi tiết

Phần này mô tả cách hệ thống **mô hình hóa** bán hàng—chăm sóc khách hàng theo mã nguồn (entity + `Application`).

### 1. Khách hàng (Customer)

- **Thông tin cốt lõi (trường chuẩn):** mã, tên, công ty, MST, điện thoại, email, địa chỉ, giới tính, ngày sinh, tỉnh/TP, ghi chú, Zalo, chức danh, **nguồn khách** (Source), ảnh đại diện.
- **Phân loại & vị trí:**
  - **Loại khách hàng** (`CustomerTypeId`) — danh mục cấu hình được.
  - **Giai đoạn pipeline (trạng thái hiện tại)** (`CustomerStatusId` → `PipelineStage`) — mỗi KH nằm tại **một** bước trên quy trình bán hàng.
  - **Khu vực** (`AreaId` — `Area`, tùy chọn) phục vụ phân vùng địa lý.
- **Lịch sử chuyển trạng thái:** bảng `CustomerStatusHistory` lưu từ giai đoạn nào → giai đoạn nào, thời điểm `ChangedAt`. Dùng cho báo cáo **chuyển đổi tiềm năng → chốt (Won)** trong kỳ.
- **Trường tùy chỉnh (EAV):** giá trị nằm ở `AppAttribute` / `AppAttributeValue` gắn với thực thể `AppEntity` tương ứng module (ví dụ mã `customer`).

### 2. Pipeline bán hàng (PipelineStage)

- Mỗi bước có **tên**, **thứ tự (Order)**, mô tả, **màu** hiển thị.
- **Loại bước** (`EnumPipeStageType`):
  - **Open** — còn trong quy trình (mở).
  - **Won** — đã chốt đơn / thành công.
  - **Lost** — thất bại / không còn theo đuổi.

Pipeline được cấu hình ở **danh mục** (cùng API master data với loại KH, khu vực) và ánh xạ trực tiếp lên từng khách hàng.

### 3. Gán khách hàng cho nhân viên (phạm vi dữ liệu)

- Bảng `UserCustomer` gắn **UserId** ↔ **CustomerId**. Mô hình này hỗ trợ bán theo tổ đội: một khách có thể được gắn với nhiều tài khoản (tùy triển khai UI/API).
- Kết hợp với bộ quyền (ví dụ *xem toàn bộ* vs *xem trong phạm vi*) xác định ai thấy danh sách nào — chi tiết kiểm tra trong `CustomerService` và `SystemPermissions` (mã `CUSTOMER_VIEW` / `CUSTOMER_VIEW_ALL`).

### 4. Tương tác khách hàng (CustomerInteraction)

- Mỗi bản ghi: **khách hàng**, **nhân viên thực hiện** (`EmployeeId` → `AppUser`), **hình thức tương tác** (`EnumInteraction`):
  - Gọi điện, Nhắn tin, Email, Nhắn Zalo, Gọi Zalo, Gặp mặt.
- Nội dung: thời điểm tương tác, nội dung tóm tắt, **ngày hẹn chăm sóc tiếp** (`FollowUpDate`), bước xử lý kế (`NextStep`), file đính kèm (đường dẫn lưu trữ).

Dùng cho **nhật ký chăm sóc** và **báo cáo hiệu suất nhân viên** (số tương tác, số KH distinct trong kỳ).

### 5. Hợp đồng (Contract)

- **Mối quan hệ:** mỗi hợp đồng thuộc **một khách hàng** (`CustomerId`) và có **chủ sở hữu** (`OwnerId` = nhân viên `AppUser`).
- **Số, tên, ngày hiệu lực, ngày hết hạn, giá trị (`Value`)**, tệp mềm ký số nếu có (`FileUrl`).
- **Loại hợp đồng** (`EnumContractType`):
  - **HĐ nguyên tắc (Master)**
  - **Đơn lẻ (Normal)**
  - **Phụ lục (Appendix)** — thường không tính riêng trong một số báo cáo doanh thu tổng hợp (API lọc `Type != Appendix` khi cộng giá trị theo gói quy ước nghiệp vụ).
- **Trạng thái** (`EnumContractStatus`):
  - Còn hiệu lực, Đã hết hạn, Thanh lý, Lưu trữ.
- **Chuỗi phiên bản / quan hệ pháp lý:** `ParentId` (hợp đồng cha), `PreviousId` (hợp đồng trước) — dùng khi tách phụ lục hoặc nối tiếp gia hạn thủ công.
- **Gia hạn tự động cấp hợp đồng:** cờ `IsAutoRenewal`, `RenewalCycle` (số tháng mặc định khi thiếu cấu hình chi tiết), và tập `ContractRenewalSetting` (bật gia hạn, loại gia hạn Phụ lục vs ký lại, số tháng `IntervalMonth`, nhắc trước bao nhiêu ngày, **điều chỉnh giá** theo loại/ giá trị).

**Tệp đính kèm & lịch sử:** `ContractFile` và `ContractHistory` lưu vết thay đổi / tài liệu (phục vụ quyền xem lịch sử và tệp theo mã `CONTRACT_FILE`, `CONTRACT_HISTORY`).

#### Gia hạn tự động (nền, Quartz)

- **Lịch:** mặc định mỗi ngày **lúc 02:00** theo múi giờ **máy chủ** (`0 0 2 * * ?` trong `Program.cs` → `ContractAutoRenewalJob`).
- **Điều kiện chọn hợp đồng cần xử lý (tóm tắt từ `ContractAutoRenewalWorker`):**
  - `IsAutoRenewal = true`, loại **Master hoặc Normal**, **không** có `ParentId` (hợp đồng gốc).
  - `EndDate` đã qua (so với *ngày hiện tại* theo local date).
  - Trạng thái **không** phải Thanh lý / Lưu trữ.
  - Phải tồn tại bản ghi `ContractRenewalSetting` tương ứng (bản cấu hình mới nhất theo hợp đồng). Nếu thiếu, job **bỏ qua** và ghi cảnh báo.
- **Chu kỳ tháng:** ưu tiên `IntervalMonth` trong setting; nếu không hợp lệ thì dùng `RenewalCycle` hợp đồng, mặc định tối thiểu 12 nếu cần.
- **Cách tạo hợp đồng mới:** tùy `RenewalType` — tạo **phụ lục** (logic nội bộ `RenewByAppendixInternalAsync`) hoặc **ký lại** (`RenewByResignInternalAsync`); **điều chỉnh giá** tự động theo `EnumPriceAdjustmentType` và `PriceAdjustmentValue` trong setting.
- Toàn bộ gói cập nhật dùng **giao dịch** và tích hợp với dịch vụ hợp đồng/audit tùy luồng (xem mã nguồn worker).

### 6. Cấu hình hệ thống — trường thông tin (EAV / `AppEntity` + `AppAttribute`)

- Mỗi **module dữ liệu** (thực thể) tương ứng một bản ghi `AppEntity` (mã `Code`, tên, mô tả, biểu tượng, cờ hệ thống).
- **Thuộc tính** `AppAttribute`: kiểu dữ liệu (`EnumDataType`: text, số, ngày, bool, select, multi select…), bắt buộc, thứ tự hiển thị, lọc/tìm kiếm, duy nhất, min/max, regex, giá trị mặc định.
- **Tùy chọn** cho select: `AppAttributeOption` (API có CRUD tùy quyền `SYSTEM_FIELD_OPTION_*`).

Nghiệp vụ: mỗi tổ chức tự **mở rộng biểu mẫu** khách hàng / hợp đồng mà không cần sửa bảng SQL cho từng trường mới (chừng mực tương thích kiểu dữ liệu).

### 7. Danh mục phục vụ khách hàng (Customer Master Data)

Theo `ICustomerMasterDataService`, quản lý **CRUD** (có quyền tương ứng):

- **Loại khách hàng** (`CustomerType`)
- **Giai đoạn pipeline** (`PipelineStage` — tên, thứ tự, màu, loại Open/Won/Lost)
- **Khu vực** (`Area`)

Đây là nền tảng để form khách hàng và báo cáo pipeline thống nhất.

### 8. Báo cáo (logic đo lường)

Các số liệu dưới đây lấy từ `ReportService` (và bộ lọc loại hợp đồng/appendix nếu nêu).

| Báo cáo / API | Nội dung nghiệp vụ |
|---------------|-------------------|
| **Tóm tắt dashboard (kỳ tùy chọn `from`–`to`)** | Số **khách mới tạo** trong kỳ. **Giá trị hợp đồng ký mới** (cộng `Value`, loại trừ phụ lục) với hợp đồng **Active hoặc Expired** có `StartDate` trong kỳ. Số hợp đồng **còn hiệu lực** có **hết hạn trong 30 ngày tới** (cảnh báo gia hạn). Số **khách hàng từng chuyển sang giai đoạn Won** (theo lịch sử trạng thái) trong kỳ, và **tỷ lệ chuyển đổi (%)** = Won / số KH mới (nếu mẫu số > 0). |
| **Doanh thu hợp đồng theo ngày** | Trong khoảng ngày: cộng `Value` theo **ngày tạo bản ghi hợp đồng** (`CreatedAt.Date`), bỏ phụ lục. |
| **Số lượng KH theo giai đoạn pipeline** | Mỗi `PipelineStage` (đã sắp thứ tự): đếm số KH hiện có với `CustomerStatusId` tương ứng; tổng cộng. |
| **Hiệu suất nhân viên (theo kỳ)** | Với từng user: số **tương tác**, số **khách hàng distinct** đã tương tác, **tổng giá trị hợp đồng tạo trong kỳ** (theo `OwnerId`, cộng `Value`, bỏ phụ lục). |
| **Dashboard mobile (theo user đăng nhập)** | **Doanh thu theo ngày trong tuần hiện tại** (HĐ không phải phụ lục, `OwnerId` = user), tổng so với **tuần trước**; **20 tương tác gần nhất** của user đó. |

> Ghi chú: định nghĩa “doanh thu” trong mã dựa trên trường `Value` và thời điểm `CreatedAt` / `StartDate` tùy báo cáo — cần thống nhất với kế toán nội bộ nếu triển khai thực tế (ví dụ đối soát hóa đơn).

### 9. Phân quyền, vai trò & kiểm toán (audit)

- **Vai trò** gom nhiều **quyền** (`AppPermission`); **người dùng** gán nhiều vai trò qua bảng nối. Danh mục quyền đầy đủ mô tả tại `api/Core/Constants/SystemPermissions.cs` (module Khách hàng, Hợp đồng, Người dùng, Vai trò, Trường thông tin, v.v. — từng mã quyền kèm mô tả dài).
- Có cơ chế **phụ thuộc quyền** (ví dụ cần quyền xem cơ bản trước khi cấp quyền sửa/xuất) — bảo đảm cấp quyền theo tầng.
- Dịch vụ `AuditService` ghi lại thao tác theo cấu hình ứng dụng (hỗ trợ truy vết theo mã `CUSTOMER_HISTORY`, `CONTRACT_HISTORY` trên nghiệp vụ).

### 10. Các thực thể bổ trợ (tham chiếu thêm)

Trong `Core/Entities` còn có mô hình phục vụ tác nghiệp mở rộng (lịch thăm, kế hoạch, log…) — UI có thể dùng dần tùy phiên bản. Khi tích hợp, nên đọc thêm từng `Controller` tương ứng.

---

## Chức năng theo tầng giao diện

- **Đăng nhập / đăng xuất** (`/auth/login`, `/auth/logout`); khu vực ứng dụng bảo vệ bằng guard (chưa đăng nhập → chuyển tới login kèm `returnUrl`).
- **Responsive:** `DevicePairOutlet` tách **bản mobile** / **bản desktop** (dashboard, danh sách KH, chi tiết, hợp đồng, tương tác, đăng nhập) — cùng URL, khác component.
- **Các màn hình chính (rút gọn từ `views.route.ts`):** Tổng quan, danh sách & chi tiết khách hàng (các tab hợp đồng / tương tác), **danh mục KH (master data)**, **trường thông tin hệ thống**, **người dùng**, **vai trò**, **danh sách hợp đồng** toàn hệ thống.

**API tương ứng:** `Auth`, `Customers`, `Contracts`, `Users`, `Roles`, `CustomerMasterData`, `System`, `Reports`, `Health` (kèm thông tin worker gia hạn nếu bật).

---

## Cấu trúc thư mục (rút gọn)

```
mini-crm/
├── api/
│   ├── Api/                 # Web API, Controllers, Program, static/SPA
│   ├── Application/         # Dịch vụ nghiệp vụ, báo cáo, workers
│   └── Core/                # EF, entities, migrations
├── web/                     # Angular (project `mini-crm`)
│   ├── src/app/
│   │   ├── views/           # Trang, auth, mobile, dashboard, customers, systems, contracts
│   │   ├── layout/
│   │   └── services/        # auth, env, report, role, layout, …
│   └── public/              # PWA: manifest, icons
└── README.md
```

---

## Yêu cầu môi trường

- **.NET 8 SDK**
- **Node.js (LTS)** + **npm** (tương thích Angular CLI trong `web/package.json`)
- **SQL Server** — cấu hình chuỗi kết nối tại API
- (Tùy triển khai) **MongoDB** hoặc sink log khác nếu cấu hình Serilog / tích hợp ngoài mặc định

### Chạy API

```bash
cd api/Api
dotnet run --launch-profile https
```

Theo `Properties/launchSettings.json`: thường **HTTP** `http://localhost:5123`, **HTTPS** `https://localhost:7089`; Swagger: đường dẫn `swagger`. Cấu hình bí mật qua `appsettings`, **User Secrets** hoặc biến môi trường — **không** commit mật khẩu/khóa JWT.

### Cập nhật cơ sở dữ liệu

Migrations sẵn có trong `api/Core/Migrations`. **Áp dụng schema theo quy trình nội bộ của bạn** (EF `database update` hoặc pipeline CI — không mô tả tạo migration mới ở đây).

### Chạy web (Angular)

```bash
cd web
npm install
npm start
```

Mặc định: `http://localhost:4200/`.

`web/src/app/services/env.service.ts` hiện gán `apiUrl` tới một backend cố định — khi phát triển local, cần **chỉnh lại** hoặc dùng proxy/env để trỏ tới `https://localhost:7089` (hoặc API nội bộ).

### Build production (web)

```bash
cd web
npm run build
```

Kết quả: `web/dist/mini-crm`. Có thể phục vụ qua cấu hình `ClientApp` trên host ASP.NET tùy bạn triển khai.

---

## Bảo mật & tài liệu API

- Xoay thông tin nhạy cảm từng bị lộ; CORS hiện cấu hình rộng — **hạn chế theo origin** trước môi trường công cộng.
- Swagger: mở khi chạy API, xem endpoint và schema (JWT Bearer trong cấu hình OpenAPI).
- Công cụ UI web bổ sung: FullCalendar, ApexCharts, Quill, bản đồ (Leaflet / Google), mask, SweetAlert, — tham số tại `web/package.json`.

## License

(Điền theo chính sách dự án nếu cần.)


## Migration tenant dev
TIMESTAMP=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
SECRET="CHANGE_THIS_HMAC_SECRET"
METHOD="POST"
PATH_URL="/api/system/migration/run"
QUERY="identity=ECOTECH"
BODY=""
CANONICAL="${METHOD}\n${PATH_URL}\n${QUERY}\n${TIMESTAMP}\n${BODY}"
SIGNATURE=$(printf "${CANONICAL}" | openssl dgst -sha256 -hmac "${SECRET}" -binary | base64)

curl -s -X POST "http://localhost:5124${PATH_URL}?${QUERY}" \
  -H "X-Api-Key: EcoControl" \
  -H "X-Timestamp: ${TIMESTAMP}" \
  -H "X-Signature: ${SIGNATURE}" | python3 -m json.tool
