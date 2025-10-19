using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tools.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "TEXT", nullable: false, defaultValue: 50m),
                    OpenAIApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    GeminiApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Token = table.Column<string>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invitations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    OwnerId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebsiteFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ParentFeatureId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebsiteFeatures_WebsiteFeatures_ParentFeatureId",
                        column: x => x.ParentFeatureId,
                        principalTable: "WebsiteFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectMembers",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMembers", x => new { x.ProjectId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ProjectMembers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectMembers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    EstimatedHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedUserId = table.Column<string>(type: "TEXT", nullable: true),
                    CompletionUrl = table.Column<string>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskComments_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "WebsiteFeatures",
                columns: new[] { "Id", "Description", "ParentFeatureId", "Title" },
                values: new object[,]
                {
                    { 1, "Establish the project's foundational characteristics and strategic scope.", null, "Project Foundation & Scope" },
                    { 2, "Select the core blueprint that best matches the project's goals.", null, "Core Website Blueprints" },
                    { 3, "Enhance the project with reusable modules and add-on capabilities.", null, "Common Modules & Add-ons" },
                    { 4, "Ensure non-functional requirements and technical safeguards are in place.", null, "Technical & Foundational Features" },
                    { 5, "Define the overall licensing model for the project.", 1, "Project Licensing & Type" },
                    { 8, "Determine who will access and use the final solution.", 1, "Target Platform" },
                    { 12, "Professional online presence for established organisations.", 2, "Corporate / Business Website" },
                    { 19, "Commerce-ready foundation with catalogue and checkout flows.", 2, "E-commerce Platform" },
                    { 39, "Robust publishing toolkit for editorial teams.", 2, "Blog / Content Platform" },
                    { 50, "Highlight individual projects and personal expertise.", 2, "Personal Portfolio / Showcase" },
                    { 56, "Secure account lifecycle and authentication flows.", 3, "User Authentication System" },
                    { 62, "Support multilingual and multi-region experiences.", 3, "Internationalization (i18n)" },
                    { 66, "Back-office tools for managing static and media assets.", 3, "Content & Media Management" },
                    { 70, "Engagement-focused interactive components.", 3, "Interactive Features" },
                    { 75, "Optimise visibility and capture actionable insights.", 4, "SEO & Analytics" },
                    { 80, "Protect the platform and meet regulatory requirements.", 4, "Security & Compliance" },
                    { 85, "Ensure the platform is fast and future ready.", 4, "Performance & Scalability" },
                    { 6, "Closed-source solution released under a commercial license.", 5, "Proprietary / Commercial Project" },
                    { 7, "Public repository with community collaboration under licenses like MIT or GPL.", 5, "Open-Source Project" },
                    { 9, "Accessible to anyone with an internet connection.", 8, "Public-Facing Website" },
                    { 10, "Restricted to company employees or internal stakeholders.", 8, "Internal Business Application / Intranet" },
                    { 11, "Subscription-based product designed for multi-tenant usage.", 8, "Software as a Service (SaaS) Platform" },
                    { 13, "Hero section with value proposition and clear calls-to-action.", 12, "Professional Homepage" },
                    { 14, "Company history, mission and vision messaging.", 12, "About Us Page" },
                    { 15, "Detailed listing of key offerings and differentiators.", 12, "Services / Products Showcase Page" },
                    { 16, "Profiles and biographies of key personnel.", 12, "Team Members Page" },
                    { 17, "Quotes and feedback from satisfied customers.", 12, "Customer Testimonials Section" },
                    { 18, "Contact form, map and essential contact details.", 12, "Contact Page" },
                    { 20, "Structured catalogue management with rich product data.", 19, "Product Catalog System" },
                    { 25, "Comprehensive purchase workflow for customers.", 19, "Shopping Cart & Checkout" },
                    { 29, "Secure payment processing with third-party providers.", 19, "Payment Gateway Integration" },
                    { 32, "Customer account area for orders and profile settings.", 19, "User Account Management" },
                    { 35, "Enhancements that drive engagement and repeat sales.", 19, "Advanced E-commerce Features" },
                    { 40, "Create, edit and manage long-form content.", 39, "Article / Post Management" },
                    { 43, "Structure and categorise articles for discoverability.", 39, "Content Organization" },
                    { 46, "Interactive tools that keep readers connected.", 39, "Reader Engagement" },
                    { 49, "Dedicated space for contributor biographies.", 39, "Author Profiles & Management" },
                    { 51, "Visual gallery with imagery, video and summaries.", 50, "Project Gallery" },
                    { 52, "Deep dives into selected projects or case studies.", 50, "Detailed Project Pages" },
                    { 53, "Comprehensive biography and resume style information.", 50, "Bio / Resume Page" },
                    { 54, "Summarise competencies and technical proficiencies.", 50, "Skills & Expertise Section" },
                    { 55, "Allow visitors to reach out for collaborations or projects.", 50, "Contact Form for Inquiries" },
                    { 57, "Email and password-based user registration.", 56, "Standard Registration" },
                    { 58, "Support Google, Facebook, GitHub and similar OAuth providers.", 56, "Social Logins" },
                    { 59, "Add multi-step verification for increased security.", 56, "Two-Factor Authentication (2FA)" },
                    { 60, "Self-service password reset workflow.", 56, "Password Reset Functionality" },
                    { 61, "Role-based access control for different audiences.", 56, "User Roles & Permissions System" },
                    { 63, "Toggleable interface supporting multiple languages.", 62, "Multi-Language Support" },
                    { 64, "Display pricing in the visitor's preferred currency.", 62, "Multi-Currency Support" },
                    { 65, "Serve tailored content based on visitor location.", 62, "Region-Specific Content Delivery" },
                    { 67, "Centralised storage for images, videos and documents.", 66, "Media Library" },
                    { 68, "Manage evergreen pages such as Privacy Policy or Terms.", 66, "Static Page Management" },
                    { 69, "Friendly content management interface for non-technical users.", 66, "CMS Interface" },
                    { 71, "Collect subscribers and integrate with tools like Mailchimp.", 70, "Newsletter Subscription Form" },
                    { 72, "Allow users to schedule appointments or demos.", 70, "Booking / Appointment Scheduling System" },
                    { 73, "Connect live chat tools such as Intercom or Crisp.", 70, "Live Chat Integration" },
                    { 74, "Facilitate asynchronous community conversations.", 70, "Forums / Community Discussion Board" },
                    { 76, "Manage meta data, structured URLs and SEO basics.", 75, "On-Page SEO Toolkit" },
                    { 77, "Keep search engines up to date with site structure.", 75, "Automated XML Sitemap Generation" },
                    { 78, "Seamless analytics and tag management connections.", 75, "Integration with Google Analytics & GTM" },
                    { 79, "Add schema markup for enhanced search snippets.", 75, "Structured Data (Schema.org) Markup" },
                    { 81, "Force encrypted connections across the application.", 80, "Enforce SSL (HTTPS)" },
                    { 82, "Mitigate the most common web application vulnerabilities.", 80, "Protection against OWASP Top 10" },
                    { 83, "Offer consent management and data privacy controls.", 80, "GDPR / CCPA Compliance Tools" },
                    { 84, "Routine backups to protect against data loss.", 80, "Automated Backups" },
                    { 86, "Optimised layouts for mobile, tablet and desktop.", 85, "Responsive Design" },
                    { 87, "Deliver visual assets efficiently and responsively.", 85, "Image Optimization & Lazy Loading" },
                    { 88, "Distribute content across global edge locations.", 85, "Content Delivery Network (CDN) Integration" },
                    { 89, "Client and server-side caching for consistently fast loads.", 85, "Advanced Caching Strategy" },
                    { 21, "Grid or list views with filtering and sorting options.", 20, "Product Listing Pages" },
                    { 22, "Image gallery, descriptions, specifications and pricing.", 20, "Product Detail Pages" },
                    { 23, "Hierarchical taxonomy for browsing and discovery.", 20, "Product Categories & Tags" },
                    { 24, "Track stock levels and manage product availability.", 20, "Inventory Management" },
                    { 26, "Seamless product selection and cart management.", 25, "Add to Cart Functionality" },
                    { 27, "Shipping, billing and payment captured across guided steps.", 25, "Multi-step Checkout Process" },
                    { 28, "Allow purchases without mandatory account creation.", 25, "Guest Checkout Option" },
                    { 30, "Integrate leading credit card payment services.", 29, "Credit Card Processing (e.g., Stripe)" },
                    { 31, "Offer PayPal as an alternative payment method.", 29, "PayPal Integration" },
                    { 33, "Customers can review previous orders and live statuses.", 32, "Order History & Status Tracking" },
                    { 34, "Store frequently used delivery and payment information.", 32, "Saved Shipping & Billing Addresses" },
                    { 36, "Enable customers to leave qualitative feedback.", 35, "Product Reviews & Ratings System" },
                    { 37, "Configurable promotions and coupon management.", 35, "Discount Codes & Promotions Engine" },
                    { 38, "Let customers save items for later consideration.", 35, "Wishlist / Favorites Functionality" },
                    { 41, "Full-featured editing experience for content creators.", 40, "Rich Text Editor (WYSIWYG)" },
                    { 42, "Plan content releases ahead of time.", 40, "Scheduled Publishing" },
                    { 44, "Taxonomy-based organisation for browsing content.", 43, "Categories & Tags System" },
                    { 45, "Allow readers to quickly locate relevant content.", 43, "Search Functionality" },
                    { 47, "Threaded discussions with moderation controls.", 46, "Comment System (with moderation)" },
                    { 48, "Easy sharing to major social platforms.", 46, "Social Sharing Buttons" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_UserId",
                table: "ProjectMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskComments_TaskId",
                table: "TaskComments",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskComments_UserId",
                table: "TaskComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProjectId",
                table: "Tasks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteFeatures_ParentFeatureId",
                table: "WebsiteFeatures",
                column: "ParentFeatureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Invitations");

            migrationBuilder.DropTable(
                name: "ProjectMembers");

            migrationBuilder.DropTable(
                name: "TaskComments");

            migrationBuilder.DropTable(
                name: "WebsiteFeatures");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
