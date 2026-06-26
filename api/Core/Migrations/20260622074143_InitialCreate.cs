using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Group = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupLabel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(1023)", maxLength: 1023, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRootAdmin = table.Column<bool>(type: "bit", nullable: false),
                    IsEmployee = table.Column<bool>(type: "bit", nullable: false),
                    SsoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LegalName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    TaxCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    BusinessRegistrationNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    BusinessRegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RepresentativeName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    RepresentativeTitle = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Hotline = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Ward = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    District = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Province = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    BankAccountName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    BankBranch = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Slogan = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreferenceKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DataJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppAttributes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DataType = table.Column<int>(type: "int", maxLength: 64, nullable: false),
                    Required = table.Column<bool>(type: "bit", nullable: false),
                    IsFilterable = table.Column<bool>(type: "bit", nullable: false),
                    IsSearchable = table.Column<bool>(type: "bit", nullable: false),
                    IsUnique = table.Column<bool>(type: "bit", nullable: false),
                    DefaultValue = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    MinValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Pattern = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    DisplayGroup = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Width = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAttributes_AppEntities_AppEntityId",
                        column: x => x.AppEntityId,
                        principalTable: "AppEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppRolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppRolePermissions_AppPermissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "AppPermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppRolePermissions_AppRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AppRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Entity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAudits_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppRoleUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRoleUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppRoleUsers_AppRoles_AppRoleId",
                        column: x => x.AppRoleId,
                        principalTable: "AppRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppRoleUsers_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CloudFolders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CloudFolders_AppUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CloudFolders_CloudFolders_ParentId",
                        column: x => x.ParentId,
                        principalTable: "CloudFolders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CloudResourcePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceType = table.Column<int>(type: "int", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CanView = table.Column<bool>(type: "bit", nullable: false),
                    CanAdd = table.Column<bool>(type: "bit", nullable: false),
                    CanEdit = table.Column<bool>(type: "bit", nullable: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudResourcePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CloudResourcePermissions_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CloudUserStorages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsedBytes = table.Column<long>(type: "bigint", nullable: false),
                    MaxBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudUserStorages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CloudUserStorages_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppAttributeOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppAttributeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OptionLabel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OptionValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAttributeOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAttributeOptions_AppAttributes_AppAttributeId",
                        column: x => x.AppAttributeId,
                        principalTable: "AppAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppAttributeValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppAttributeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ValueText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValueNumber = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ValueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValueBoolean = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAttributeValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAttributeValues_AppAttributes_AppAttributeId",
                        column: x => x.AppAttributeId,
                        principalTable: "AppAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CloudFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    FolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CloudFiles_AppUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CloudFiles_CloudFolders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "CloudFolders",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "AppEntities",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "Icon", "IsDeleted", "IsSystem", "ModifiedAt", "ModifiedBy", "Name" },
                values: new object[] { new Guid("f715f0e5-cdd3-4dac-80eb-0d07098b916d"), "User", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, "Đây là nơi lưu trữ thông tin cá nhân, chức vụ, phòng ban và tài khoản đăng nhập của từng người trong công ty.", null, false, true, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, "Nhân viên" });

            migrationBuilder.InsertData(
                table: "AppPermissions",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "Group", "GroupLabel", "IsDeleted", "ModifiedAt", "ModifiedBy", "Name", "Order", "RequiredCode" },
                values: new object[,]
                {
                    { new Guid("0baa2484-9cc7-4770-bace-91e8777da256"), "USER_DELETE", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cho phép xóa mềm một hoặc nhiều người dùng; hệ thống thường chặn tự xóa chính tài khoản đang đăng nhập.", "USER", "Người dùng", false, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Xóa người dùng", 5, "USER_VIEW" },
                    { new Guid("135536bc-a10d-4c3b-9d25-a76b67c0302a"), "ROLE_PERMISSION_SET", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cho phép gán lại tập quyền cho một vai trò; hệ thống kiểm tra quyền phụ thuộc (RequiredCode) trước khi lưu.", "ROLE", "Vai trò", false, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cập nhật quyền cho vai trò", 11, "ROLE_PERMISSION_VIEW" },
                    { new Guid("16624401-8eb2-4d3a-9c80-a9f4b8c9ed02"), "USER_CREATE", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cho phép tạo tài khoản mới: đặt tên đăng nhập, mật khẩu ban đầu, thông tin cá nhân và cờ như quản trị gốc hoặc nhân viên nếu được cấu hình.", "USER", "Người dùng", false, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Thêm mới người dùng", 3, "USER_VIEW" },
                    { new Guid("1f3d0401-fa31-47f3-bd30-47b643afd82d"), "ROLE_MEMBER_ADD", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cho phép thêm một hoặc nhiều người dùng vào một vai trò đã có, bỏ qua những người đã thuộc vai trò.", "ROLE", "Vai trò", false, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Thêm thành viên vào vai trò", 7, "ROLE_VIEW" },
                    { new Guid("2c2d6d42-349a-4bde-a2ac-10553b54ac4c"), "USER_AVATAR", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cho phép cập nhật tệp ảnh đại diện (tên tệp đã lưu trữ) gắn với hồ sơ người dùng.", "USER", "Người dùng", false, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cập nhật ảnh đại diện người dùng", 7, "USER_VIEW" },
                    { new Guid("352f2e8d-91cd-4f42-a368-1add325ce153"), "ROLE_USER_ROLES_VIEW", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cho phép xem danh sách vai trò đang được gán cho một người dùng cụ thể, kèm thời điểm gán nếu có.", "ROLE", "Vai trò", false, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Xem vai trò của người dùng", 9, "ROLE_VIEW" },
                    { new Guid("3b48f126-a6e7-41d6-8119-4bbe0f2548dd"), "USER_VIEW_ALL", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cho phép lấy toàn bộ người dùng đang hoạt động dưới dạng danh sách đầy đủ, thường dùng cho dropdown, gán vai trò hoặc tích hợp nội bộ.", "USER", "Người dùng", false, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Xem toàn bộ danh sách người dùng (không phân trang)", 2, "USER_VIEW" },
                    { new Guid("4013a204-48d5-4632-937d-abeee0d4e73a"), "ROLE_MEMBER_REMOVE", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cho phép gỡ một hoặc nhiều người dùng khỏi vai trò đã chọn.", "ROLE", "Vai trò", false, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Xóa thành viên khỏi vai trò", 8, "ROLE_VIEW" },
                    { new Guid("42307ab7-027c-496a-a098-b7cd27fb31e5"), "ROLE_CREATE", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cho phép tạo vai trò mới với tên và mô tả; tên vai trò phải duy nhất trong hệ thống.", "ROLE", "Vai trò", false, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Thêm mới vai trò", 3, "ROLE_VIEW" },
                    { new Guid("5aa8dfe7-2824-4330-a0b9-283eef4b1ee7"), "ROLE_ASSIGN_TO_USER", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cho phép thiết lập danh sách vai trò cho một người dùng cụ thể (thay thế toàn bộ vai trò hiện có của họ theo luồng API).", "ROLE", "Vai trò", false, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Gán vai trò cho người dùng", 6, "ROLE_VIEW" },
                    { new Guid("7c024160-a173-4e29-8c8f-0ad2d81f2089"), "ROLE_DELETE", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cho phép xóa mềm vai trò và gỡ các liên kết thành viên–vai trò liên quan trước khi xóa.", "ROLE", "Vai trò", false, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Xóa vai trò", 5, "ROLE_VIEW" },
                    { new Guid("8d64accf-d1c7-447d-84bb-1422d2c775fa"), "ROLE_PERMISSION_VIEW", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cho phép xem toàn bộ danh mục quyền trong hệ thống và danh sách mã quyền (định danh) hiện gán cho từng vai trò.", "ROLE", "Vai trò", false, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Xem danh mục quyền và quyền được gán cho vai trò", 10, "ROLE_VIEW" },
                    { new Guid("ace7debe-3885-4c6b-b264-357be5892672"), "ROLE_UPDATE", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cho phép chỉnh sửa tên và mô tả của vai trò đã tồn tại, tuân thủ kiểm tra trùng tên.", "ROLE", "Vai trò", false, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cập nhật vai trò", 4, "ROLE_VIEW" },
                    { new Guid("c3e8f1a2-6b4d-4e7f-9c0d-2a3b4c5d6e7f"), "SYSTEM_SETTING_COMPANY_INFO", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cho phép xem và cập nhật thông tin đơn vị/doanh nghiệp trên hệ thống (tên, mã số thuế, đại diện pháp luật, liên hệ, ngân hàng…).", "SYSTEM_SETTING", "Cài đặt hệ thống", false, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cài đặt thông tin đơn vị", 1, null },
                    { new Guid("e08debda-5c16-403a-869a-eb5349c37ac1"), "USER_RESET_PASSWORD", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cho phép quản trị viên đặt mật khẩu mới cho người dùng mà không cần biết mật khẩu cũ, dùng khi hỗ trợ hoặc khôi phục truy cập.", "USER", "Người dùng", false, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Đặt lại mật khẩu người dùng", 6, "USER_VIEW" },
                    { new Guid("e5b173a9-3022-406c-8ab5-4745da8e7e85"), "ROLE_VIEW", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cho phép xem danh sách vai trò có phân trang (kèm thành viên tóm tắt nếu giao diện hiển thị) và xem chi tiết một vai trò theo mã định danh.", "ROLE", "Vai trò", false, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Xem danh sách và chi tiết vai trò", 1, null },
                    { new Guid("e74129f3-ea4a-49c2-8371-fc6942888194"), "USER_VIEW", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cho phép xem danh sách tài khoản người dùng có phân trang và tìm kiếm theo tên đăng nhập, họ tên, email hoặc số điện thoại.", "USER", "Người dùng", false, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Xem danh sách người dùng", 1, null },
                    { new Guid("ed09e739-05c0-4f91-9c4e-f13968125d63"), "USER_UPDATE", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cho phép chỉnh sửa hồ sơ người dùng (họ tên, liên hệ, mô tả, cờ quyền hệ thống…) ngoại trừ đổi mật khẩu trực tiếp qua luồng reset.", "USER", "Người dùng", false, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cập nhật thông tin người dùng", 4, "USER_VIEW" },
                    { new Guid("fc1a9fdb-e31a-40a9-9cf9-1be3c714e725"), "ROLE_VIEW_ALL", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Cho phép lấy toàn bộ vai trò để chọn nhanh trong form (không phân trang), ví dụ khi gán vai trò cho người dùng.", "ROLE", "Vai trò", false, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Xem toàn bộ vai trò (lookup)", 2, "ROLE_VIEW" }
                });

            migrationBuilder.InsertData(
                table: "AppUsers",
                columns: new[] { "Id", "Address", "Avatar", "Birthday", "CreatedAt", "CreatedBy", "Description", "Email", "Gender", "IsDeleted", "IsEmployee", "IsRootAdmin", "LastLogin", "ModifiedAt", "ModifiedBy", "Name", "Password", "Phone", "SsoId", "UserName" },
                values: new object[] { new Guid("afd55aae-d6b2-4741-8ada-5131ea70f00d"), "Headquarters", "", null, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, "Seeded root administrator account", "admin@cloud.local", 0, false, true, true, null, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, "System Administrator", "cs80krLdOzJuOkLfDeo4h6uG6CS7dErnbCkhpuBFNkk=", "0000000000", null, "admin" });

            migrationBuilder.InsertData(
                table: "CompanyInfos",
                columns: new[] { "Id", "Address", "BankAccountName", "BankAccountNumber", "BankBranch", "BankName", "BusinessRegistrationDate", "BusinessRegistrationNumber", "Country", "CreatedAt", "CreatedBy", "District", "Email", "Hotline", "IsDeleted", "LegalName", "LogoUrl", "ModifiedAt", "ModifiedBy", "Name", "Note", "PhoneNumber", "PostalCode", "Province", "RepresentativeName", "RepresentativeTitle", "Slogan", "TaxCode", "Ward", "Website" },
                values: new object[] { new Guid("c4f7fa3c-987b-4569-826e-2b2e92fdb236"), "Số 32A/856 Tôn Đức Thắng, Phường Hồng Bàng, Thành phố Hải Phòng, Việt Nam", null, null, null, null, new DateTime(2025, 2, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Việt Nam", new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "hungnd@happyecotech.com", null, false, null, null, new DateTime(2026, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, "CÔNG TY CỔ PHẦN HỢP TÁC VÀ PHÁT TRIỂN HAPPY ECOTECH", null, null, null, "Hải Phòng", "Nguyễn Duy Hùng", null, null, "", null, null });

            migrationBuilder.CreateIndex(
                name: "IX_AppAttributeOptions_AppAttributeId",
                table: "AppAttributeOptions",
                column: "AppAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAttributes_AppEntityId",
                table: "AppAttributes",
                column: "AppEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAttributeValues_AppAttributeId",
                table: "AppAttributeValues",
                column: "AppAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAudits_UserId",
                table: "AppAudits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRolePermissions_PermissionId",
                table: "AppRolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRolePermissions_RoleId",
                table: "AppRolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRoleUsers_AppRoleId",
                table: "AppRoleUsers",
                column: "AppRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRoleUsers_AppUserId",
                table: "AppRoleUsers",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CloudFiles_FolderId",
                table: "CloudFiles",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_CloudFiles_OwnerId",
                table: "CloudFiles",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_CloudFolders_OwnerId",
                table: "CloudFolders",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_CloudFolders_ParentId",
                table: "CloudFolders",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CloudResourcePermissions_UserId",
                table: "CloudResourcePermissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CloudUserStorages_UserId",
                table: "CloudUserStorages",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAttributeOptions");

            migrationBuilder.DropTable(
                name: "AppAttributeValues");

            migrationBuilder.DropTable(
                name: "AppAudits");

            migrationBuilder.DropTable(
                name: "AppRolePermissions");

            migrationBuilder.DropTable(
                name: "AppRoleUsers");

            migrationBuilder.DropTable(
                name: "CloudFiles");

            migrationBuilder.DropTable(
                name: "CloudResourcePermissions");

            migrationBuilder.DropTable(
                name: "CloudUserStorages");

            migrationBuilder.DropTable(
                name: "CompanyInfos");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "AppAttributes");

            migrationBuilder.DropTable(
                name: "AppPermissions");

            migrationBuilder.DropTable(
                name: "AppRoles");

            migrationBuilder.DropTable(
                name: "CloudFolders");

            migrationBuilder.DropTable(
                name: "AppEntities");

            migrationBuilder.DropTable(
                name: "AppUsers");
        }
    }
}
